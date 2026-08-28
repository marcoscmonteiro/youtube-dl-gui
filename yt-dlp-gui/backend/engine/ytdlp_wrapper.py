from __future__ import annotations

import asyncio
import io
import os
import shutil
import sys
import tempfile
import threading
from typing import Any, Callable, Dict, List, Optional

from backend.models.domain import AudioFormat, DownloadItem, DownloadStatus, VideoQuality
from backend.models.schemas import DownloadProgressReport


class DownloadCancelledException(Exception):
    pass


class YtDlpLogger:
    def __init__(
        self,
        item: DownloadItem,
        log_callback: Optional[Callable[[str], None]] = None,
        progress_callback: Optional[Callable[[DownloadProgressReport], None]] = None,
    ) -> None:
        self.item = item
        self.log_callback = log_callback
        self.progress_callback = progress_callback
        self._lock = threading.Lock()

    def debug(self, msg: str) -> None:
        # Avoid printing debug messages that are progress lines (handled by progress_hooks)
        if msg.startswith("[debug] "):
            self._append_line(msg)
        else:
            self._append_line(f"[yt-dlp] {msg}")

    def info(self, msg: str) -> None:
        self._append_line(f"[yt-dlp] {msg}")
        self._check_status_hints(msg)

    def warning(self, msg: str) -> None:
        self._append_line(f"[WARNING] {msg}")

    def error(self, msg: str) -> None:
        self._append_line(f"[ERROR] {msg}")

    def _append_line(self, line: str) -> None:
        with self._lock:
            if self.item.log:
                self.item.log += f"\n{line}"
            else:
                self.item.log = line

        if self.log_callback:
            self.log_callback(line)

    def _check_status_hints(self, msg: str) -> None:
        if "[ExtractAudio]" in msg:
            self.item.status_message = "Converting audio..."
            if self.progress_callback:
                self.progress_callback(
                    DownloadProgressReport(
                        id=self.item.id,
                        status_text="Converting audio...",
                        raw_log_line=msg,
                    )
                )
        elif "[Merger]" in msg:
            self.item.status_message = "Merging formats..."
            if self.progress_callback:
                self.progress_callback(
                    DownloadProgressReport(
                        id=self.item.id,
                        status_text="Merging formats...",
                        raw_log_line=msg,
                    )
                )


