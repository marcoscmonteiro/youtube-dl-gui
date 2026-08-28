# yt-dlp-gui ⚡

Aplicação moderna, desacoplada e nativamente multiplataforma (**Windows, macOS e Linux**) para download de mídias com o motor **yt-dlp** executado diretamente em memória Python, sem o overhead de subprocessos CLI redundantes.

---

## 🏗️ Arquitetura Desacoplada

```
┌─────────────────────────────────────────────────────────────┐
│                          CLIENTES                           │
│   [ Extensão Chrome/Firefox ]    [ GUI PySide6 / Qt6 ]      │
└──────────────┬───────────────────────────────▲──────────────┘
               │ (POST /api/download)          │ (WebSocket /ws)
               ▼                               ▼
┌─────────────────────────────────────────────────────────────┐
│          BACKEND HEADLESS (yt-dlp-gui-daemon)               │
│  - Servidor HTTP Local (Porta 48190) com suporte a CORS     │
│  - Canal de Streaming WebSocket bidirecional em tempo real   │
│  - Fila Assíncrona e Controle de Downloads Simultâneos      │
│  - Motor yt-dlp Nativo em Memória (import yt_dlp)           │
│  - Persistência Multiplataforma (Configurações e Histórico) │
└─────────────────────────────────────────────────────────────┘
```

### Principais Vantagens:
1. **Desempenho Extremo e Economia de Memória**: O `yt-dlp` roda nativamente em memória. O consumo de RAM é reduzido em até **~80%** em downloads paralelos em comparação com a execução repetida de executáveis PyInstaller.
2. **Backend Independente (Headless)**: O backend pode rodar em background ou servidor sem interface gráfica.
3. **Sincronização em Tempo Real (WebSocket)**: Qualquer download adicionado pela extensão do navegador aparece instantaneamente na interface gráfica com progresso suave e logs.
4. **Resiliência Total**: Fechar a interface gráfica **não cancela os downloads ativos**. Ao reabri-la, ela se reconecta e recupera o estado e streaming da fila instantaneamente.
5. **Compatibilidade Retroativa**: A extensão do navegador existente (`youtube-dl-gui-extension`) funciona imediatamente sem nenhuma modificação.

---

## 🚀 Instalação e Execução

### Pré-requisitos
* Python 3.10 ou superior
* Pip

```bash
cd yt-dlp-gui
pip install -r requirements.txt
```

---

### Executando a Aplicação Desktop (GUI)

```bash
# Inicia a interface gráfica (inicia o backend daemon automaticamente em background se não estiver ativo)
python -m frontend.main
```

---

### Executando Apenas o Backend (Headless Daemon)

```bash
# Inicia o serviço na porta 48190 (padrão)
python -m backend.main --port 48190
```

---

## 🌐 Endpoints da API REST & WebSocket

* `GET /api/ping` — Verificação de status e saúde do serviço.
* `GET /api/status` — Métricas globais (ativos, fila, concluídos, erros, diretório padrão).
* `POST /api/download` — Adiciona download à fila (compatível com a extensão do navegador).
* `GET /api/downloads` — Lista todos os downloads ativos e histórico.
* `POST /api/downloads/{id}/cancel` — Cancela o download especificado.
* `POST /api/downloads/{id}/retry` — Retenta o download especificado.
* `DELETE /api/downloads/{id}?deleteFile=true` — Remove da fila e opcionalmente exclui do disco.
* `POST /api/downloads/clear-completed` — Limpa itens concluídos da fila.
* `GET /api/help` — Consulta de opções CLI do yt-dlp.
* `POST /api/engine/update` — Dispara atualização in-place do yt-dlp.
* `WS /ws` — Canal WebSocket bidirecional para streaming de eventos (`item_added`, `progress`, `status_changed`, `log_line`, `item_removed`).

---

## 🧪 Executando os Testes Automatizados

```bash
# Execução via test_runner integrado:
python tests/test_runner.py

# Ou via pytest (se instalado):
pytest tests/
```

---

## 📦 Empacotamento para Distribuição

* **Windows**: Execute `build_scripts\build_windows.ps1`
* **macOS**: Execute `./build_scripts/build_macos.sh`
* **Linux**: Execute `./build_scripts/build_linux.sh`

