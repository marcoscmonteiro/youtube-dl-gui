from __future__ import annotations

from dataclasses import dataclass, field
from typing import Any, Dict, List, Optional


@dataclass
class ExternalDownloadRequest:
    url: str
    title: Optional[str] = None
    quality: Optional[str] = "Best"
    audio_only: Optional[bool] = False
    audio_format: Optional[str] = "None"
    playlist: Optional[bool] = False
    download_directory: Optional[str] = None
    output_directory: Optional[str] = None
    cookies_text: Optional[str] = None
    extra_options: Optional[str] = None
    extra_args: Optional[str] = None
    proxy: Optional[str] = None
    player_clients: Optional[str] = None
    extractor_args: Optional[str] = None

    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> ExternalDownloadRequest:
        audio_only = data.get("audioOnly")
        if audio_only is None:
            audio_only = data.get("audio_only", False)

        playlist = data.get("playlist", False)

        download_dir = data.get("downloadDirectory") or data.get("download_directory") or data.get("outputDirectory") or data.get("output_directory")
        cookies = data.get("cookiesText") or data.get("cookies_text")
        extra_opts = data.get("extraOptions") or data.get("extra_options") or data.get("extraArgs") or data.get("extra_args")
        player_clients = data.get("playerClients") or data.get("player_clients")

        extractor_args = data.get("extractorArgs") or data.get("extractor_args")
        if not player_clients and extractor_args and extractor_args.startswith("youtube:player_client="):
            player_clients = extractor_args[len("youtube:player_client="):]

        return cls(
            url=data.get("url", ""),
            title=data.get("title"),
            quality=data.get("quality", "Best"),
            audio_only=bool(audio_only),
            audio_format=data.get("audioFormat") or data.get("audio_format") or "None",
            playlist=bool(playlist),
            download_directory=download_dir,
            output_directory=download_dir,
            cookies_text=cookies,
            extra_options=extra_opts,
            extra_args=extra_opts,
            proxy=data.get("proxy"),
            player_clients=player_clients,
            extractor_args=extractor_args,
        )

    def to_dict(self) -> Dict[str, Any]:
        return {
            "url": self.url,
            "title": self.title,
            "quality": self.quality,
            "audioOnly": self.audio_only,
            "audioFormat": self.audio_format,
            "playlist": self.playlist,
            "downloadDirectory": self.download_directory,
            "cookiesText": self.cookies_text,
            "extraOptions": self.extra_options,
            "proxy": self.proxy,
            "playerClients": self.player_clients,
        }


@dataclass
class DownloadProgressReport:
    id: str
    percentage: float = 0.0
    download_speed: str = ""
    eta: str = ""
    total_size: str = ""
    status_text: str = ""
    extracted_file_name: str = ""
    raw_log_line: str = ""

    def to_dict(self) -> Dict[str, Any]:
        return {
            "id": self.id,
            "percentage": self.percentage,
            "downloadSpeed": self.download_speed,
            "eta": self.eta,
            "totalSize": self.total_size,
            "statusText": self.status_text,
            "extractedFileName": self.extracted_file_name,
            "rawLogLine": self.raw_log_line,
        }


@dataclass
class StatusSummaryResponse:
    active: int
    queued: int
    completed: int
    failed: int
    total: int
    work_dir: str
    default_downloads_folder: str
    default_quality: str
    default_audio_format: str
    app: str = "YoutubeDlGui"
    version: str = "2.0"
    status: str = "ok"

    def to_dict(self) -> Dict[str, Any]:
        return {
            "status": self.status,
            "app": self.app,
            "version": self.version,
            "active": self.active,
            "queued": self.queued,
            "completed": self.completed,
            "failed": self.failed,
            "total": self.total,
            "workDir": self.work_dir,
            "defaultDownloadsFolder": self.default_downloads_folder,
            "defaultQuality": self.default_quality,
            "defaultAudioFormat": self.default_audio_format,
        }


@dataclass
class WsEvent:
    event: str
    data: Any

    def to_dict(self) -> Dict[str, Any]:
        return {
            "event": self.event,
            "data": self.data,
        }


@dataclass
class WsAction:
    action: str
    id: Optional[str] = None
    data: Optional[Dict[str, Any]] = None

    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> WsAction:
        return cls(
            action=data.get("action", ""),
            id=data.get("id"),
            data=data.get("data"),
        )

