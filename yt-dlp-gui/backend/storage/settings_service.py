from __future__ import annotations

import asyncio
import json
import os
import sys
import tempfile
from pathlib import Path
from typing import Any, Iterable, List, Optional

from backend.models.domain import AppSettings, DownloadItem


class JsonSettingsService:
    def __init__(self, custom_storage_folder: Optional[str] = None) -> None:
        self.storage_folder = custom_storage_folder or self.resolve_storage_folder()
        self.settings_file_path = os.path.join(self.storage_folder, "settings.json")
        self.history_file_path = os.path.join(self.storage_folder, "history.json")
        self.settings: AppSettings = AppSettings.create_default()
        self._lock = asyncio.Lock()

        if not custom_storage_folder:
            self._migrate_legacy_settings_if_needed()

    @staticmethod
    def resolve_storage_folder() -> str:
        if sys.platform.startswith("win"):
            # 1. Try OneDrive consumer or commercial folders for automatic cloud sync & backup
            one_drive = (
                os.environ.get("OneDriveConsumer")
                or os.environ.get("OneDrive")
                or os.environ.get("OneDriveCommercial")
            )
            if one_drive and os.path.isdir(one_drive):
                return os.path.join(one_drive, "Aplicativos", "YtDlpGui", "Config")

            # 2. Fallback to standard %APPDATA%\YoutubeDlGui
            app_data = os.environ.get("APPDATA")
            if app_data:
                return os.path.join(app_data, "YoutubeDlGui")
            return str(Path.home() / ".youtubedlgui")

        elif sys.platform == "darwin":
            # macOS: ~/Library/Application Support/YoutubeDlGui
            return str(Path.home() / "Library" / "Application Support" / "YoutubeDlGui")
        else:
            # Linux: ~/.config/youtubedlgui or $XDG_CONFIG_HOME/youtubedlgui
            xdg_config = os.environ.get("XDG_CONFIG_HOME")
            if xdg_config and os.path.isdir(xdg_config):
                return os.path.join(xdg_config, "youtubedlgui")
            return str(Path.home() / ".config" / "youtubedlgui")

    def _migrate_legacy_settings_if_needed(self) -> None:
        if not sys.platform.startswith("win"):
            return

        try:
            app_data = os.environ.get("APPDATA")
            if not app_data:
                return

            legacy_app_data_folder = os.path.join(app_data, "YoutubeDlGui")

            if (
                os.path.normpath(self.storage_folder).lower()
                != os.path.normpath(legacy_app_data_folder).lower()
            ):
                legacy_settings = os.path.join(legacy_app_data_folder, "settings.json")
                legacy_history = os.path.join(legacy_app_data_folder, "history.json")

                os.makedirs(self.storage_folder, exist_ok=True)

                if not os.path.exists(self.settings_file_path) and os.path.exists(legacy_settings):
                    with open(legacy_settings, "r", encoding="utf-8") as src, open(
                        self.settings_file_path, "w", encoding="utf-8"
                    ) as dst:
                        dst.write(src.read())

                if not os.path.exists(self.history_file_path) and os.path.exists(legacy_history):
                    with open(legacy_history, "r", encoding="utf-8") as src, open(
                        self.history_file_path, "w", encoding="utf-8"
                    ) as dst:
                        dst.write(src.read())
        except Exception:
            pass

    async def load_async(self) -> AppSettings:
        async with self._lock:
            try:
                if os.path.exists(self.settings_file_path):
                    with open(self.settings_file_path, "r", encoding="utf-8") as f:
                        data = json.load(f)
                    self.settings = AppSettings.from_dict(data)
                else:
                    self.settings = AppSettings.create_default()
                    await self._save_internal(self.settings_file_path, self.settings.to_dict())
            except Exception:
                self.settings = AppSettings.create_default()
            return self.settings

    async def save_async(self) -> None:
        async with self._lock:
            await self._save_internal(self.settings_file_path, self.settings.to_dict())

    async def load_history_async(self) -> List[DownloadItem]:
        async with self._lock:
            try:
                if os.path.exists(self.history_file_path):
                    with open(self.history_file_path, "r", encoding="utf-8") as f:
                        items_data = json.load(f)
                    if isinstance(items_data, list):
                        return [DownloadItem.from_dict(item) for item in items_data]
            except Exception:
                pass
            return []

    async def save_history_async(self, items: Iterable[DownloadItem]) -> None:
        async with self._lock:
            data = [item.to_dict() for item in items]
            await self._save_internal(self.history_file_path, data)

    async def _save_internal(self, file_path: str, data: Any) -> None:
        os.makedirs(self.storage_folder, exist_ok=True)
        content = json.dumps(data, indent=2, ensure_ascii=False)

        # Atomic write via temp file in same directory
        dir_name = os.path.dirname(file_path)
        temp_file = tempfile.NamedTemporaryFile(
            "w", dir=dir_name, delete=False, encoding="utf-8"
        )
        temp_path = temp_file.name
        try:
            temp_file.write(content)
            temp_file.flush()
            os.fsync(temp_file.fileno())
            temp_file.close()

            # Retry replace on Windows in case file is briefly locked
            max_retries = 3
            for i in range(max_retries):
                try:
                    os.replace(temp_path, file_path)
                    break
                except OSError:
                    if i == max_retries - 1:
                        raise
                    await asyncio.sleep(0.05)
        finally:
            if os.path.exists(temp_path):
                try:
                    os.remove(temp_path)
                except OSError:
                    pass

