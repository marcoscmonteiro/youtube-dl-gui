import os
try:
    from backend.engine.ytdlp_wrapper import YtDlpEngine, YtDlpLogger
    from backend.models.domain import AudioFormat, DownloadItem, VideoQuality
except ImportError:
    from yt_dlp_gui.backend.engine.ytdlp_wrapper import YtDlpEngine, YtDlpLogger
    from yt_dlp_gui.backend.models.domain import AudioFormat, DownloadItem, VideoQuality


def test_format_helpers():
    assert YtDlpEngine.format_bytes(1048576) == "1.0 MiB"
    assert YtDlpEngine.format_speed(10485760) == "10.0 MiB/s"
    assert YtDlpEngine.format_eta(65) == "01:05"
    assert YtDlpEngine.format_eta(3665) == "01:01:05"


def test_build_format_string():
    engine = YtDlpEngine()

    fmt_4k = engine.build_format_string(VideoQuality.UHD_4K, AudioFormat.NONE)
    assert "2160" in fmt_4k

    fmt_1080p = engine.build_format_string(VideoQuality.FHD_1080P, AudioFormat.NONE)
    assert "1080" in fmt_1080p

    fmt_audio = engine.build_format_string(VideoQuality.BEST, AudioFormat.MP3)
    assert fmt_audio == "bestaudio/best"


def test_cancellation_flags():
    engine = YtDlpEngine()
    item_id = "test_item_123"

    assert not engine.is_cancelled(item_id)
    engine.cancel_download(item_id)
    assert engine.is_cancelled(item_id)
    engine.cleanup_cancel_flag(item_id)
    assert not engine.is_cancelled(item_id)


def test_build_ydl_options_js_runtimes_validity():
    import yt_dlp
    engine = YtDlpEngine()
    item = DownloadItem(
        url="https://www.youtube.com/watch?v=dQw4w9WgXcQ",
        extra_options="--limit-rate 5M",
    )
    logger = YtDlpLogger(item)

    opts = engine.build_ydl_options(
        item=item,
        quality=VideoQuality.BEST,
        audio_format=AudioFormat.NONE,
        download_playlist=False,
        no_cache_dir=True,
        no_part_file=True,
        logger=logger,
        progress_hook=lambda d: None,
    )

    # Must be able to initialize YoutubeDL without ValueError on js_runtimes
    ydl = yt_dlp.YoutubeDL(opts)
    assert ydl is not None
