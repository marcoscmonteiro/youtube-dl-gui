from __future__ import annotations

import os
import subprocess
import sys
from typing import Any, Callable, Dict, Optional

try:
    from PySide6.QtCore import Qt, QUrl
    from PySide6.QtGui import QDesktopServices
    from PySide6.QtWidgets import (
        QFrame,
        QGraphicsDropShadowEffect,
        QHBoxLayout,
        QLabel,
        QMessageBox,
        QProgressBar,
        QPushButton,
        QSizePolicy,
        QVBoxLayout,
        QWidget,
    )
except ImportError:
    # Graceful mock when PySide6 is not yet loaded in development
    QFrame = QWidget = object  # type: ignore


class DownloadCard(QFrame):
    def __init__(
        self,
        item_data: Dict[str, Any],
        on_cancel: Callable[[str], None],
        on_retry: Callable[[str], None],
        on_remove: Callable[[str, bool], None],
        on_view_log: Callable[[str], None],
        parent: Optional[QWidget] = None,
    ) -> None:
        super().__init__(parent)
        self.setObjectName("DownloadCard")
        self.item_data = item_data
        self.item_id = item_data.get("id", "")
        self.on_cancel = on_cancel
        self.on_retry = on_retry
        self.on_remove = on_remove
        self.on_view_log = on_view_log

        self._setup_ui()
        self.update_data(item_data)

    def _setup_ui(self) -> None:
        layout = QVBoxLayout(self)
        layout.setContentsMargins(12, 10, 12, 10)
        layout.setSpacing(6)

        # 1. Top Row: Badge, Title & Filename, Actions Toolbar
        top_row = QHBoxLayout()
        top_row.setSpacing(10)

        self.badge_label = QLabel("Queued")
        self.badge_label.setObjectName("BadgeQueued")
        top_row.addWidget(self.badge_label)

        title_box = QVBoxLayout()
        title_box.setSpacing(2)

        self.title_label = QLabel("")
        self.title_label.setStyleSheet("font-weight: 600; font-size: 13px;")
        self.title_label.setTextInteractionFlags(Qt.TextSelectableByMouse)
        title_box.addWidget(self.title_label)

        self.filename_label = QLabel("")
        self.filename_label.setObjectName("TextMuted")
        self.filename_label.setTextInteractionFlags(Qt.TextSelectableByMouse)
        title_box.addWidget(self.filename_label)

        top_row.addLayout(title_box, 1)

        # Action Buttons
        self.btn_play = QPushButton("▶ Reproduzir")
        self.btn_play.setObjectName("ActionButton")
        self.btn_play.clicked.connect(self._play_file)
        top_row.addWidget(self.btn_play)

        self.btn_folder = QPushButton("📁 Pasta")
        self.btn_folder.setObjectName("ActionButton")
        self.btn_folder.clicked.connect(self._open_folder)
        top_row.addWidget(self.btn_folder)

        self.btn_log = QPushButton("📋 Log")
        self.btn_log.setObjectName("ActionButton")
        self.btn_log.clicked.connect(lambda: self.on_view_log(self.item_id))
        top_row.addWidget(self.btn_log)

        self.btn_retry = QPushButton("🔄 Repetir")
        self.btn_retry.setObjectName("ActionButton")
        self.btn_retry.clicked.connect(lambda: self.on_retry(self.item_id))
        top_row.addWidget(self.btn_retry)

        self.btn_cancel = QPushButton("⏹ Cancelar")
        self.btn_cancel.setObjectName("ActionButton")
        self.btn_cancel.clicked.connect(lambda: self.on_cancel(self.item_id))
        top_row.addWidget(self.btn_cancel)

        self.btn_delete_file = QPushButton("🗑 Excluir arquivo")
        self.btn_delete_file.setObjectName("ActionButton")
        self.btn_delete_file.clicked.connect(self._delete_file_from_disk)
        top_row.addWidget(self.btn_delete_file)

        self.btn_remove = QPushButton("❌")
        self.btn_remove.setObjectName("ActionButton")
        self.btn_remove.setToolTip("Remover da lista")
        self.btn_remove.clicked.connect(lambda: self.on_remove(self.item_id, False))
        top_row.addWidget(self.btn_remove)

        layout.addLayout(top_row)

        # 2. Middle Row: Progress Bar
        self.progress_bar = QProgressBar()
        self.progress_bar.setRange(0, 1000)
        self.progress_bar.setValue(0)
        self.progress_bar.setTextVisible(False)
        layout.addWidget(self.progress_bar)

        # 3. Bottom Row: Status Message, Speed, Total Size, ETA
        bottom_row = QHBoxLayout()
        self.status_msg_label = QLabel("Queued")
        self.status_msg_label.setObjectName("TextSecondary")
        bottom_row.addWidget(self.status_msg_label, 1)

        self.metrics_label = QLabel("")
        self.metrics_label.setObjectName("TextSecondary")
        self.metrics_label.setStyleSheet("font-weight: 500;")
        bottom_row.addWidget(self.metrics_label)

        layout.addLayout(bottom_row)

    def update_data(self, data: Dict[str, Any]) -> None:
        self.item_data = data
        self.item_id = data.get("id", self.item_id)

        title = data.get("title") or data.get("url") or ""
        self.title_label.setText(title)

        filename = data.get("fileName") or ""
        if filename:
            self.filename_label.setText(filename)
            self.filename_label.show()
        else:
            self.filename_label.hide()

        status = data.get("status", "Queued")
        self.badge_label.setText(status)
        self.badge_label.setObjectName(f"Badge{status}")
        self.badge_label.setStyle(self.badge_label.style())

        percent = float(data.get("progressPercentage", 0.0))
        self.progress_bar.setValue(int(percent * 10))

        status_msg = data.get("statusMessage", status)
        self.status_msg_label.setText(status_msg)

        speed = data.get("downloadSpeed", "")
        size = data.get("totalSize", "")
        eta = data.get("eta", "")

        metrics_parts = []
        if speed:
            metrics_parts.append(f"⚡ {speed}")
        if size:
            metrics_parts.append(f"📦 {size}")
        if eta:
            metrics_parts.append(f"⏳ ETA: {eta}")

        self.metrics_label.setText("   ".join(metrics_parts))

        # Button visibility & enabled states
        is_active = status in ("Downloading", "Processing", "Queued")
        is_finished = status in ("Completed", "Failed", "Cancelled")
        file_exists = data.get("fileExists", False) or self._check_file_exists()

        self.btn_play.setVisible(file_exists)
        self.btn_delete_file.setVisible(file_exists)
        self.btn_cancel.setEnabled(is_active)
        self.btn_retry.setEnabled(is_finished)

    def update_progress(self, report: Dict[str, Any]) -> None:
        percent = float(report.get("percentage", 0.0))
        self.progress_bar.setValue(int(percent * 10))

        if report.get("statusText"):
            self.status_msg_label.setText(report["statusText"])

        speed = report.get("downloadSpeed", "")
        size = report.get("totalSize", "")
        eta = report.get("eta", "")

        metrics_parts = []
        if speed:
            metrics_parts.append(f"⚡ {speed}")
        if size:
            metrics_parts.append(f"📦 {size}")
        if eta:
            metrics_parts.append(f"⏳ ETA: {eta}")

        self.metrics_label.setText("   ".join(metrics_parts))

        if report.get("extractedFileName"):
            self.filename_label.setText(report["extractedFileName"])
            self.filename_label.show()

    def _check_file_exists(self) -> bool:
        target = self.item_data.get("existingFilePath") or self.item_data.get("fullPath")
        return bool(target and os.path.exists(target))

    def _get_target_file(self) -> Optional[str]:
        target = self.item_data.get("existingFilePath")
        if target and os.path.exists(target):
            return target

        out_dir = self.item_data.get("outputDirectory", "")
        fname = self.item_data.get("fileName", "")
        if out_dir and fname:
            candidate = os.path.join(out_dir, fname)
            if os.path.exists(candidate):
                return candidate
        return None

    def _play_file(self) -> None:
        target = self._get_target_file()
        if not target:
            QMessageBox.warning(self, "Aviso", "O arquivo baixado não foi encontrado no disco.")
            return

        try:
            if sys.platform.startswith("win"):
                os.startfile(target)
            elif sys.platform == "darwin":
                subprocess.Popen(["open", target])
            else:
                subprocess.Popen(["xdg-open", target])
        except Exception as ex:
            QMessageBox.critical(self, "Erro", f"Não foi possível reproduzir o arquivo:\n{ex}")

    def _open_folder(self) -> None:
        target_file = self._get_target_file()
        out_dir = self.item_data.get("outputDirectory", "")

        try:
            if sys.platform.startswith("win"):
                if target_file and os.path.exists(target_file):
                    subprocess.Popen(f'explorer.exe /select,"{os.path.normpath(target_file)}"')
                elif out_dir and os.path.isdir(out_dir):
                    subprocess.Popen(f'explorer.exe "{os.path.normpath(out_dir)}"')
            elif sys.platform == "darwin":
                if target_file and os.path.exists(target_file):
                    subprocess.Popen(["open", "-R", target_file])
                elif out_dir and os.path.isdir(out_dir):
                    subprocess.Popen(["open", out_dir])
            else:
                folder = out_dir if os.path.isdir(out_dir) else os.path.dirname(target_file or "")
                if folder and os.path.isdir(folder):
                    subprocess.Popen(["xdg-open", folder])
        except Exception as ex:
            QMessageBox.critical(self, "Erro", f"Não foi possível abrir a pasta:\n{ex}")

    def _delete_file_from_disk(self) -> None:
        target_file = self._get_target_file()
        if not target_file:
            QMessageBox.warning(self, "Aviso", "Arquivo não localizado no disco.")
            return

        res = QMessageBox.question(
            self,
            "Confirmar Exclusão de Arquivo",
            f"Deseja realmente excluir permanentemente este arquivo do disco?\n{target_file}",
            QMessageBox.Yes | QMessageBox.No,
            QMessageBox.No,
        )
        if res == QMessageBox.Yes:
            try:
                if os.path.exists(target_file):
                    os.remove(target_file)
                self.btn_play.setVisible(False)
                self.btn_delete_file.setVisible(False)
                self.on_remove(self.item_id, False)
            except Exception as ex:
                QMessageBox.critical(self, "Erro", f"Erro ao excluir arquivo: {ex}")

