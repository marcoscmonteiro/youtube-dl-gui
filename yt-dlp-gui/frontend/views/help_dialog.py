from __future__ import annotations

from typing import Optional

try:
    from PySide6.QtCore import Qt
    from PySide6.QtWidgets import (
        QDialog,
        QHBoxLayout,
        QLabel,
        QLineEdit,
        QPlainTextEdit,
        QPushButton,
        QVBoxLayout,
        QWidget,
    )
except ImportError:
    QDialog = QWidget = object  # type: ignore


class HelpDialog(QDialog):
    def __init__(self, help_text: str, parent: Optional[QWidget] = None) -> None:
        super().__init__(parent)
        self.setWindowTitle("Ajuda e Opções CLI - yt-dlp")
        self.resize(800, 520)
        self.full_help_text = help_text

        self._setup_ui()
        self.text_area.setPlainText(help_text)

    def _setup_ui(self) -> None:
        layout = QVBoxLayout(self)
        layout.setContentsMargins(16, 16, 16, 16)
        layout.setSpacing(12)

        # Search filter bar
        search_layout = QHBoxLayout()
        search_lbl = QLabel("Filtrar Opções:")
        search_layout.addWidget(search_lbl)

        self.search_input = QLineEdit()
        self.search_input.setPlaceholderText("Digite um parâmetro (ex: --cookies, --format, --proxy)...")
        self.search_input.textChanged.connect(self._filter_text)
        search_layout.addWidget(self.search_input, 1)

        layout.addLayout(search_layout)

        # Help content
        self.text_area = QPlainTextEdit()
        self.text_area.setReadOnly(True)
        layout.addWidget(self.text_area, 1)

        # Bottom buttons
        btn_layout = QHBoxLayout()
        btn_layout.addStretch()

        self.btn_close = QPushButton("Fechar")
        self.btn_close.clicked.connect(self.accept)
        btn_layout.addWidget(self.btn_close)

        layout.addLayout(btn_layout)

    def _filter_text(self, query: str) -> None:
        if not query.strip():
            self.text_area.setPlainText(self.full_help_text)
            return

        q = query.strip().lower()
        lines = self.full_help_text.splitlines()
        matched = [line for line in lines if q in line.lower()]
        self.text_area.setPlainText("\n".join(matched))

