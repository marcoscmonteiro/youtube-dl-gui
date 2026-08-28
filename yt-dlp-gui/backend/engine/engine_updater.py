from __future__ import annotations

import asyncio
import subprocess
import sys
from typing import Callable, Optional


class EngineUpdater:
    @staticmethod
    async def update_engine_async(
        progress_callback: Optional[Callable[[str], None]] = None,
    ) -> str:
        output_lines = []

        def _report(line: str) -> None:
            output_lines.append(line)
            if progress_callback:
                progress_callback(line)

        _report("Iniciando verificação e atualização do yt-dlp...")

        loop = asyncio.get_running_loop()

        def _run_pip_update() -> int:
            cmd = [sys.executable, "-m", "pip", "install", "--upgrade", "yt-dlp"]
            _report(f"Executando: {' '.join(cmd)}")

            process = subprocess.Popen(
                cmd,
                stdout=subprocess.PIPE,
                stderr=subprocess.STDOUT,
                text=True,
                encoding="utf-8",
                errors="replace",
            )

            if process.stdout:
                for line in iter(process.stdout.readline, ""):
                    clean_line = line.strip()
                    if clean_line:
                        _report(clean_line)

            process.wait()
            return process.returncode

        try:
            return_code = await loop.run_in_executor(None, _run_pip_update)
            if return_code == 0:
                _report("\n[Sucesso] yt-dlp atualizado com sucesso para a versão mais recente!")
            else:
                _report(f"\n[Aviso] Atualização via pip finalizada com código: {return_code}")
        except Exception as ex:
            _report(f"\n[Erro] Falha ao atualizar yt-dlp: {ex}")

        return "\n".join(output_lines)

