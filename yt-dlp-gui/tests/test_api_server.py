import asyncio
import json
import shutil
import tempfile
import urllib.error
import urllib.request

try:
    from backend.api.api_server import ApiServer
    from backend.engine.ytdlp_wrapper import YtDlpEngine
    from backend.queue.queue_manager import DownloadQueueManager
    from backend.storage.settings_service import JsonSettingsService
except ImportError:
    from yt_dlp_gui.backend.api.api_server import ApiServer
    from yt_dlp_gui.backend.engine.ytdlp_wrapper import YtDlpEngine
    from yt_dlp_gui.backend.queue.queue_manager import DownloadQueueManager
    from yt_dlp_gui.backend.storage.settings_service import JsonSettingsService


async def test_api_server_endpoints():
    temp_dir = tempfile.mkdtemp(prefix="ytdlp_test_api_")
    test_port = 48197
    try:
        settings_service = JsonSettingsService(temp_dir)
        engine = YtDlpEngine()
        queue_manager = DownloadQueueManager(engine, settings_service)
        server = ApiServer(queue_manager, settings_service, host="127.0.0.1", port=test_port)

        await server.start_async()
        await asyncio.sleep(0.1)

        loop = asyncio.get_running_loop()

        # 1. Test GET /api/ping
        def _ping():
            req = urllib.request.Request(f"http://127.0.0.1:{test_port}/api/ping")
            with urllib.request.urlopen(req, timeout=3.0) as resp:
                assert resp.status == 200
                data = json.loads(resp.read().decode("utf-8"))
                assert data["status"] == "ok"
                assert data["app"] == "YoutubeDlGui"
                assert data["port"] == test_port

        await loop.run_in_executor(None, _ping)

        # 2. Test GET /api/status
        def _status():
            req = urllib.request.Request(f"http://127.0.0.1:{test_port}/api/status")
            with urllib.request.urlopen(req, timeout=3.0) as resp:
                assert resp.status == 200
                data = json.loads(resp.read().decode("utf-8"))
                assert "active" in data
                assert "queued" in data

        await loop.run_in_executor(None, _status)

        # 3. Test POST /api/download (valid request)
        def _download():
            payload = json.dumps({
                "url": "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                "quality": "FHD_1080p",
                "audioOnly": False,
                "downloadDirectory": temp_dir,
                "cookiesText": "# Netscape HTTP Cookie File\n.youtube.com\tTRUE\t/\tTRUE\t1798765432\tSID\tsample",
            }).encode("utf-8")

            req = urllib.request.Request(
                f"http://127.0.0.1:{test_port}/api/download",
                data=payload,
                headers={"Content-Type": "application/json"},
                method="POST"
            )
            with urllib.request.urlopen(req, timeout=3.0) as resp:
                assert resp.status == 200
                data = json.loads(resp.read().decode("utf-8"))
                assert data["success"] is True
                assert "id" in data

        await loop.run_in_executor(None, _download)

        # 4. Test POST /api/download (invalid URL rejection)
        def _invalid_download():
            payload = json.dumps({"url": "invalid-not-http"}).encode("utf-8")
            req = urllib.request.Request(
                f"http://127.0.0.1:{test_port}/api/download",
                data=payload,
                headers={"Content-Type": "application/json"},
                method="POST"
            )
            try:
                urllib.request.urlopen(req, timeout=3.0)
                assert False, "Should have raised HTTP 400"
            except urllib.error.HTTPError as err:
                assert err.code == 400

        await loop.run_in_executor(None, _invalid_download)

        await server.stop_async()
    finally:
        shutil.rmtree(temp_dir, ignore_errors=True)

