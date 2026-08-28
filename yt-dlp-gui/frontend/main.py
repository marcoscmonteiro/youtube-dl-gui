from __future__ import annotations

import argparse
import os
import sys

# Ensure project root is in sys.path
current_dir = os.path.dirname(os.path.abspath(__file__))
project_root = os.path.abspath(os.path.join(current_dir, "..", ".."))
if project_root not in sys.path:
    sys.path.insert(0, project_root)

try:
    from PySide6.QtGui import QIcon
    from PySide6.QtWidgets import QApplication
except ImportError:
    print("[yt-dlp-gui] PySide6 não está instalado. Execute: pip install PySide6")
    sys.exit(1)

from frontend.client.backend_client import BackendClient
from frontend.client.daemon_manager import DaemonManager
from frontend.views.main_window import MainWindow


def main() -> None:
    parser = argparse.ArgumentParser(description="yt-dlp-gui Modern Desktop GUI")
    parser.add_argument("--host", default="127.0.0.1", help="Endereço do backend daemon (padrão: 127.0.0.1)")
    parser.add_argument("--port", type=int, default=48190, help="Porta do backend daemon (padrão: 48190)")
    parser.add_argument("--no-auto-daemon", action="store_true", help="Não iniciar o daemon automaticamente")

    args = parser.parse_args()

    app = QApplication(sys.argv)
    app.setApplicationName("yt-dlp GUI")
    app.setOrganizationName("yt-dlp-gui")

    # Set Application Icon if exists
    icon_candidates = [
        os.path.join(current_dir, "assets", "VideoDownload.ico"),
        os.path.join(project_root, "YoutubeDlGui.App", "VideoDownload.ico"),
    ]
    for ic in icon_candidates:
        if os.path.isfile(ic):
            app.setWindowIcon(QIcon(ic))
            break

    # 1. Ensure backend daemon is running
    if not args.no_auto_daemon:
        print("[yt-dlp-gui] Verificando status do backend daemon...")
        started = DaemonManager.ensure_daemon_started(args.host, args.port)
        if not started:
            print("[yt-dlp-gui] Aviso: Não foi possível conectar ao daemon local automaticamente.")

    # 2. Instantiate Backend Client
    client = BackendClient(host=args.host, port=args.port)

    # 3. Create and show main window
    window = MainWindow(client=client)
    window.show()

    sys.exit(app.exec())


if __name__ == "__main__":
    main()

