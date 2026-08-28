from __future__ import annotations

import os
import sys
from typing import Any, Dict, List, Optional

try:
    from PySide6.QtCore import QEvent, Qt, QTimer
    from PySide6.QtGui import QGuiApplication, QIcon
    from PySide6.QtWidgets import (
        QApplication,
        QCheckBox,
        QComboBox,
        QFileDialog,
        QFrame,
        QGraphicsDropShadowEffect,
        QHBoxLayout,
        QLabel,
        QLineEdit,
        QMainWindow,
        QMessageBox,
        QPushButton,
        QScrollArea,
        QSizePolicy,
        QSpinBox,
        QSplitter,
        QStackedWidget,
        QVBoxLayout,
        QWidget,
    )
except ImportError:
    QMainWindow = QWidget = object  # type: ignore

from backend.models.domain import AppSettings, AudioFormat, VideoQuality
from backend.models.schemas import ExternalDownloadRequest
from frontend.client.backend_client import BackendClient
from frontend.styles import load_theme
from frontend.views.download_card import DownloadCard
from frontend.views.help_dialog import HelpDialog
from frontend.views.log_viewer_dialog import LogViewerDialog
from frontend.views.update_dialog import UpdateDialog


class MainWindow(QMainWindow):
    def __init__(self, client: BackendClient, parent: Optional[QWidget] = None) -> None:
        super().__init__(parent)
        self.client = client
        self.settings = AppSettings.create_default()
        self.download_cards: Dict[str, DownloadCard] = {}
        self.active_log_dialogs: Dict[str, LogViewerDialog] = {}
        self.search_filter = ""
        self._is_initializing = True

        self.setWindowTitle("yt-dlp GUI")
        self.resize(1040, 720)
        self.setMinimumSize(850, 520)

        self._setup_ui()
        self._connect_signals()

        # Load initial settings and data
        QTimer.singleShot(100, self._load_initial_data)

    def _setup_ui(self) -> None:
        central_widget = QWidget(self)
        central_widget.setObjectName("CentralWidget")
        self.setCentralWidget(central_widget)

        main_layout = QVBoxLayout(central_widget)
        main_layout.setContentsMargins(0, 0, 0, 0)
        main_layout.setSpacing(0)

        # 1. Header / Top Bar
        header_frame = QFrame()
        header_frame.setObjectName("HeaderFrame")
        header_layout = QHBoxLayout(header_frame)
        header_layout.setContentsMargins(16, 8, 16, 8)
        header_layout.setSpacing(10)

        # Logo / Title
        title_label = QLabel("⚡ yt-dlp GUI")
        title_label.setObjectName("AppTitle")
        header_layout.addWidget(title_label)

        # Connection status badge
        self.conn_badge = QLabel("🟢 Conectado")
        self.conn_badge.setObjectName("BadgeCompleted")
        header_layout.addWidget(self.conn_badge)

        header_layout.addStretch()

        # Top Action Buttons
        self.btn_update = QPushButton("⚡ Atualizar Engine")
        self.btn_update.clicked.connect(self._open_update_dialog)
        header_layout.addWidget(self.btn_update)

        self.btn_help = QPushButton("❓ Ajuda CLI")
        self.btn_help.clicked.connect(self._open_help_dialog)
        header_layout.addWidget(self.btn_help)

        self.btn_reset = QPushButton("🔄 Restaurar Padrões")
        self.btn_reset.clicked.connect(self._reset_defaults)
        header_layout.addWidget(self.btn_reset)

        # Dark Mode Switch
        self.chk_dark_mode = QCheckBox("🌙 Tema Escuro")
        self.chk_dark_mode.setChecked(True)
        self.chk_dark_mode.toggled.connect(self._toggle_theme)
        header_layout.addWidget(self.chk_dark_mode)

        main_layout.addWidget(header_frame)

        # Content container
        content_layout = QVBoxLayout()
        content_layout.setContentsMargins(20, 14, 20, 10)
        content_layout.setSpacing(12)

        # 2. Input Card
        input_card = QFrame()
        input_card.setObjectName("CardFrame")
        input_layout = QVBoxLayout(input_card)
        input_layout.setSpacing(10)

        # Row 1: URL input + Paste + Start Download
        url_row = QHBoxLayout()
        url_lbl = QLabel("URL do Vídeo:")
        url_lbl.setStyleSheet("font-weight: 600; font-size: 13px;")
        url_row.addWidget(url_lbl)

        self.url_input = QLineEdit()
        self.url_input.setPlaceholderText("Cole aqui o link do vídeo, música ou playlist...")
        self.url_input.returnPressed.connect(self._start_download)
        url_row.addWidget(self.url_input, 1)

        self.btn_paste = QPushButton("📋 Colar")
        self.btn_paste.clicked.connect(self._paste_clipboard)
        url_row.addWidget(self.btn_paste)

        self.btn_start = QPushButton("⬇ Iniciar Download")
        self.btn_start.setObjectName("PrimaryButton")
        self.btn_start.clicked.connect(self._start_download)
        url_row.addWidget(self.btn_start)

        input_layout.addLayout(url_row)

        # Row 2: Directory + Quality + Audio + Options Button
        opts_row = QHBoxLayout()
        opts_row.setSpacing(12)

        # Destination Directory
        dir_layout = QHBoxLayout()
        dir_lbl = QLabel("Destino:")
        dir_lbl.setObjectName("TextSecondary")
        dir_layout.addWidget(dir_lbl)

        self.combo_dir = QComboBox()
        self.combo_dir.setEditable(True)
        self.combo_dir.currentTextChanged.connect(self._on_setting_changed)
        dir_layout.addWidget(self.combo_dir, 1)

        self.btn_browse = QPushButton("📁 Procurar...")
        self.btn_browse.clicked.connect(self._browse_folder)
        dir_layout.addWidget(self.btn_browse)

        opts_row.addLayout(dir_layout, 2)

        # Video Quality
        qual_layout = QHBoxLayout()
        qual_lbl = QLabel("Qualidade:")
        qual_lbl.setObjectName("TextSecondary")
        qual_layout.addWidget(qual_lbl)

        self.combo_quality = QComboBox()
        for q in VideoQuality:
            self.combo_quality.addItem(q.value, q)
        self.combo_quality.currentIndexChanged.connect(self._on_setting_changed)
        qual_layout.addWidget(self.combo_quality, 1)
        opts_row.addLayout(qual_layout, 1)

        # Audio Format
        audio_layout = QHBoxLayout()
        audio_lbl = QLabel("Áudio:")
        audio_lbl.setObjectName("TextSecondary")
        audio_layout.addWidget(audio_lbl)

        self.combo_audio = QComboBox()
        for a in AudioFormat:
            self.combo_audio.addItem(a.value, a)
        self.combo_audio.currentIndexChanged.connect(self._on_setting_changed)
        audio_layout.addWidget(self.combo_audio, 1)
        opts_row.addLayout(audio_layout, 1)

        # Advanced Toggle Button
        self.btn_toggle_adv = QPushButton("⚙ Opções")
        self.btn_toggle_adv.clicked.connect(self._toggle_advanced_options)
        opts_row.addWidget(self.btn_toggle_adv)

        input_layout.addLayout(opts_row)

        # Row 3: Collapsible Advanced Options Frame
        self.adv_frame = QFrame()
        self.adv_frame.setObjectName("CardFrame")
        self.adv_frame.setStyleSheet("background-color: #0F172A; margin-top: 4px;")
        self.adv_frame.setVisible(False)
        adv_layout = QVBoxLayout(self.adv_frame)
        adv_layout.setSpacing(8)

        # Checkboxes row
        chk_row = QHBoxLayout()
        self.chk_playlist = QCheckBox("Download de Playlist")
        self.chk_playlist.toggled.connect(self._on_setting_changed)
        chk_row.addWidget(self.chk_playlist)

        self.chk_no_cache = QCheckBox("Sem Cache (--no-cache-dir)")
        self.chk_no_cache.setChecked(True)
        self.chk_no_cache.toggled.connect(self._on_setting_changed)
        chk_row.addWidget(self.chk_no_cache)

        self.chk_no_part = QCheckBox("Não usar .part")
        self.chk_no_part.setChecked(True)
        self.chk_no_part.toggled.connect(self._on_setting_changed)
        chk_row.addWidget(self.chk_no_part)

        self.chk_auto_paste = QCheckBox("Auto colar URL do clipboard")
        self.chk_auto_paste.setChecked(True)
        self.chk_auto_paste.toggled.connect(self._on_setting_changed)
        chk_row.addWidget(self.chk_auto_paste)

        chk_row.addStretch()
        adv_layout.addLayout(chk_row)

        # Extra options & Concurrent downloads
        extra_row = QHBoxLayout()
        extra_lbl = QLabel("Argumentos Extras:")
        extra_lbl.setObjectName("TextSecondary")
        extra_row.addWidget(extra_lbl)

        self.combo_extra_opts = QComboBox()
        self.combo_extra_opts.setEditable(True)
        self.combo_extra_opts.currentTextChanged.connect(self._on_setting_changed)
        extra_row.addWidget(self.combo_extra_opts, 1)

        conc_lbl = QLabel("Downloads simultâneos:")
        conc_lbl.setObjectName("TextSecondary")
        extra_row.addWidget(conc_lbl)

        self.spin_concurrent = QSpinBox()
        self.spin_concurrent.setRange(1, 10)
        self.spin_concurrent.setValue(3)
        self.spin_concurrent.valueChanged.connect(self._on_setting_changed)
        extra_row.addWidget(self.spin_concurrent)

        adv_layout.addLayout(extra_row)
        input_layout.addWidget(self.adv_frame)

        content_layout.addWidget(input_card)

        # 3. Downloads List Toolbar & Filter
        list_toolbar = QHBoxLayout()
        queue_title = QLabel("Fila de Downloads")
        queue_title.setStyleSheet("font-weight: 600; font-size: 15px;")
        list_toolbar.addWidget(queue_title)

        self.search_box = QLineEdit()
        self.search_box.setPlaceholderText("🔍 Filtrar downloads...")
        self.search_box.setMaximumWidth(280)
        self.search_box.textChanged.connect(self._on_search_changed)
        list_toolbar.addWidget(self.search_box)

        list_toolbar.addStretch()

        self.btn_clear_completed = QPushButton("🧹 Limpar Concluídos")
        self.btn_clear_completed.clicked.connect(self.client.clear_completed)
        list_toolbar.addWidget(self.btn_clear_completed)

        self.btn_cancel_all = QPushButton("⏹ Cancelar Todos")
        self.btn_cancel_all.clicked.connect(self.client.cancel_all)
        list_toolbar.addWidget(self.btn_cancel_all)

        content_layout.addLayout(list_toolbar)

        # 4. Scrollable Downloads List Area
        self.scroll_area = QScrollArea()
        self.scroll_area.setWidgetResizable(True)
        self.scroll_area.setFrameShape(QFrame.NoFrame)

        self.cards_container = QWidget()
        self.cards_layout = QVBoxLayout(self.cards_container)
        self.cards_layout.setContentsMargins(0, 0, 0, 0)
        self.cards_layout.setSpacing(6)
        self.cards_layout.addStretch()

        self.scroll_area.setWidget(self.cards_container)
        content_layout.addWidget(self.scroll_area, 1)

        main_layout.addLayout(content_layout, 1)

        # 5. Status Bar
        status_frame = QFrame()
        status_frame.setObjectName("StatusBarFrame")
        status_layout = QHBoxLayout(status_frame)
        status_layout.setContentsMargins(16, 6, 16, 6)
        status_layout.setSpacing(12)

        self.stat_active = QLabel("⚡ Baixando: 0")
        self.stat_active.setObjectName("TextSecondary")
        status_layout.addWidget(self.stat_active)

        self.stat_queued = QLabel("⏳ Na Fila: 0")
        self.stat_queued.setObjectName("TextSecondary")
        status_layout.addWidget(self.stat_queued)

        self.stat_completed = QLabel("✅ Concluídos: 0")
        self.stat_completed.setObjectName("TextSecondary")
        status_layout.addWidget(self.stat_completed)

        self.stat_failed = QLabel("❌ Erros: 0")
        self.stat_failed.setObjectName("TextSecondary")
        status_layout.addWidget(self.stat_failed)

        status_layout.addStretch()

        version_label = QLabel("yt-dlp GUI v2.0")
        version_label.setObjectName("TextMuted")
        status_layout.addWidget(version_label)

        main_layout.addWidget(status_frame)

    def _connect_signals(self) -> None:
        self.client.connection_status_changed.connect(self._on_connection_status_changed)
        self.client.initial_state_received.connect(self._on_initial_state_received)
        self.client.item_added.connect(self._on_item_added)
        self.client.progress_updated.connect(self._on_progress_updated)
        self.client.status_changed.connect(self._on_status_changed)
        self.client.log_line_received.connect(self._on_log_line_received)
        self.client.item_removed.connect(self._on_item_removed)

    def _load_initial_data(self) -> None:
        # Start client listener
        self.client.start()

        # Fetch settings and items via REST
        settings_dict = self.client.get_settings()
        if settings_dict:
            self.settings = AppSettings.from_dict(settings_dict)
            self._apply_settings_to_ui()

        items = self.client.get_downloads()
        for item_dict in reversed(items):
            self._add_or_update_card(item_dict)

        self._update_status_counters()
        self._is_initializing = False

    def _apply_settings_to_ui(self) -> None:
        s = self.settings

        # Theme
        self.chk_dark_mode.setChecked(s.theme.value.lower() != "light")
        self._apply_theme()

        # Destination history
        self.combo_dir.clear()
        for d in s.destination_history:
            self.combo_dir.addItem(d)
        if s.work_dir:
            self.combo_dir.setEditText(s.work_dir)

        # Quality & Audio
        idx_q = self.combo_quality.findData(s.default_quality)
        if idx_q >= 0:
            self.combo_quality.setCurrentIndex(idx_q)

        idx_a = self.combo_audio.findData(s.default_audio_format)
        if idx_a >= 0:
            self.combo_audio.setCurrentIndex(idx_a)

        # Advanced Checkboxes
        self.chk_playlist.setChecked(s.download_playlist)
        self.chk_no_cache.setChecked(s.no_cache_dir)
        self.chk_no_part.setChecked(s.no_part_file)
        self.chk_auto_paste.setChecked(s.clipboard_auto_paste)
        self.spin_concurrent.setValue(s.max_concurrent_downloads)

        # Extra options history
        self.combo_extra_opts.clear()
        for opt in s.extra_options_history:
            self.combo_extra_opts.addItem(opt)
        if s.extra_options:
            self.combo_extra_opts.setEditText(s.extra_options)

        self.adv_frame.setVisible(s.is_advanced_options_open)

    def _apply_theme(self) -> None:
        theme_name = "Dark" if self.chk_dark_mode.isChecked() else "Light"
        qss = load_theme(theme_name)
        if qss:
            self.setStyleSheet(qss)

    def _toggle_theme(self, checked: bool) -> None:
        self._apply_theme()
        if not self._is_initializing:
            self.settings.theme = (
                AppSettings.create_default().theme if checked else AppSettings.create_default().theme
            )
            self._save_settings_async()

    def _toggle_advanced_options(self) -> None:
        is_visible = not self.adv_frame.isVisible()
        self.adv_frame.setVisible(is_visible)
        self.settings.is_advanced_options_open = is_visible
        self._save_settings_async()

    def _browse_folder(self) -> None:
        current = self.combo_dir.currentText() or str(os.path.expanduser("~/Videos"))
        chosen = QFileDialog.getExistingDirectory(self, "Selecione o diretório de download", current)
        if chosen:
            self.combo_dir.setEditText(chosen)
            self._save_settings_async()

    def _paste_clipboard(self) -> None:
        clipboard = QGuiApplication.clipboard()
        text = clipboard.text().strip()
        if text:
            self.url_input.setText(text)

    def _start_download(self) -> None:
        url = self.url_input.text().strip()
        if not url:
            QMessageBox.warning(self, "Aviso", "Por favor, informe a URL do vídeo ou áudio.")
            return

        if not (url.startswith("http://") or url.startswith("https://")):
            QMessageBox.critical(self, "URL Inválida", "A URL informada deve iniciar com http:// ou https://")
            return

        out_dir = self.combo_dir.currentText().strip() or str(os.path.expanduser("~/Downloads"))
        quality = self.combo_quality.currentData().value
        audio_format = self.combo_audio.currentData().value
        extra_opts = self.combo_extra_opts.currentText().strip()

        payload = {
            "url": url,
            "quality": quality,
            "audioOnly": audio_format != "None",
            "audioFormat": audio_format,
            "playlist": self.chk_playlist.isChecked(),
            "downloadDirectory": out_dir,
            "extraOptions": extra_opts,
        }

        self.client.add_download(payload)
        self.url_input.clear()

        # Update destination and options history
        self._add_to_history(self.combo_dir, out_dir, self.settings.destination_history)
        if extra_opts:
            self._add_to_history(self.combo_extra_opts, extra_opts, self.settings.extra_options_history)

        self._save_settings_async()

    def _add_to_history(self, combo: QComboBox, item: str, history_list: List[str]) -> None:
        if not item or item in history_list:
            return
        history_list.insert(0, item)
        combo.insertItem(0, item)

    def _on_search_changed(self, text: str) -> None:
        self.search_filter = text.strip().lower()
        for card in self.download_cards.values():
            if not self.search_filter:
                card.show()
            else:
                title = (card.item_data.get("title") or "").lower()
                filename = (card.item_data.get("fileName") or "").lower()
                url = (card.item_data.get("url") or "").lower()
                matched = (
                    self.search_filter in title
                    or self.search_filter in filename
                    or self.search_filter in url
                )
                card.setVisible(matched)

    def _on_connection_status_changed(self, is_connected: bool) -> None:
        if is_connected:
            self.conn_badge.setText("🟢 Conectado")
            self.conn_badge.setObjectName("BadgeCompleted")
        else:
            self.conn_badge.setText("🔴 Desconectado")
            self.conn_badge.setObjectName("BadgeFailed")
        self.conn_badge.setStyle(self.conn_badge.style())

    def _on_initial_state_received(self, data: Dict[str, Any]) -> None:
        items = data.get("items", [])
        for item_dict in reversed(items):
            self._add_or_update_card(item_dict)
        self._update_status_counters()

    def _on_item_added(self, item_dict: Dict[str, Any]) -> None:
        self._add_or_update_card(item_dict, insert_top=True)
        self._update_status_counters()

    def _on_progress_updated(self, report: Dict[str, Any]) -> None:
        item_id = report.get("id")
        if item_id in self.download_cards:
            self.download_cards[item_id].update_progress(report)

    def _on_status_changed(self, item_dict: Dict[str, Any]) -> None:
        item_id = item_dict.get("id")
        if item_id in self.download_cards:
            self.download_cards[item_id].update_data(item_dict)
        self._update_status_counters()

    def _on_log_line_received(self, item_id: str, line: str) -> None:
        if item_id in self.active_log_dialogs:
            self.active_log_dialogs[item_id].append_log(line)

    def _on_item_removed(self, item_id: str) -> None:
        if item_id in self.download_cards:
            card = self.download_cards.pop(item_id)
            self.cards_layout.removeWidget(card)
            card.deleteLater()
        self._update_status_counters()

    def _add_or_update_card(self, item_dict: Dict[str, Any], insert_top: bool = False) -> None:
        item_id = item_dict.get("id")
        if not item_id:
            return

        if item_id in self.download_cards:
            self.download_cards[item_id].update_data(item_dict)
            return

        card = DownloadCard(
            item_data=item_dict,
            on_cancel=self.client.cancel_download,
            on_retry=self.client.retry_download,
            on_remove=self.client.remove_download,
            on_view_log=self._open_log_dialog,
            parent=self.cards_container,
        )

        self.download_cards[item_id] = card
        if insert_top:
            self.cards_layout.insertWidget(0, card)
        else:
            # Insert before the stretch spacer
            count = self.cards_layout.count()
            self.cards_layout.insertWidget(max(0, count - 1), card)

    def _update_status_counters(self) -> None:
        active = sum(
            1
            for c in self.download_cards.values()
            if c.item_data.get("status") in ("Downloading", "Processing")
        )
        queued = sum(
            1
            for c in self.download_cards.values()
            if c.item_data.get("status") == "Queued"
        )
        completed = sum(
            1
            for c in self.download_cards.values()
            if c.item_data.get("status") == "Completed"
        )
        failed = sum(
            1
            for c in self.download_cards.values()
            if c.item_data.get("status") == "Failed"
        )

        self.stat_active.setText(f"⚡ Baixando: {active}")
        self.stat_queued.setText(f"⏳ Na Fila: {queued}")
        self.stat_completed.setText(f"✅ Concluídos: {completed}")
        self.stat_failed.setText(f"❌ Erros: {failed}")

    def _open_log_dialog(self, item_id: str) -> None:
        if item_id in self.active_log_dialogs:
            dialog = self.active_log_dialogs[item_id]
            dialog.activateWindow()
            dialog.raise_()
            return

        item_data = self.download_cards.get(item_id, {}).item_data if item_id in self.download_cards else {}
        initial_log = item_data.get("log", "")
        title = item_data.get("title", item_id)

        dialog = LogViewerDialog(item_id, initial_log, title, self)
        self.active_log_dialogs[item_id] = dialog
        dialog.finished.connect(lambda: self.active_log_dialogs.pop(item_id, None))
        dialog.show()

    def _open_help_dialog(self) -> None:
        help_text = self.client.get_help()
        dlg = HelpDialog(help_text, self)
        dlg.exec()

    def _open_update_dialog(self) -> None:
        dlg = UpdateDialog(self.client.update_engine, self)
        dlg.exec()

    def _reset_defaults(self) -> None:
        res = QMessageBox.question(
            self,
            "Restaurar Padrões",
            "Deseja realmente restaurar todas as configurações para os padrões de fábrica?",
            QMessageBox.Yes | QMessageBox.No,
            QMessageBox.No,
        )
        if res == QMessageBox.Yes:
            self.settings = AppSettings.create_default()
            self._apply_settings_to_ui()
            self._save_settings_async()
            QMessageBox.information(self, "Sucesso", "Configurações restauradas com sucesso!")

    def _on_setting_changed(self) -> None:
        if self._is_initializing:
            return
        self.settings.work_dir = self.combo_dir.currentText().strip()
        self.settings.default_quality = self.combo_quality.currentData()
        self.settings.default_audio_format = self.combo_audio.currentData()
        self.settings.download_playlist = self.chk_playlist.isChecked()
        self.settings.no_cache_dir = self.chk_no_cache.isChecked()
        self.settings.no_part_file = self.chk_no_part.isChecked()
        self.settings.clipboard_auto_paste = self.chk_auto_paste.isChecked()
        self.settings.max_concurrent_downloads = self.spin_concurrent.value()
        self.settings.extra_options = self.combo_extra_opts.currentText().strip()
        self._save_settings_async()

    def _save_settings_async(self) -> None:
        self.client.save_settings(self.settings.to_dict())

    def changeEvent(self, event: QEvent) -> None:
        super().changeEvent(event)
        if event.type() == QEvent.ActivationChange and self.isActiveWindow():
            if self.chk_auto_paste.isChecked():
                clipboard = QGuiApplication.clipboard()
                text = clipboard.text().strip()
                if text and (text.startswith("http://") or text.startswith("https://")):
                    if self.url_input.text() != text:
                        self.url_input.setText(text)

