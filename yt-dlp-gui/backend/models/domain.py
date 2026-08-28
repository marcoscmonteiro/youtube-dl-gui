from __future__ import annotations

import os
import uuid
from dataclasses import dataclass, field
from datetime import datetime
from enum import Enum
from pathlib import Path
from typing import List, Optional


class DownloadStatus(str, Enum):
    QUEUED = "Queued"
    DOWNLOADING = "Downloading"
    PROCESSING = "Processing"
    COMPLETED = "Completed"
    FAILED = "Failed"
    CANCELLED = "Cancelled"


class VideoQuality(str, Enum):
    BEST = "Best"
    UHD_4K = "UHD_4K"
    QHD_1440P = "QHD_1440p"
    FHD_1080P = "FHD_1080p"
    HD_720P = "HD_720p"
    SD_480P = "SD_480p"
    WORST = "Worst"


class AudioFormat(str, Enum):
    NONE = "None"
    BEST_AUDIO = "BestAudio"
    MP3 = "Mp3"
    M4A = "M4a"
    AAC = "Aac"
    FLAC = "Flac"
    WAV = "Wav"
    OPUS = "Opus"
    VORBIS = "Vorbis"


class AppTheme(str, Enum):
    SYSTEM = "System"
    DARK = "Dark"
    LIGHT = "Light"


@dataclass
class DownloadItem:
    url: str
    id: str = field(default_factory=lambda: uuid.uuid4().hex)
    title: str = ""
    file_name: str = ""
    output_directory: str = ""
    command_line_arguments: str = ""
    status: DownloadStatus = DownloadStatus.QUEUED
    progress_percentage: float = 0.0
    download_speed: str = ""
    eta: str = ""
    total_size: str = ""
    status_message: str = "Queued"
    log: str = ""
    created_at: str = field(default_factory=lambda: datetime.now().isoformat())
    completed_at: Optional[str] = None
    temporary_cookie_file_path: Optional[str] = None
    cookies_text: Optional[str] = None
    extra_options: Optional[str] = None
    proxy: Optional[str] = None
    player_clients: Optional[str] = None

    def __post_init__(self) -> None:
        if not self.title:
            self.title = self.url
        if isinstance(self.status, str):
            try:
                self.status = DownloadStatus(self.status)
            except ValueError:
                self.status = DownloadStatus.QUEUED

    @property
    def full_path(self) -> str:
        if self.output_directory and self.file_name:
            return os.path.join(self.output_directory, self.file_name)
        return ""

    @property
    def part_full_path(self) -> str:
        fp = self.full_path
        return f"{fp}.part" if fp else ""

    @property
    def existing_file_path(self) -> Optional[str]:
        if self.full_path and os.path.exists(self.full_path):
            return self.full_path

        if self.part_full_path and os.path.exists(self.part_full_path):
            return self.part_full_path

        if self.output_directory and os.path.isdir(self.output_directory) and self.file_name:
            base_name = Path(self.file_name).stem
            if base_name:
                try:
                    for entry in os.scandir(self.output_directory):
                        if (
                            entry.is_file()
                            and entry.name.startswith(base_name)
                            and not entry.name.endswith((".part", ".ytdl"))
                        ):
                            return entry.path
                except OSError:
                    pass
        return None

    @property
    def file_exists(self) -> bool:
        path = self.existing_file_path
        return bool(path and os.path.exists(path))

    def to_dict(self) -> dict:
        return {
            "id": self.id,
            "url": self.url,
            "title": self.title,
            "fileName": self.file_name,
            "outputDirectory": self.output_directory,
            "commandLineArguments": self.command_line_arguments,
            "status": self.status.value,
            "progressPercentage": self.progress_percentage,
            "downloadSpeed": self.download_speed,
            "eta": self.eta,
            "totalSize": self.total_size,
            "statusMessage": self.status_message,
            "log": self.log,
            "createdAt": self.created_at,
            "completedAt": self.completed_at,
            "cookiesText": self.cookies_text,
            "extraOptions": self.extra_options,
            "proxy": self.proxy,
            "playerClients": self.player_clients,
            "fullPath": self.full_path,
            "fileExists": self.file_exists,
            "existingFilePath": self.existing_file_path,
        }

    @classmethod
    def from_dict(cls, data: dict) -> DownloadItem:
        status_val = data.get("status", DownloadStatus.QUEUED.value)
        try:
            status = DownloadStatus(status_val)
        except ValueError:
            status = DownloadStatus.QUEUED

        return cls(
            id=data.get("id", uuid.uuid4().hex),
            url=data.get("url", ""),
            title=data.get("title", ""),
            file_name=data.get("fileName", data.get("file_name", "")),
            output_directory=data.get("outputDirectory", data.get("output_directory", "")),
            command_line_arguments=data.get("commandLineArguments", data.get("command_line_arguments", "")),
            status=status,
            progress_percentage=float(data.get("progressPercentage", data.get("progress_percentage", 0.0))),
            download_speed=data.get("downloadSpeed", data.get("download_speed", "")),
            eta=data.get("eta", ""),
            total_size=data.get("totalSize", data.get("total_size", "")),
            status_message=data.get("statusMessage", data.get("status_message", "Queued")),
            log=data.get("log", ""),
            created_at=data.get("createdAt", data.get("created_at", datetime.now().isoformat())),
            completed_at=data.get("completedAt", data.get("completed_at")),
            cookies_text=data.get("cookiesText", data.get("cookies_text")),
            extra_options=data.get("extraOptions", data.get("extra_options")),
            proxy=data.get("proxy"),
            player_clients=data.get("playerClients", data.get("player_clients")),
        )


