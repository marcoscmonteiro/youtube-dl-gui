from __future__ import annotations

from typing import Any, Dict, Optional

try:
    from PySide6.QtCore import Qt
    from PySide6.QtGui import QGuiApplication
    from PySide6.QtWidgets import (
        QCheckBox,
        QDialog,
        QHBoxLayout,
        QLabel,
        QPlainTextEdit,
        QPushButton,
        QVBoxLayout,
        QWidget,
    )
except ImportError:
    QDialog = QWidget = object  # type: ignore


class LogViewerDialog(QDialog):
    def __init__(
        self,
        item_id: str,
        initial_log: str = "",
        item_title: str = "",
        parent: Optional[QWidget] = None,
    ) -> None:
        super().__init__(parent)
        self.item_id = item_id
        self.setWindowTitle(f"Log de Download - {item_title or item_id}")
        self.resize(750, 480)
        self.setMinimumSize(500, 300)

        self._setup_ui()
        if initial_log:
            self.append_log(initial_log)

    def _setup_ui(self) -> None:
        layout = QVBoxLayout(self)
        layout.setContentsMargins(16, 16, 16, 16)
        layout.setSpacing(12)

        # Header Info
        header_layout = QHBoxLayout()
        title_lbl = QLabel(f"<b>ID do Download:</b> {self.item_id}")
        header_layout.addWidget(title_lbl)
        header_layout.addStretch()

        self.chk_autoscroll = QCheckBox("Rolar automaticamente")
        self.chk_autoscroll.setChecked(True)
        header_layout.addWidget(self.chk_autoscroll)

        layout.addLayout(header_layout)

        # Text Area
        self.log_text = QPlainTextEdit()
        self.log_text.setReadOnly(True)
        layout.addWidget(self.log_text, 1)

        # Buttons
        btn_layout = QHBoxLayout()
        btn_layout.addStretch()

        self.btn_copy = QPushButton("📋 Copiar Log")
        self.btn_copy.clicked.connect(self._copy_to_clipboard)
        btn_layout.addWidget(self.btn_copy)

        self.btn_clear = QPushButton("Limpar Visualização")
        self.btn_clear.clicked.connect(self.log_text.clear)
        btn_layout.addWidget(self.btn_clear)

        self.btn_close = QPushButton("Fechar")
        self.btn_close.clicked.connect(self.accept)
        btn_layout.addWidget(self.btn_close)

        layout.addLayout(btn_layout)

    def append_log(self, text: str) -> None:
        self.log_text.appendPlainText(text)
        if self.chk_autoscroll.isChecked():
            scrollbar = self.log_text.verticalScrollBar()
            scrollbar.setValue(scrollbar.maximum())

    def _copy_to_clipboard(self) -> None:
        clipboard = QGuiApplication.clipboard()
        clipboard.setText(self.log_text.toPlainText())

