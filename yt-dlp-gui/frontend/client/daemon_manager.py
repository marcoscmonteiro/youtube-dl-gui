from __future__ import annotations

import os
import subprocess
import sys
import time
import urllib.error
import urllib.request
from typing import Optional


class DaemonManager:
    @staticmethod
    def is_daemon_running(host: str = "127.0.0.1", port: int = 48190) -> bool:
        try:
            req = urllib.request.Request(f"http://{host}:{port}/api/ping", method="GET")
            with urllib.request.urlopen(req, timeout=1.0) as resp:
                return resp.status == 200
        except Exception:
            return False

    @staticmethod
    def ensure_daemon_started(
        host: str = "127.0.0.1",
        port: int = 48190,
        wait_timeout: float = 6.0,
    ) -> bool:
        if DaemonManager.is_daemon_running(host, port):
            return True

        current_dir = os.path.dirname(os.path.abspath(__file__))
        project_root = os.path.abspath(os.path.join(current_dir, "..", ".."))

        # Launch detached backend daemon
        cmd = [
            sys.executable,
            "-m",
            "backend.main",
            "--host",
            host,
            "--port",
            str(port),
        ]

        kwargs = {
            "cwd": project_root,
            "stdout": subprocess.DEVNULL,
            "stderr": subprocess.DEVNULL,
        }

        if sys.platform == "win32":
            # DETACHED_PROCESS | CREATE_NEW_PROCESS_GROUP
            kwargs["creationflags"] = 0x00000008 | 0x00000200
        else:
            kwargs["start_new_session"] = True

        try:
            subprocess.Popen(cmd, **kwargs)
        except Exception as ex:
            print(f"[DaemonManager] Erro ao iniciar daemon em background: {ex}")
            return False

        # Wait for daemon to respond to ping
        start_time = time.time()
        while time.time() - start_time < wait_timeout:
            if DaemonManager.is_daemon_running(host, port):
                return True
            time.sleep(0.2)

        return DaemonManager.is_daemon_running(host, port)

