from __future__ import annotations

import threading
from typing import Callable, Optional

try:
    from PySide6.QtCore import Qt, Signal
    from PySide6.QtWidgets import (
        QDialog,
        QHBoxLayout,
        QLabel,
        QPlainTextEdit,
        QProgressBar,
        QPushButton,
        QVBoxLayout,
        QWidget,
    )
except ImportError:
    QDialog = QWidget = object  # type: ignore


class UpdateDialog(QDialog):
    def __init__(
        self,
        update_func: Callable[[], str],
        parent: Optional[QWidget] = None,
    ) -> None:
        super().__init__(parent)
        self.setWindowTitle("Atualização da Engine - yt-dlp")
        self.resize(650, 400)
        self.update_func = update_func

        self._setup_ui()
        self._start_update()

    def _setup_ui(self) -> None:
        layout = QVBoxLayout(self)
        layout.setContentsMargins(16, 16, 16, 16)
        layout.setSpacing(12)

        self.status_label = QLabel("Verificando e atualizando a biblioteca yt-dlp...")
        self.status_label.setStyleSheet("font-weight: 600; font-size: 13px;")
        layout.addWidget(self.status_label)

        self.progress_bar = QProgressBar()
        self.progress_bar.setRange(0, 0)  # Indeterminate spinner
        layout.addWidget(self.progress_bar)

        self.log_area = QPlainTextEdit()
        self.log_area.setReadOnly(True)
        layout.addWidget(self.log_area, 1)

        btn_layout = QHBoxLayout()
        btn_layout.addStretch()

        self.btn_close = QPushButton("Fechar")
        self.btn_close.setEnabled(False)
        self.btn_close.clicked.connect(self.accept)
        btn_layout.addWidget(self.btn_close)

        layout.addLayout(btn_layout)

    def _start_update(self) -> None:
        def _worker() -> None:
            output = self.update_func()
            self._on_finished(output)

        thread = threading.Thread(target=_worker, daemon=True)
        thread.start()

    def _on_finished(self, output: str) -> None:
        from PySide6.QtCore import QMetaObject, Qt, Q_ARG

        def _update_ui() -> None:
            self.progress_bar.setRange(0, 100)
            self.progress_bar.setValue(100)
            self.status_label.setText("Processo de atualização finalizado.")
            self.log_area.setPlainText(output)
            self.btn_close.setEnabled(True)

        # Ensure thread safety on Qt GUI thread
        from PySide6.QtCore import QTimer
        QTimer.singleShot(0, _update_ui)

