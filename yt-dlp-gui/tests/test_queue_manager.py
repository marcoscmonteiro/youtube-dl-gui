import tempfile
import shutil

try:
    from backend.engine.ytdlp_wrapper import YtDlpEngine
    from backend.models.domain import DownloadItem, DownloadStatus
    from backend.models.schemas import ExternalDownloadRequest
    from backend.queue.queue_manager import DownloadQueueManager
    from backend.storage.settings_service import JsonSettingsService
except ImportError:
    from yt_dlp_gui.backend.engine.ytdlp_wrapper import YtDlpEngine
    from yt_dlp_gui.backend.models.domain import DownloadItem, DownloadStatus
    from yt_dlp_gui.backend.models.schemas import ExternalDownloadRequest
    from yt_dlp_gui.backend.queue.queue_manager import DownloadQueueManager
    from yt_dlp_gui.backend.storage.settings_service import JsonSettingsService


async def test_queue_manager_enqueue_item():
    temp_dir = tempfile.mkdtemp(prefix="ytdlp_test_queue_")
    try:
        settings_service = JsonSettingsService(temp_dir)
        engine = YtDlpEngine()
        queue = DownloadQueueManager(engine, settings_service)

        req = ExternalDownloadRequest(
            url="https://www.youtube.com/watch?v=dQw4w9WgXcQ",
            quality="FHD_1080p",
            download_directory=temp_dir,
        )

        item = queue.enqueue_from_request(req)
        assert item in queue.items
        assert item.status in (DownloadStatus.QUEUED, DownloadStatus.DOWNLOADING)
        assert item.output_directory == temp_dir

        summary = queue.get_status_summary()
        assert summary["total"] >= 1

        # Test cancel
        queue.cancel(item.id)
        assert item.status == DownloadStatus.CANCELLED
    finally:
        shutil.rmtree(temp_dir, ignore_errors=True)

