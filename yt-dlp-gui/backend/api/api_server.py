from __future__ import annotations

import asyncio
import base64
import hashlib
import json
import struct
import urllib.parse
from typing import Any, Dict, List, Optional, Set

from backend.engine.engine_updater import EngineUpdater
from backend.engine.ytdlp_wrapper import YtDlpEngine
from backend.models.domain import AppSettings
from backend.models.schemas import ExternalDownloadRequest
from backend.queue.queue_manager import DownloadQueueManager
from backend.storage.settings_service import JsonSettingsService


class ApiServer:
    def __init__(
        self,
        queue_manager: DownloadQueueManager,
        settings_service: JsonSettingsService,
        host: str = "127.0.0.1",
        port: int = 48190,
    ) -> None:
        self.queue_manager = queue_manager
        self.settings_service = settings_service
        self.host = host
        self.port = port
        self._server: Optional[asyncio.AbstractServer] = None
        self._ws_clients: Set[asyncio.StreamWriter] = set()
        self._is_running = False

    @property
    def is_running(self) -> bool:
        return self._is_running

    async def start_async(self) -> None:
        if self._is_running:
            return

        self._server = await asyncio.start_server(
            self._handle_client,
            self.host,
            self.port,
        )
        self._is_running = True
        self.queue_manager.subscribe(self._on_queue_event)

    async def stop_async(self) -> None:
        if not self._is_running:
            return

        self._is_running = False
        self.queue_manager.unsubscribe(self._on_queue_event)

        for writer in list(self._ws_clients):
            try:
                writer.close()
                await writer.wait_closed()
            except Exception:
                pass
        self._ws_clients.clear()

        if self._server:
            self._server.close()
            await self._server.wait_closed()
            self._server = None

    def _on_queue_event(self, event: str, data: Any) -> None:
        payload = json.dumps({"event": event, "data": data})
        for writer in list(self._ws_clients):
            try:
                self._send_ws_text(writer, payload)
            except Exception:
                self._ws_clients.discard(writer)

    async def _handle_client(
        self, reader: asyncio.StreamReader, writer: asyncio.StreamWriter
    ) -> None:
        try:
            while self._is_running and not reader.at_eof():
                request_line = await reader.readline()
                if not request_line:
                    break

                line_str = request_line.decode("utf-8", errors="ignore").strip()
                if not line_str:
                    continue

                parts = line_str.split(" ")
                if len(parts) < 2:
                    break

                method = parts[0].upper()
                raw_path = parts[1]

                # Parse headers
                headers: Dict[str, str] = {}
                while True:
                    header_line = await reader.readline()
                    if not header_line or header_line == b"\r\n" or header_line == b"\n":
                        break
                    h_str = header_line.decode("utf-8", errors="ignore").strip()
                    if ":" in h_str:
                        k, v = h_str.split(":", 1)
                        headers[k.strip().lower()] = v.strip()

                # Read body if Content-Length provided
                content_length = int(headers.get("content-length", 0))
                body = b""
                if content_length > 0:
                    body = await reader.readexactly(content_length)

                # WebSocket Upgrade Request
                if (
                    headers.get("upgrade", "").lower() == "websocket"
                    and "sec-websocket-key" in headers
                ):
                    await self._handle_ws_upgrade(headers, reader, writer)
                    return

                # HTTP Request Routing
                await self._route_http_request(method, raw_path, headers, body, writer)

                # Check connection keep-alive vs close
                if headers.get("connection", "").lower() == "close":
                    break

        except (asyncio.IncompleteReadError, ConnectionResetError, BrokenPipeError):
            pass
        except Exception:
            pass
        finally:
            try:
                writer.close()
                await writer.wait_closed()
            except Exception:
                pass

    async def _route_http_request(
        self,
        method: str,
        raw_path: str,
        headers: Dict[str, str],
        body: bytes,
        writer: asyncio.StreamWriter,
    ) -> None:
        parsed = urllib.parse.urlparse(raw_path)
        path = parsed.path.rstrip("/") or "/"
        query = urllib.parse.parse_qs(parsed.query)

        # CORS Preflight
        if method == "OPTIONS":
            self._send_http_response(writer, 204, b"", "text/plain")
            return

        # 1. GET /api/ping or GET /
        if method == "GET" and (path in ("/", "/api/ping")):
            res = {
                "status": "ok",
                "app": "YoutubeDlGui",
                "version": "2.0",
                "port": self.port,
            }
            self._send_json_response(writer, 200, res)
            return

        # 2. GET /api/status
        if method == "GET" and path == "/api/status":
            summary = self.queue_manager.get_status_summary()
            self._send_json_response(writer, 200, summary)
            return

        # 3. GET /api/downloads
        if method == "GET" and path == "/api/downloads":
            items = [item.to_dict() for item in self.queue_manager.items]
            self._send_json_response(writer, 200, items)
            return

        # 4. POST /api/download
        if method == "POST" and path == "/api/download":
            try:
                data = json.loads(body.decode("utf-8")) if body else {}
                req = ExternalDownloadRequest.from_dict(data)

                if not req.url or not (
                    req.url.startswith("http://") or req.url.startswith("https://")
                ):
                    self._send_json_response(
                        writer,
                        400,
                        {
                            "success": False,
                            "message": "URL inválida ou não fornecida. Deve iniciar com http:// ou https://",
                        },
                    )
                    return

                item = self.queue_manager.enqueue_from_request(req)
                self._send_json_response(
                    writer,
                    200,
                    {
                        "success": True,
                        "message": "Download adicionado à fila com sucesso!",
                        "url": req.url,
                        "id": item.id,
                        "item": item.to_dict(),
                    },
                )
                return
            except Exception as ex:
                self._send_json_response(
                    writer, 500, {"success": False, "error": str(ex)}
                )
                return

        # 5. POST /api/downloads/{id}/cancel
        if method == "POST" and path.startswith("/api/downloads/") and path.endswith("/cancel"):
            item_id = path[len("/api/downloads/") : -len("/cancel")]
            self.queue_manager.cancel(item_id)
            self._send_json_response(writer, 200, {"success": True, "id": item_id})
            return

        # 6. POST /api/downloads/{id}/retry
        if method == "POST" and path.startswith("/api/downloads/") and path.endswith("/retry"):
            item_id = path[len("/api/downloads/") : -len("/retry")]
            self.queue_manager.retry(item_id)
            self._send_json_response(writer, 200, {"success": True, "id": item_id})
            return

        # 7. DELETE /api/downloads/{id}
        if method == "DELETE" and path.startswith("/api/downloads/"):
            item_id = path[len("/api/downloads/") :]
            delete_file = query.get("deleteFile", ["false"])[0].lower() == "true"
            self.queue_manager.remove(item_id, delete_file=delete_file)
            self._send_json_response(writer, 200, {"success": True, "id": item_id})
            return

        # 8. POST /api/downloads/clear-completed
        if method == "POST" and path == "/api/downloads/clear-completed":
            self.queue_manager.clear_completed()
            self._send_json_response(writer, 200, {"success": True})
            return

        # 9. POST /api/downloads/cancel-all
        if method == "POST" and path == "/api/downloads/cancel-all":
            self.queue_manager.cancel_all()
            self._send_json_response(writer, 200, {"success": True})
            return

        # 10. GET /api/help
        if method == "GET" and path == "/api/help":
            help_text = await YtDlpEngine.get_help_async()
            self._send_json_response(writer, 200, {"help": help_text})
            return

        # 11. POST /api/engine/update
        if method == "POST" and path == "/api/engine/update":
            log_output = await EngineUpdater.update_engine_async()
            self._send_json_response(writer, 200, {"success": True, "output": log_output})
            return

        # 12. GET /api/settings and POST /api/settings
        if path == "/api/settings":
            if method == "GET":
                self._send_json_response(
                    writer, 200, self.settings_service.settings.to_dict()
                )
                return
            elif method == "POST":
                try:
                    data = json.loads(body.decode("utf-8")) if body else {}
                    self.settings_service.settings = AppSettings.from_dict(data)
                    self.queue_manager.set_max_concurrent_downloads(
                        self.settings_service.settings.max_concurrent_downloads
                    )
                    await self.settings_service.save_async()
                    self._send_json_response(writer, 200, {"success": True})
                    return
                except Exception as ex:
                    self._send_json_response(writer, 400, {"error": str(ex)})
                    return

        # Not Found
        self._send_json_response(writer, 404, {"error": "Endpoint não encontrado."})

    def _send_json_response(
        self, writer: asyncio.StreamWriter, status_code: int, data: Any
    ) -> None:
        body = json.dumps(data, indent=2, ensure_ascii=False).encode("utf-8")
        self._send_http_response(writer, status_code, body, "application/json; charset=utf-8")

    def _send_http_response(
        self,
        writer: asyncio.StreamWriter,
        status_code: int,
        body: bytes,
        content_type: str,
    ) -> None:
        status_messages = {
            200: "OK",
            204: "No Content",
            400: "Bad Request",
            404: "Not Found",
            500: "Internal Server Error",
        }
        msg = status_messages.get(status_code, "OK")
        header = (
            f"HTTP/1.1 {status_code} {msg}\r\n"
            f"Content-Type: {content_type}\r\n"
            f"Content-Length: {len(body)}\r\n"
            f"Access-Control-Allow-Origin: *\r\n"
            f"Access-Control-Allow-Methods: GET, POST, DELETE, OPTIONS\r\n"
            f"Access-Control-Allow-Headers: Content-Type, Authorization, X-Requested-With\r\n"
            f"Connection: keep-alive\r\n\r\n"
        )
        try:
            writer.write(header.encode("utf-8") + body)
        except Exception:
            pass

    # WebSocket Implementation (RFC 6455)
    async def _handle_ws_upgrade(
        self,
        headers: Dict[str, str],
        reader: asyncio.StreamReader,
        writer: asyncio.StreamWriter,
    ) -> None:
        sec_key = headers["sec-websocket-key"]
        guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
        accept_val = base64.b64encode(
            hashlib.sha1((sec_key + guid).encode("utf-8")).digest()
        ).decode("utf-8")

        response = (
            "HTTP/1.1 101 Switching Protocols\r\n"
            "Upgrade: websocket\r\n"
            "Connection: Upgrade\r\n"
            f"Sec-WebSocket-Accept: {accept_val}\r\n\r\n"
        )
        writer.write(response.encode("utf-8"))
        await writer.drain()

        self._ws_clients.add(writer)

        # Send initial full state
        initial_state = {
            "event": "init_state",
            "data": {
                "items": [item.to_dict() for item in self.queue_manager.items],
                "summary": self.queue_manager.get_status_summary(),
                "settings": self.settings_service.settings.to_dict(),
            },
        }
        self._send_ws_text(writer, json.dumps(initial_state))

        try:
            while self._is_running and not reader.at_eof():
                header = await reader.readexactly(2)
                b1, b2 = header[0], header[1]

                opcode = b1 & 0x0F
                is_masked = bool(b2 & 0x80)
                payload_len = b2 & 0x7F

                if opcode == 0x8:  # Connection close
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
                        raw_payload[i] ^ mask[i % 4] for i in range(len(raw_payload))
                    )
                else:
                    unmasked = raw_payload

                if opcode == 0x1:  # Text frame
                    text = unmasked.decode("utf-8", errors="ignore")
                    await self._handle_ws_client_message(writer, text)
                elif opcode == 0x9:  # Ping
                    self._send_ws_pong(writer, unmasked)

        except Exception:
            pass
        finally:
            self._ws_clients.discard(writer)

    def _send_ws_text(self, writer: asyncio.StreamWriter, text: str) -> None:
        payload = text.encode("utf-8")
        length = len(payload)

        header = bytearray([0x81])  # FIN=1, Text Opcode=0x1
        if length <= 125:
            header.append(length)
        elif length <= 65535:
            header.append(126)
            header.extend(struct.pack("!H", length))
        else:
            header.append(127)
            header.extend(struct.pack("!Q", length))

        try:
            writer.write(bytes(header) + payload)
        except Exception:
            self._ws_clients.discard(writer)

    def _send_ws_pong(self, writer: asyncio.StreamWriter, payload: bytes) -> None:
        header = bytearray([0x8A, len(payload)])  # FIN=1, Pong Opcode=0xA
        try:
            writer.write(bytes(header) + payload)
        except Exception:
            self._ws_clients.discard(writer)

    async def _handle_ws_client_message(
        self, writer: asyncio.StreamWriter, text: str
    ) -> None:
        try:
            msg = json.loads(text)
            action = msg.get("action")
            action_id = msg.get("id")

            if action == "cancel" and action_id:
                self.queue_manager.cancel(action_id)
            elif action == "retry" and action_id:
                self.queue_manager.retry(action_id)
            elif action == "remove" and action_id:
                delete_file = bool(msg.get("deleteFile", False))
                self.queue_manager.remove(action_id, delete_file=delete_file)
            elif action == "clear_completed":
                self.queue_manager.clear_completed()
            elif action == "cancel_all":
                self.queue_manager.cancel_all()
        except Exception:
            pass

