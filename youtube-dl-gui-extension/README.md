# Extensão de Navegador para YoutubeDL-GUI (Chrome & Firefox)

Extensão de navegador moderna em **Manifest V3** para integração direta com o aplicativo desktop **YoutubeDL-GUI**. Permite enviar páginas de vídeo, músicas e playlists para download com um único clique ou através do menu de contexto.

---

## Recursos

- **Download com 1 Clique**: Detecta automaticamente a URL e o título da aba ativa no navegador.
- **Seletor de Qualidade e Formato**:
  - Vídeo: *Melhor Disponível*, *4K (2160p)*, *2K (1440p)*, *Full HD (1080p)*, *HD (720p)*, *SD (480p)*, etc.
  - Áudio Apenas: *MP3*, *M4A*, *OPUS*, *FLAC*, *WAV*, etc.
- **Detecção Inteligente de Playlists**: Marcação automática para baixar playlists inteiras quando um link de playlist é detectado.
- **Menu de Contexto (Botão Direito)**:
  - *"Baixar esta página com YoutubeDL-GUI"*
  - *"Baixar link com YoutubeDL-GUI"*
  - *"Baixar mídia selecionada com YoutubeDL-GUI"*
- **Atalho de Teclado**: Pressione `Alt + Shift + D` para baixar imediatamente a mídia da aba ativa.
- **Status em Tempo Real**: Verificação de conectividade e contadores de downloads ativos e em fila no aplicativo desktop.

---

## Como Instalar

### No Google Chrome / Microsoft Edge / Brave / Opera

1. Abra o navegador e acesse `chrome://extensions` (ou `edge://extensions` no Edge).
2. Ative a chave **"Modo do desenvolvedor"** (*Developer mode*) no canto superior direito.
3. Clique no botão **"Carregar sem compactação"** (*Load unpacked*).
4. Selecione a pasta `youtube-dl-gui-extension` deste repositório.
5. O ícone do **YoutubeDL-GUI** aparecerá na barra de ferramentas do seu navegador!

### No Mozilla Firefox

1. Abra o Firefox e acesse `about:debugging#/runtime/this-firefox`.
2. Clique no botão **"Carregar extensão temporária..."** (*Load Temporary Add-on...*).
3. Navegue até a pasta `youtube-dl-gui-extension` e selecione o arquivo `manifest.json` (ou o arquivo `dist/youtube-dl-gui-firefox.zip`).
4. A extensão será ativada instantaneamente.

---

## Comunicação com o Aplicativo Desktop

A extensão se comunica diretamente com o executável `YoutubeDlGui.App` em execução no Windows através do servidor de loopback seguro:
`http://127.0.0.1:48190/api/download`

- Nenhuma configuração ou instalação no Registro do Windows é necessária.
- Basta manter o **YoutubeDL-GUI** aberto enquanto navega na Web.