@dataclass
class AppSettings:
    work_dir: str = ""
    destination_history: List[str] = field(default_factory=list)
    extra_options: str = ""
    extra_options_history: List[str] = field(default_factory=list)
    theme: AppTheme = AppTheme.DARK
    engine_executable: str = "yt-dlp"
    clipboard_auto_paste: bool = True
    max_concurrent_downloads: int = 3
    default_quality: VideoQuality = VideoQuality.BEST
    default_audio_format: AudioFormat = AudioFormat.NONE
    download_playlist: bool = False
    no_cache_dir: bool = True
    no_part_file: bool = True
    use_ffplay: bool = False
    is_advanced_options_open: bool = False
    bridge_port: int = 48190
    enable_browser_integration: bool = True
    window_width: float = 1040.0
    window_height: float = 720.0
    window_top: Optional[float] = None
    window_left: Optional[float] = None

    @classmethod
    def create_default(cls) -> AppSettings:
        home = str(Path.home())
        videos_dir = str(Path.home() / "Videos")
        downloads_dir = str(Path.home() / "Downloads")

        default_work_dir = videos_dir if os.path.isdir(videos_dir) else home
        destinations = [default_work_dir]
        if os.path.isdir(downloads_dir) and downloads_dir not in destinations:
            destinations.append(downloads_dir)

        return cls(
            work_dir=default_work_dir,
            destination_history=destinations,
            extra_options="",
            extra_options_history=[],
            theme=AppTheme.DARK,
            engine_executable="yt-dlp",
            default_quality=VideoQuality.BEST,
            default_audio_format=AudioFormat.NONE,
            max_concurrent_downloads=3,
            clipboard_auto_paste=True,
            download_playlist=False,
            no_cache_dir=True,
            no_part_file=True,
            is_advanced_options_open=False,
            bridge_port=48190,
            enable_browser_integration=True,
            window_width=1040.0,
            window_height=720.0,
        )

    def to_dict(self) -> dict:
        return {
            "workDir": self.work_dir,
            "destinationHistory": self.destination_history,
            "extraOptions": self.extra_options,
            "extraOptionsHistory": self.extra_options_history,
            "theme": self.theme.value,
            "engineExecutable": self.engine_executable,
            "clipboardAutoPaste": self.clipboard_auto_paste,
            "maxConcurrentDownloads": self.max_concurrent_downloads,
            "defaultQuality": self.default_quality.value,
            "defaultAudioFormat": self.default_audio_format.value,
            "downloadPlaylist": self.download_playlist,
            "noCacheDir": self.no_cache_dir,
            "noPartFile": self.no_part_file,
            "useFfplay": self.use_ffplay,
            "isAdvancedOptionsOpen": self.is_advanced_options_open,
            "bridgePort": self.bridge_port,
            "enableBrowserIntegration": self.enable_browser_integration,
            "windowWidth": self.window_width,
            "windowHeight": self.window_height,
            "windowTop": self.window_top,
            "windowLeft": self.window_left,
        }

    @classmethod
    def from_dict(cls, data: dict) -> AppSettings:
        default = cls.create_default()

        theme_str = data.get("theme", default.theme.value)
        try:
            theme = AppTheme(theme_str)
        except ValueError:
            theme = default.theme

        qual_str = data.get("defaultQuality", default.default_quality.value)
        try:
            quality = VideoQuality(qual_str)
        except ValueError:
            quality = default.default_quality

        audio_str = data.get("defaultAudioFormat", default.default_audio_format.value)
        try:
            audio_format = AudioFormat(audio_str)
        except ValueError:
            audio_format = default.default_audio_format

        return cls(
            work_dir=data.get("workDir", default.work_dir),
            destination_history=data.get("destinationHistory", default.destination_history),
            extra_options=data.get("extraOptions", default.extra_options),
            extra_options_history=data.get("extraOptionsHistory", default.extra_options_history),
            theme=theme,
            engine_executable=data.get("engineExecutable", default.engine_executable),
            clipboard_auto_paste=bool(data.get("clipboardAutoPaste", default.clipboard_auto_paste)),
            max_concurrent_downloads=int(data.get("maxConcurrentDownloads", default.max_concurrent_downloads)),
            default_quality=quality,
            default_audio_format=audio_format,
            download_playlist=bool(data.get("downloadPlaylist", default.download_playlist)),
            no_cache_dir=bool(data.get("noCacheDir", default.no_cache_dir)),
            no_part_file=bool(data.get("noPartFile", default.no_part_file)),
            use_ffplay=bool(data.get("useFfplay", default.use_ffplay)),
            is_advanced_options_open=bool(data.get("isAdvancedOptionsOpen", default.is_advanced_options_open)),
            bridge_port=int(data.get("bridgePort", default.bridge_port)),
            enable_browser_integration=bool(data.get("enableBrowserIntegration", default.enable_browser_integration)),
            window_width=float(data.get("windowWidth", default.window_width)),
            window_height=float(data.get("windowHeight", default.window_height)),
            window_top=data.get("windowTop"),
            window_left=data.get("windowLeft"),
        )

