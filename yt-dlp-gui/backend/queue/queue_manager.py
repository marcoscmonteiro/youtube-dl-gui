from __future__ import annotations

import asyncio
from datetime import datetime
import os
from typing import Any, Callable, Dict, List, Optional, Set

from backend.engine.ytdlp_wrapper import YtDlpEngine
from backend.models.domain import (
    AudioFormat,
    DownloadItem,
    DownloadStatus,
    VideoQuality,
)
from backend.models.schemas import DownloadProgressReport, ExternalDownloadRequest
from backend.storage.settings_service import JsonSettingsService


class DownloadQueueManager:
    def __init__(
        self,
        engine: YtDlpEngine,
        settings_service: JsonSettingsService,
    ) -> None:
        self.engine = engine
        self.settings_service = settings_service
        self.items: List[DownloadItem] = []
        self._max_concurrent = settings_service.settings.max_concurrent_downloads or 3
        self._semaphore = asyncio.Semaphore(self._max_concurrent)
        self._listeners: Set[Callable[[str, Any], None]] = set()
        self._tasks: Dict[str, asyncio.Task] = {}
        self._lock = asyncio.Lock()

    @property
    def max_concurrent_downloads(self) -> int:
        return self._max_concurrent

    def set_max_concurrent_downloads(self, value: int) -> None:
        if value < 1:
            value = 1
        self._max_concurrent = value
        self._semaphore = asyncio.Semaphore(self._max_concurrent)
        self.settings_service.settings.max_concurrent_downloads = value

    @property
    def active_downloads_count(self) -> int:
        return sum(
            1
            for i in self.items
            if i.status in (DownloadStatus.DOWNLOADING, DownloadStatus.PROCESSING)
        )

    @property
    def queued_downloads_count(self) -> int:
        return sum(1 for i in self.items if i.status == DownloadStatus.QUEUED)

    @property
    def completed_downloads_count(self) -> int:
        return sum(1 for i in self.items if i.status == DownloadStatus.COMPLETED)

    @property
    def failed_downloads_count(self) -> int:
        return sum(1 for i in self.items if i.status == DownloadStatus.FAILED)

    def subscribe(self, callback: Callable[[str, Any], None]) -> None:
        self._listeners.add(callback)

    def unsubscribe(self, callback: Callable[[str, Any], None]) -> None:
        self._listeners.discard(callback)

    def _broadcast(self, event: str, data: Any) -> None:
        for listener in list(self._listeners):
            try:
                listener(event, data)
            except Exception:
                pass

    async def initialize_async(self) -> None:
        history = await self.settings_service.load_history_async()
        for item in history:
            if not any(existing.id == item.id for existing in self.items):
                self.items.append(item)

    def get_item(self, item_id: str) -> Optional[DownloadItem]:
        for item in self.items:
            if item.id == item_id:
                return item
        return None

    def enqueue_from_request(self, req: ExternalDownloadRequest) -> DownloadItem:
        if not req.url or not req.url.strip():
            raise ValueError("URL não fornecida.")

        url = req.url.strip()
        settings = self.settings_service.settings

        # 1. Output directory
        output_dir = req.download_directory or req.output_directory
        if output_dir:
            output_dir = os.path.expandvars(os.path.expanduser(output_dir.strip()))
        else:
            output_dir = settings.work_dir or str(os.path.expanduser("~/Downloads"))

        # 2. Quality
        quality = VideoQuality.BEST
        if req.quality:
            try:
                quality = VideoQuality(req.quality)
            except ValueError:
                quality = VideoQuality.BEST

        # 3. Audio format
        audio_format = AudioFormat.NONE
        if req.audio_only:
            audio_format = AudioFormat.MP3
            if req.audio_format and req.audio_format != "None":
                try:
                    audio_format = AudioFormat(req.audio_format)
                except ValueError:
                    audio_format = AudioFormat.MP3
        elif req.audio_format and req.audio_format != "None":
            try:
                audio_format = AudioFormat(req.audio_format)
            except ValueError:
                audio_format = AudioFormat.NONE

        item = DownloadItem(
            url=url,
            title=req.title or url,
            output_directory=output_dir,
            status=DownloadStatus.QUEUED,
            status_message="Queued",
            cookies_text=req.cookies_text,
            extra_options=req.extra_options or req.extra_args,
            proxy=req.proxy,
            player_clients=req.player_clients,
        )

        self.items.insert(0, item)
        self._broadcast("item_added", item.to_dict())

        # Start async worker task
        task = asyncio.create_task(
            self._process_queue_item_async(
                item=item,
                quality=quality,
                audio_format=audio_format,
                playlist=req.playlist or False,
                no_cache_dir=settings.no_cache_dir,
                no_part_file=settings.no_part_file,
            )
        )
        self._tasks[item.id] = task

        return item

    def enqueue(
        self,
        item: DownloadItem,
        quality: VideoQuality = VideoQuality.BEST,
        audio_format: AudioFormat = AudioFormat.NONE,
        playlist: bool = False,
    ) -> None:
        item.status = DownloadStatus.QUEUED
        item.status_message = "Queued"
        item.progress_percentage = 0.0

        if item not in self.items:
            self.items.insert(0, item)

        self._broadcast("item_added", item.to_dict())

        settings = self.settings_service.settings
        task = asyncio.create_task(
            self._process_queue_item_async(
                item=item,
                quality=quality,
                audio_format=audio_format,
                playlist=playlist,
                no_cache_dir=settings.no_cache_dir,
                no_part_file=settings.no_part_file,
            )
        )
        self._tasks[item.id] = task

    def cancel(self, item_id: str) -> None:
        item = self.get_item(item_id)
        if not item:
            return

        self.engine.cancel_download(item_id)

        task = self._tasks.get(item_id)
        if task and not task.done():
            task.cancel()

        if item.status in (DownloadStatus.QUEUED, DownloadStatus.DOWNLOADING, DownloadStatus.PROCESSING):
            item.status = DownloadStatus.CANCELLED
            item.status_message = "Cancelled"
            self._broadcast("status_changed", item.to_dict())
            asyncio.create_task(self._persist_history_safe())

    def retry(self, item_id: str) -> None:
        item = self.get_item(item_id)
        if not item or item.status in (DownloadStatus.DOWNLOADING, DownloadStatus.PROCESSING):
            return

        self.enqueue(item)

    def remove(self, item_id: str, delete_file: bool = False) -> None:
        item = self.get_item(item_id)
        if not item:
            return

        self.cancel(item_id)

        if delete_file:
            target_path = item.existing_file_path or item.full_path
            if target_path and os.path.exists(target_path):
                try:
                    os.remove(target_path)
                except OSError:
                    pass
            if item.part_full_path and os.path.exists(item.part_full_path):
                try:
                    os.remove(item.part_full_path)
                except OSError:
                    pass

        self.items.remove(item)
        self._broadcast("item_removed", {"id": item_id})
        asyncio.create_task(self._persist_history_safe())

    def clear_completed(self) -> None:
        to_remove = [
            i
            for i in self.items
            if i.status in (DownloadStatus.COMPLETED, DownloadStatus.CANCELLED, DownloadStatus.FAILED)
        ]
        for item in to_remove:
            self.items.remove(item)
            self._broadcast("item_removed", {"id": item.id})

        asyncio.create_task(self._persist_history_safe())

    def cancel_all(self) -> None:
        for item in list(self.items):
            if item.status in (DownloadStatus.QUEUED, DownloadStatus.DOWNLOADING, DownloadStatus.PROCESSING):
                self.cancel(item.id)

    async def _process_queue_item_async(
        self,
        item: DownloadItem,
        quality: VideoQuality,
        audio_format: AudioFormat,
        playlist: bool,
        no_cache_dir: bool,
        no_part_file: bool,
    ) -> None:
        async with self._semaphore:
            if self.engine.is_cancelled(item.id):
                item.status = DownloadStatus.CANCELLED
                item.status_message = "Cancelled before start"
                self._broadcast("status_changed", item.to_dict())
                return

            item.status = DownloadStatus.DOWNLOADING
            item.status_message = "Downloading..."
            self._broadcast("status_changed", item.to_dict())

            def on_progress(report: DownloadProgressReport) -> None:
                self._broadcast("progress", report.to_dict())

            def on_log(line: str) -> None:
                self._broadcast("log_line", {"id": item.id, "line": line})

            success = False
            try:
                success = await self.engine.download_async(
                    item=item,
                    quality=quality,
                    audio_format=audio_format,
                    download_playlist=playlist,
                    no_cache_dir=no_cache_dir,
                    no_part_file=no_part_file,
                    progress_callback=on_progress,
                    log_callback=on_log,
                )
            except asyncio.CancelledError:
                item.status = DownloadStatus.CANCELLED
                item.status_message = "Cancelled"
                self._broadcast("status_changed", item.to_dict())
                return
            except Exception as ex:
                item.status = DownloadStatus.FAILED
                item.status_message = f"Failed: {ex}"
                self._broadcast("status_changed", item.to_dict())
                return

            if self.engine.is_cancelled(item.id):
                item.status = DownloadStatus.CANCELLED
                item.status_message = "Cancelled"
            elif success:
                item.status = DownloadStatus.COMPLETED
                item.status_message = "Completed"
                item.progress_percentage = 100.0
                item.completed_at = datetime.now().isoformat()
            else:
                item.status = DownloadStatus.FAILED
                item.status_message = "Error downloading"

            self._broadcast("status_changed", item.to_dict())
            await self._persist_history_safe()

    async def _persist_history_safe(self) -> None:
        try:
            await self.settings_service.save_history_async(self.items)
        except Exception:
            pass

    def get_status_summary(self) -> Dict[str, Any]:
        settings = self.settings_service.settings
        downloads_dir = str(os.path.expanduser("~/Downloads"))
        return {
            "status": "ok",
            "app": "YoutubeDlGui",
            "version": "2.0",
            "active": self.active_downloads_count,
            "queued": self.queued_downloads_count,
            "completed": self.completed_downloads_count,
            "failed": self.failed_downloads_count,
            "total": len(self.items),
            "workDir": settings.work_dir,
            "defaultDownloadsFolder": (
                downloads_dir if os.path.isdir(downloads_dir) else settings.work_dir
            ),
            "defaultQuality": settings.default_quality.value,
            "defaultAudioFormat": settings.default_audio_format.value,
        }

