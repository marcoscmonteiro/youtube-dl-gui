from __future__ import annotations

import asyncio
import json
import os
import subprocess
import sys
import threading
import time
import urllib.error
import urllib.parse
import urllib.request
from typing import Any, Dict, List, Optional

try:
    from PySide6.QtCore import QObject, Signal
except ImportError:
    # Graceful mock for testing outside PySide6 environment
    class QObject:  # type: ignore
        pass

    class Signal:  # type: ignore
        def __init__(self, *args: Any) -> None:
            pass

        def emit(self, *args: Any) -> None:
            pass


class BackendClient(QObject):
    item_added = Signal(dict)
    progress_updated = Signal(dict)
    status_changed = Signal(dict)
    log_line_received = Signal(str, str)  # (item_id, log_line)
    item_removed = Signal(str)  # item_id
    initial_state_received = Signal(dict)
    connection_status_changed = Signal(bool)

    def __init__(self, host: str = "127.0.0.1", port: int = 48190) -> None:
        super().__init__()
        self.host = host
        self.port = port
        self.base_url = f"http://{host}:{port}"
        self._is_running = False
        self._is_connected = False
        self._ws_thread: Optional[threading.Thread] = None

    @property
    def is_connected(self) -> bool:
        return self._is_connected

    def start(self) -> None:
        if self._is_running:
            return
        self._is_running = True
        self._ws_thread = threading.Thread(target=self._ws_worker_loop, daemon=True)
        self._ws_thread.start()

    def stop(self) -> None:
        self._is_running = False

    def ping(self) -> bool:
        try:
            req = urllib.request.Request(f"{self.base_url}/api/ping", method="GET")
            with urllib.request.urlopen(req, timeout=1.5) as resp:
                if resp.status == 200:
                    data = json.loads(resp.read().decode("utf-8"))
                    return data.get("status") == "ok"
        except Exception:
            return False
        return False

    def get_status(self) -> Optional[Dict[str, Any]]:
        try:
            req = urllib.request.Request(f"{self.base_url}/api/status", method="GET")
            with urllib.request.urlopen(req, timeout=3.0) as resp:
                return json.loads(resp.read().decode("utf-8"))
        except Exception:
            return None

    def get_downloads(self) -> List[Dict[str, Any]]:
        try:
            req = urllib.request.Request(f"{self.base_url}/api/downloads", method="GET")
            with urllib.request.urlopen(req, timeout=3.0) as resp:
                return json.loads(resp.read().decode("utf-8"))
        except Exception:
            return []

    def add_download(self, payload: Dict[str, Any]) -> Optional[Dict[str, Any]]:
        try:
            body = json.dumps(payload).encode("utf-8")
            req = urllib.request.Request(
                f"{self.base_url}/api/download",
                data=body,
                headers={"Content-Type": "application/json"},
                method="POST",
            )
            with urllib.request.urlopen(req, timeout=5.0) as resp:
                return json.loads(resp.read().decode("utf-8"))
        except Exception as ex:
            return {"success": False, "error": str(ex)}

    def cancel_download(self, item_id: str) -> None:
        try:
            req = urllib.request.Request(
                f"{self.base_url}/api/downloads/{item_id}/cancel",
                data=b"",
                method="POST",
            )
            with urllib.request.urlopen(req, timeout=3.0):
                pass
        except Exception:
            pass

    def retry_download(self, item_id: str) -> None:
        try:
            req = urllib.request.Request(
                f"{self.base_url}/api/downloads/{item_id}/retry",
                data=b"",
                method="POST",
            )
            with urllib.request.urlopen(req, timeout=3.0):
                pass
        except Exception:
            pass

    def remove_download(self, item_id: str, delete_file: bool = False) -> None:
        try:
            query = "?deleteFile=true" if delete_file else ""
            req = urllib.request.Request(
                f"{self.base_url}/api/downloads/{item_id}{query}",
                method="DELETE",
            )
            with urllib.request.urlopen(req, timeout=3.0):
                pass
        except Exception:
            pass

    def clear_completed(self) -> None:
        try:
            req = urllib.request.Request(
                f"{self.base_url}/api/downloads/clear-completed",
                data=b"",
                method="POST",
            )
            with urllib.request.urlopen(req, timeout=3.0):
                pass
        except Exception:
            pass

    def cancel_all(self) -> None:
        try:
            req = urllib.request.Request(
                f"{self.base_url}/api/downloads/cancel-all",
                data=b"",
                method="POST",
            )
            with urllib.request.urlopen(req, timeout=3.0):
                pass
        except Exception:
            pass

    def get_settings(self) -> Optional[Dict[str, Any]]:
        try:
            req = urllib.request.Request(f"{self.base_url}/api/settings", method="GET")
            with urllib.request.urlopen(req, timeout=3.0) as resp:
                return json.loads(resp.read().decode("utf-8"))
        except Exception:
            return None

    def save_settings(self, settings_dict: Dict[str, Any]) -> bool:
        try:
            body = json.dumps(settings_dict).encode("utf-8")
            req = urllib.request.Request(
                f"{self.base_url}/api/settings",
                data=body,
                headers={"Content-Type": "application/json"},
                method="POST",
            )
            with urllib.request.urlopen(req, timeout=3.0) as resp:
                return resp.status == 200
        except Exception:
            return False

    def get_help(self) -> str:
        try:
            req = urllib.request.Request(f"{self.base_url}/api/help", method="GET")
            with urllib.request.urlopen(req, timeout=10.0) as resp:
                data = json.loads(resp.read().decode("utf-8"))
                return data.get("help", "")
        except Exception as ex:
            return f"Não foi possível obter a ajuda do yt-dlp: {ex}"

    def update_engine(self) -> str:
        try:
            req = urllib.request.Request(
                f"{self.base_url}/api/engine/update",
                data=b"",
                method="POST",
            )
            with urllib.request.urlopen(req, timeout=180.0) as resp:
                data = json.loads(resp.read().decode("utf-8"))
                return data.get("output", "")
        except Exception as ex:
            return f"Erro ao atualizar engine: {ex}"

    # WebSocket connection worker loop
    def _ws_worker_loop(self) -> None:
        asyncio.run(self._async_ws_client_loop())

    async def _async_ws_client_loop(self) -> None:
        import base64
        import hashlib
        import struct

        while self._is_running:
            try:
                reader, writer = await asyncio.open_connection(self.host, self.port)

                # RFC 6455 Handshake
                sec_key = base64.b64encode(os.urandom(16)).decode("utf-8")
                handshake = (
                    f"GET /ws HTTP/1.1\r\n"
                    f"Host: {self.host}:{self.port}\r\n"
                    f"Upgrade: websocket\r\n"
                    f"Connection: Upgrade\r\n"
                    f"Sec-WebSocket-Key: {sec_key}\r\n"
                    f"Sec-WebSocket-Version: 13\r\n\r\n"
                )
                writer.write(handshake.encode("utf-8"))
                await writer.drain()

                # Read handshake response
                response = b""
                while b"\r\n\r\n" not in response:
                    chunk = await reader.read(1024)
                    if not chunk:
                        break
                    response += chunk

                if b"101 Switching Protocols" in response:
                    self._is_connected = True
                    self.connection_status_changed.emit(True)

                    # Read WebSocket frames
                    while self._is_running and not reader.at_eof():
                        header = await reader.readexactly(2)
                        b1, b2 = header[0], header[1]
                        opcode = b1 & 0x0F
                        is_masked = bool(b2 & 0x80)
                        payload_len = b2 & 0x7F

                        if opcode == 0x8:  # Close
                            break

                        if payload_len == 126:
                            data = await reader.readexactly(2)
                            payload_len = struct.unpack("!H", data)[0]
                        elif payload_len == 127:
                            data = await reader.readexactly(8)
                            payload_len = struct.unpack("!Q", data)[0]

                        mask = await reader.readexactly(4) if is_masked else None
                        raw_payload = await reader.readexactly(payload_len)

                        if mask:
                            unmasked = bytes(
                                raw_payload[i] ^ mask[i % 4]
                                for i in range(len(raw_payload))
                            )
                        else:
                            unmasked = raw_payload

                        if opcode == 0x1:  # Text frame
                            text = unmasked.decode("utf-8", errors="ignore")
                            self._handle_ws_event(text)
                        elif opcode == 0x9:  # Ping
                            # Send pong
                            pong_hdr = bytearray([0x8A, len(unmasked)])
                            writer.write(bytes(pong_hdr) + unmasked)
                            await writer.drain()

                writer.close()
                await writer.wait_closed()
            except Exception:
                pass

            self._is_connected = False
            self.connection_status_changed.emit(False)
            await asyncio.sleep(2.0)

    def _handle_ws_event(self, text: str) -> None:
        try:
            msg = json.loads(text)
            event = msg.get("event")
            data = msg.get("data")

            if event == "init_state":
                self.initial_state_received.emit(data)
            elif event == "item_added":
                self.item_added.emit(data)
            elif event == "progress":
                self.progress_updated.emit(data)
            elif event == "status_changed":
                self.status_changed.emit(data)
            elif event == "log_line":
                if isinstance(data, dict):
                    self.log_line_received.emit(
                        data.get("id", ""), data.get("line", "")
                    )
            elif event == "item_removed":
                if isinstance(data, dict):
                    self.item_removed.emit(data.get("id", ""))
        except Exception:
            pass

