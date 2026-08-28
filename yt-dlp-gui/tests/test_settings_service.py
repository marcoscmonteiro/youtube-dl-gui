import asyncio
import os
import shutil
import tempfile

try:
    from backend.models.domain import AppSettings, AppTheme, AudioFormat, DownloadItem, VideoQuality
    from backend.storage.settings_service import JsonSettingsService
except ImportError:
    from yt_dlp_gui.backend.models.domain import AppSettings, AppTheme, AudioFormat, DownloadItem, VideoQuality
    from yt_dlp_gui.backend.storage.settings_service import JsonSettingsService


async def test_json_settings_save_and_load():
    temp_dir = tempfile.mkdtemp(prefix="ytdlp_test_config_")
    try:
        service = JsonSettingsService(temp_dir)
        service.settings.extra_options = "--limit-rate 5M --embed-subs"
        service.settings.destination_history = ["/path/1", "/path/2"]
        service.settings.extra_options_history = ["--limit-rate 5M", "--embed-subs"]
        service.settings.max_concurrent_downloads = 5
        service.settings.download_playlist = True
        service.settings.no_cache_dir = False
        service.settings.no_part_file = False
        service.settings.clipboard_auto_paste = False
        service.settings.is_advanced_options_open = True
        service.settings.default_quality = VideoQuality.FHD_1080P
        service.settings.default_audio_format = AudioFormat.MP3

        await service.save_async()

        # Create new service instance and load from same directory
        service2 = JsonSettingsService(temp_dir)
        await service2.load_async()

        assert service2.settings.extra_options == "--limit-rate 5M --embed-subs"
        assert service2.settings.destination_history == ["/path/1", "/path/2"]
        assert service2.settings.max_concurrent_downloads == 5
        assert service2.settings.download_playlist is True
        assert service2.settings.default_quality == VideoQuality.FHD_1080P
        assert service2.settings.default_audio_format == AudioFormat.MP3
    finally:
        shutil.rmtree(temp_dir, ignore_errors=True)


async def test_json_settings_history_save_and_load():
    temp_dir = tempfile.mkdtemp(prefix="ytdlp_test_history_")
    try:
        service = JsonSettingsService(temp_dir)
        items = [
            DownloadItem(url="https://youtube.com/watch?v=1", title="Video 1"),
            DownloadItem(url="https://youtube.com/watch?v=2", title="Video 2"),
        ]

        await service.save_history_async(items)

        service2 = JsonSettingsService(temp_dir)
        loaded = await service2.load_history_async()

        assert len(loaded) == 2
        assert loaded[0].url == "https://youtube.com/watch?v=1"
        assert loaded[1].url == "https://youtube.com/watch?v=2"
    finally:
        shutil.rmtree(temp_dir, ignore_errors=True)

