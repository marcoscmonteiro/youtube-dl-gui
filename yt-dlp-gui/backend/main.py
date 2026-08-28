from __future__ import annotations

import argparse
import asyncio
import os
import signal
import sys

# Ensure package root is in sys.path
current_dir = os.path.dirname(os.path.abspath(__file__))
project_root = os.path.abspath(os.path.join(current_dir, "..", ".."))
if project_root not in sys.path:
    sys.path.insert(0, project_root)

from backend.api.api_server import ApiServer
from backend.engine.ytdlp_wrapper import YtDlpEngine
from backend.queue.queue_manager import DownloadQueueManager
from backend.storage.settings_service import JsonSettingsService


async def run_daemon(
    host: str = "127.0.0.1",
    port: int = 48190,
    storage_folder: str | None = None,
) -> None:
    settings_service = JsonSettingsService(storage_folder)
    await settings_service.load_async()

    engine = YtDlpEngine()
    queue_manager = DownloadQueueManager(engine, settings_service)
    await queue_manager.initialize_async()

    server = ApiServer(
        queue_manager=queue_manager,
        settings_service=settings_service,
        host=host,
        port=port or settings_service.settings.bridge_port,
    )

    print(f"[yt-dlp-gui daemon] Iniciando servidor em http://{host}:{port}...")
    await server.start_async()
    print(f"[yt-dlp-gui daemon] Servidor ativo e pronto para receber conexões.")
    print(f"[yt-dlp-gui daemon] Endpoints disponíveis: /api/ping, /api/status, /api/download, /ws")

    stop_event = asyncio.Event()

    def _signal_handler() -> None:
        print("\n[yt-dlp-gui daemon] Encerrando serviço...")
        stop_event.set()

    if sys.platform != "win32":
        loop = asyncio.get_running_loop()
        for sig in (signal.SIGINT, signal.SIGTERM):
            loop.add_signal_handler(sig, _signal_handler)

    try:
        await stop_event.wait()
    except (KeyboardInterrupt, SystemExit):
        pass
    finally:
        await server.stop_async()
        print("[yt-dlp-gui daemon] Servidor finalizado com sucesso.")


def main() -> None:
    parser = argparse.ArgumentParser(description="yt-dlp-gui Headless Download Daemon")
    parser.add_argument("--host", default="127.0.0.1", help="Endereço de escuta (padrão: 127.0.0.1)")
    parser.add_argument("--port", type=int, default=48190, help="Porta HTTP/WebSocket (padrão: 48190)")
    parser.add_argument("--storage-dir", default=None, help="Diretório de configurações e histórico")

    args = parser.parse_args()

    try:
        asyncio.run(
            run_daemon(
                host=args.host,
                port=args.port,
                storage_folder=args.storage_dir,
            )
        )
    except KeyboardInterrupt:
        print("\n[yt-dlp-gui daemon] Encerrado pelo usuário.")


if __name__ == "__main__":
    main()

