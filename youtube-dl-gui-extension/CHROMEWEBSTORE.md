# Chrome Web Store & Mozilla Add-ons Listing Metadata

**Last Updated:** 2026-08-24  
**Extension Name:** YoutubeDL-GUI - Downloader  
**Short Name:** YoutubeDL-GUI  
**Version:** 1.0.0  
**Category:** Productivity / Tools  

---

## 1. Store Descriptions

### Summary (132 characters max)
Envie vídeos, músicas e playlists diretamente para o aplicativo desktop YoutubeDL-GUI com apenas um clique ou pelo menu de contexto.

### Detailed Description
O **YoutubeDL-GUI Downloader** é a extensão oficial para integrar seu navegador ao poderoso aplicativo desktop YoutubeDL-GUI (baseado em yt-dlp).

Com esta extensão, você pode enviar links de vídeos e áudios diretamente do navegador para download no computador, sem precisar copiar e colar URLs manualmente.

### Principais Recursos:
- 🚀 **Download Instantâneo**: Clique no ícone da extensão para capturar a página atual e enviar ao aplicativo desktop.
- 🎯 **Opções Rápidas de Qualidade**: Escolha entre Melhor Qualidade, 4K, 1080p, 720p ou SD antes de enviar.
- 🎵 **Modo Áudio**: Extraia áudio diretamente em MP3, M4A, OPUS, FLAC ou WAV.
- 📋 **Menu de Contexto**: Clique com o botão direito em qualquer link, vídeo ou página para enviar o download.
- ⌨️ **Atalho de Teclado**: Use `Alt+Shift+D` para download rápido da aba ativa.
- 📊 **Status em Tempo Real**: Veja se o YoutubeDL-GUI está aberto e acompanhe quantos downloads estão ativos.

*Nota: Esta extensão requer que o aplicativo desktop YoutubeDL-GUI esteja instalado e em execução no seu computador.*

---

## 2. Permissions Justification

| Permission / Host | Plain-English Justification |
| :--- | :--- |
| `activeTab` | Permite obter a URL e o título do vídeo ou página da aba ativa quando o usuário clica na extensão ou aciona o atalho de teclado. |
| `tabs` | Necessário para identificar a URL ativa do navegador ao acionar atalhos de teclado e comandos contextuais. |
| `contextMenus` | Permite criar opções no menu de clique direito ("Baixar link com YoutubeDL-GUI", "Baixar mídia") para maior praticidade. |
| `storage` | Utilizado exclusivamente para salvar preferências locais do usuário (como porta do servidor local e opções padrão de qualidade). |
| `notifications` | Exibe avisos no sistema operacional confirmando se o link foi enviado com sucesso ou se o aplicativo desktop está fechado. |
| `http://127.0.0.1:48190/*`<br>`http://localhost:48190/*` | Permite a comunicação local HTTP entre a extensão do navegador e o servidor embutido no aplicativo desktop YoutubeDL-GUI na máquina do usuário. Nenhum dado é enviado para servidores externos. |

---

## 3. Privacy & Data Use

- **Coleta de Dados**: Nenhum dado pessoal, histórico de navegação ou informação sensível é coletado, rastreado ou compartilhado com terceiros.
- **Transmissão**: Todas as requisições ocorrem exclusivamente em loopback local (`127.0.0.1`) no computador do usuário para comunicação com o aplicativo desktop.
- **Armazenamento**: As preferências do usuário (qualidade e porta) são salvas apenas localmente no dispositivo via `chrome.storage.local`.

---

## 4. Version History

- **v1.0.0** (2026-08-24):
  - Lançamento inicial com suporte ao Chrome, Edge, Brave e Firefox (Manifest V3).
  - Comunicação via API Loopback HTTP com o YoutubeDL-GUI.
  - Menus de contexto, suporte a atalhos de teclado e popup interativo com tema escuro.