class YtDlpEngine:
    def __init__(self) -> None:
        self._cancelled_items: set[str] = set()
        self._lock = threading.Lock()

    def cancel_download(self, item_id: str) -> None:
        with self._lock:
            self._cancelled_items.add(item_id)

    def is_cancelled(self, item_id: str) -> bool:
        with self._lock:
            return item_id in self._cancelled_items

    def cleanup_cancel_flag(self, item_id: str) -> None:
        with self._lock:
            self._cancelled_items.discard(item_id)

    @staticmethod
    def resolve_quickjs_path() -> Optional[str]:
        qjs = shutil.which("qjs") or shutil.which("qjs.exe")
        if qjs:
            return qjs

        base_dirs = [
            os.path.dirname(sys.executable),
            os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..")),
            os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", "..")),
            os.path.expanduser("~/.local/bin"),
        ]
        app_data = os.environ.get("APPDATA")
        if app_data:
            base_dirs.append(os.path.join(app_data, "YoutubeDlGui"))

        candidates = []
        for bd in base_dirs:
            if bd and os.path.isdir(bd):
                candidates.extend([
                    os.path.join(bd, "qjs.exe"),
                    os.path.join(bd, "qjs"),
                ])

        for c in candidates:
            if os.path.isfile(c):
                return c
        return None

    @staticmethod
    def format_bytes(num_bytes: Optional[float]) -> str:
        if num_bytes is None or num_bytes <= 0:
            return ""
        for unit in ["B", "KiB", "MiB", "GiB", "TiB"]:
            if abs(num_bytes) < 1024.0:
                return f"{num_bytes:3.1f} {unit}"
            num_bytes /= 1024.0
        return f"{num_bytes:.1f} PiB"

    @staticmethod
    def format_speed(speed_bytes_sec: Optional[float]) -> str:
        if speed_bytes_sec is None or speed_bytes_sec <= 0:
            return ""
        return f"{YtDlpEngine.format_bytes(speed_bytes_sec)}/s"

    @staticmethod
    def format_eta(eta_seconds: Optional[int]) -> str:
        if eta_seconds is None or eta_seconds < 0:
            return ""
        m, s = divmod(int(eta_seconds), 60)
        h, m = divmod(m, 60)
        if h > 0:
            return f"{h:02d}:{m:02d}:{s:02d}"
        return f"{m:02d}:{s:02d}"

    def build_format_string(self, quality: VideoQuality, audio_format: AudioFormat) -> Optional[str]:
        if audio_format != AudioFormat.NONE:
            return "bestaudio/best"

        quality_map = {
            VideoQuality.UHD_4K: "bestvideo[height<=?2160]+bestaudio/best[height<=?2160]",
            VideoQuality.QHD_1440P: "bestvideo[height<=?1440]+bestaudio/best[height<=?1440]",
            VideoQuality.FHD_1080P: "bestvideo[height<=?1080]+bestaudio/best[height<=?1080]",
            VideoQuality.HD_720P: "bestvideo[height<=?720]+bestaudio/best[height<=?720]",
            VideoQuality.SD_480P: "bestvideo[height<=?480]+bestaudio/best[height<=?480]",
            VideoQuality.WORST: "worstvideo+worstaudio/worst",
            VideoQuality.BEST: "bestvideo+bestaudio/best",
        }
        return quality_map.get(quality, "bestvideo+bestaudio/best")

    def build_ydl_options(
        self,
        item: DownloadItem,
        quality: VideoQuality,
        audio_format: AudioFormat,
        download_playlist: bool,
        no_cache_dir: bool,
        no_part_file: bool,
        logger: YtDlpLogger,
        progress_hook: Callable[[Dict[str, Any]], None],
        temp_cookie_path: Optional[str] = None,
    ) -> Dict[str, Any]:
        work_dir = item.output_directory or os.getcwd()
        os.makedirs(work_dir, exist_ok=True)

        opts: Dict[str, Any] = {
            "outtmpl": os.path.join(work_dir, "%(title)s [%(id)s].%(ext)s"),
            "logger": logger,
            "progress_hooks": [progress_hook],
            "noplaylist": not download_playlist,
            "nopart": no_part_file,
            "nocache": no_cache_dir,
            "ignoreerrors": False,
            "no_color": True,
            "encoding": "utf-8",
        }

        # Format / Quality
        fmt = self.build_format_string(quality, audio_format)
        if fmt:
            opts["format"] = fmt

        # Audio extraction postprocessor
        if audio_format != AudioFormat.NONE:
            postprocessors: List[Dict[str, Any]] = [
                {
                    "key": "FFmpegExtractAudio",
                    "preferredcodec": (
                        audio_format.value.lower()
                        if audio_format != AudioFormat.BEST_AUDIO
                        else "mp3"
                    ),
                    "preferredquality": "192",
                }
            ]
            opts["postprocessors"] = postprocessors

        # QuickJS runtime if available (correct dictionary format {runtime: {config}})
        qjs_path = self.resolve_quickjs_path()
        if qjs_path and os.path.isfile(qjs_path):
            opts["js_runtimes"] = {"quickjs": {"path": qjs_path}}

        # Proxy
        if item.proxy:
            opts["proxy"] = item.proxy.strip()

        # Cookies
        if temp_cookie_path and os.path.exists(temp_cookie_path):
            opts["cookiefile"] = temp_cookie_path

        # Player Clients extractor-args
        if item.player_clients:
            opts["extractor_args"] = {
                "youtube": {"player_client": item.player_clients.split(",")}
            }

        # Merge custom extra options if provided
        if item.extra_options and item.extra_options.strip():
            try:
                import shlex
                import yt_dlp.options

                extra_args = shlex.split(item.extra_options.strip(), posix=(os.name != "nt"))
                parser = yt_dlp.options.create_parser()
                parsed_opts = parser.parse_args(extra_args)[0]
                parsed_dict = {k: v for k, v in vars(parsed_opts).items() if v is not None}

                # Normalize js_runtimes if passed in extra_options
                if "js_runtimes" in parsed_dict:
                    clean_runtimes = {}
                    raw_rt = parsed_dict["js_runtimes"]
                    if isinstance(raw_rt, list):
                        for rt in raw_rt:
                            if isinstance(rt, str):
                                parts = rt.split(":", 1)
                                name = parts[0]
                                path = parts[1] if len(parts) > 1 else None
                                clean_runtimes[name] = {"path": path} if path else {}
                            elif isinstance(rt, dict):
                                clean_runtimes.update(rt)
                    elif isinstance(raw_rt, dict):
                        clean_runtimes = raw_rt
                    parsed_dict["js_runtimes"] = clean_runtimes

                opts.update(parsed_dict)
            except Exception as ex:
                logger.warning(f"Não foi possível processar opções extras '{item.extra_options}': {ex}")

        return opts

    async def download_async(
        self,
        item: DownloadItem,
        quality: VideoQuality = VideoQuality.BEST,
        audio_format: AudioFormat = AudioFormat.NONE,
        download_playlist: bool = False,
        no_cache_dir: bool = True,
        no_part_file: bool = True,
        progress_callback: Optional[Callable[[DownloadProgressReport], None]] = None,
        log_callback: Optional[Callable[[str], None]] = None,
    ) -> bool:
        try:
            import yt_dlp
        except ImportError:
            error_msg = "[ERROR] Módulo yt-dlp não está instalado no ambiente Python."
            item.log += f"\n{error_msg}"
            item.status_message = "yt-dlp não instalado"
            if log_callback:
                log_callback(error_msg)
            return False

        self.cleanup_cancel_flag(item.id)
        logger = YtDlpLogger(item, log_callback, progress_callback)

        temp_cookie_path = None
        if item.cookies_text:
            try:
                clean_cookies = item.cookies_text.strip()
                if not clean_cookies.startswith(("# Netscape HTTP Cookie File", "# HTTP Cookie File")):
                    clean_cookies = "# Netscape HTTP Cookie File\n" + clean_cookies

                fd, temp_cookie_path = tempfile.mkstemp(prefix="ydl_cookie_", suffix=".txt")
                with os.fdopen(fd, "w", encoding="utf-8") as f:
                    f.write(clean_cookies + "\n")
            except Exception:
                temp_cookie_path = None

        def progress_hook(d: Dict[str, Any]) -> None:
            if self.is_cancelled(item.id):
                raise DownloadCancelledException("Download cancelado pelo usuário.")

            status = d.get("status")
            if status == "downloading":
                downloaded = d.get("downloaded_bytes") or 0
                total = d.get("total_bytes") or d.get("total_bytes_estimate") or 0

                percent = 0.0
                if total > 0:
                    percent = (downloaded / total) * 100.0

                speed_str = self.format_speed(d.get("speed"))
                eta_str = self.format_eta(d.get("eta"))
                total_size_str = self.format_bytes(total)

                filename = d.get("filename", "")
                if filename:
                    base_filename = os.path.basename(filename)
                    if base_filename.endswith(".part"):
                        base_filename = base_filename[:-5]
                    item.file_name = base_filename

                item.progress_percentage = percent
                item.download_speed = speed_str
                item.eta = eta_str
                item.total_size = total_size_str
                item.status_message = "Downloading..."

                if progress_callback:
                    report = DownloadProgressReport(
                        id=item.id,
                        percentage=percent,
                        download_speed=speed_str,
                        eta=eta_str,
                        total_size=total_size_str,
                        status_text="Downloading...",
                        extracted_file_name=item.file_name,
                        raw_log_line=f"[download] {percent:5.1f}% of {total_size_str} at {speed_str} ETA {eta_str}",
                    )
                    progress_callback(report)

            elif status == "finished":
                filename = d.get("filename", "")
                if filename:
                    item.file_name = os.path.basename(filename)
                item.progress_percentage = 100.0
                item.status_message = "Processing..."
                if progress_callback:
                    progress_callback(
                        DownloadProgressReport(
                            id=item.id,
                            percentage=100.0,
                            status_text="Processing...",
                            extracted_file_name=item.file_name,
                            raw_log_line=f"[download] 100% concluído para {item.file_name}",
                        )
                    )

        ydl_opts = self.build_ydl_options(
            item=item,
            quality=quality,
            audio_format=audio_format,
            download_playlist=download_playlist,
            no_cache_dir=no_cache_dir,
            no_part_file=no_part_file,
            logger=logger,
            progress_hook=progress_hook,
            temp_cookie_path=temp_cookie_path,
        )

        loop = asyncio.get_running_loop()

        def _execute_download() -> bool:
            try:
                with yt_dlp.YoutubeDL(ydl_opts) as ydl:
                    info = ydl.extract_info(item.url, download=True)
                    if info and isinstance(info, dict):
                        title = info.get("title")
                        if title and not item.title:
                            item.title = title
                return True
            except DownloadCancelledException:
                logger.info("Download cancelado pelo usuário.")
                return False
            except Exception as ex:
                logger.error(f"Falha no download: {ex}")
                return False

        try:
            success = await loop.run_in_executor(None, _execute_download)
            return success
        finally:
            self.cleanup_cancel_flag(item.id)
            if temp_cookie_path and os.path.exists(temp_cookie_path):
                try:
                    os.remove(temp_cookie_path)
                except OSError:
                    pass

    @staticmethod
    async def get_help_async() -> str:
        try:
            import yt_dlp

            buffer = io.StringIO()
            old_stdout = sys.stdout
            try:
                sys.stdout = buffer
                with yt_dlp.YoutubeDL() as ydl:
                    ydl.parse_options(["--help"])
            finally:
                sys.stdout = old_stdout
            return buffer.getvalue()
        except Exception:
            return "Ajuda do yt-dlp: Utilize as opções padrão do yt-dlp conforme documentação oficial."

