import os
import tempfile

try:
    from backend.models.domain import (
        AppTheme,
        AudioFormat,
        DownloadItem,
        DownloadStatus,
        AppSettings,
        VideoQuality,
    )
except ImportError:
    from yt_dlp_gui.backend.models.domain import (
        AppTheme,
        AudioFormat,
        DownloadItem,
        DownloadStatus,
        AppSettings,
        VideoQuality,
    )


def test_app_settings_default_values():
    settings = AppSettings.create_default()
    assert settings.theme == AppTheme.DARK
    assert settings.default_quality == VideoQuality.BEST
    assert settings.default_audio_format == AudioFormat.NONE
    assert settings.max_concurrent_downloads == 3
    assert settings.clipboard_auto_paste is True
    assert settings.download_playlist is False
    assert settings.no_cache_dir is True
    assert settings.no_part_file is True
    assert settings.is_advanced_options_open is False


def test_download_item_path_computation():
    item = DownloadItem(
        url="https://www.youtube.com/watch?v=dQw4w9WgXcQ",
        file_name="sample_video.mp4",
        output_directory=r"C:\Videos" if os.name == "nt" else "/tmp/Videos",
    )

    expected_full = os.path.join(item.output_directory, "sample_video.mp4")
    assert item.full_path == expected_full
    assert item.part_full_path == f"{expected_full}.part"


def test_download_item_serialization_roundtrip():
    item = DownloadItem(
        url="https://www.youtube.com/watch?v=dQw4w9WgXcQ",
        title="Rick Astley - Never Gonna Give You Up",
        file_name="rick.mp4",
        output_directory=r"C:\Downloads" if os.name == "nt" else "/tmp/Downloads",
        status=DownloadStatus.COMPLETED,
        progress_percentage=100.0,
        download_speed="12.5 MiB/s",
        eta="00:00",
        total_size="45.0 MiB",
        status_message="Completed",
    )

    d = item.to_dict()
    assert d["status"] == "Completed"
    assert d["progressPercentage"] == 100.0
    assert d["downloadSpeed"] == "12.5 MiB/s"

    restored = DownloadItem.from_dict(d)
    assert restored.id == item.id
    assert restored.url == item.url
    assert restored.title == item.title
    assert restored.status == DownloadStatus.COMPLETED
    assert restored.progress_percentage == 100.0

