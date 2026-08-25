/**
 * YoutubeDL-GUI Browser Extension - Background Service Worker
 * Compatible with Manifest V3 (Chrome, Edge, Firefox, Brave, Opera)
 */

try {
  importScripts('cookieHelper.js', 'proxyHelper.js');
} catch (e) {
  // Firefox or alternate environment
}

const crossBrowser = typeof browser !== 'undefined' ? browser : chrome;
const DEFAULT_PORT = 48190;

// Setup context menus when installed or updated
crossBrowser.runtime.onInstalled.addListener(() => {
  setupContextMenus();
});

function setupContextMenus() {
  crossBrowser.contextMenus.removeAll(() => {
    // 1. Download current page
    crossBrowser.contextMenus.create({
      id: 'ydl_download_page',
      title: 'Baixar esta página com YoutubeDL-GUI',
      contexts: ['page']
    });

    // 2. Download specific link
    crossBrowser.contextMenus.create({
      id: 'ydl_download_link',
      title: 'Baixar link com YoutubeDL-GUI',
      contexts: ['link']
    });

    // 3. Download media element (video / audio)
    crossBrowser.contextMenus.create({
      id: 'ydl_download_media',
      title: 'Baixar mídia selecionada com YoutubeDL-GUI',
      contexts: ['video', 'audio']
    });
  });
}

// Handle context menu clicks
crossBrowser.contextMenus.onClicked.addListener(async (info, tab) => {
  let targetUrl = '';
  let title = tab?.title || '';

  if (info.menuItemId === 'ydl_download_link' && info.linkUrl) {
    targetUrl = info.linkUrl;
    title = info.selectionText || info.linkUrl;
  } else if (info.menuItemId === 'ydl_download_media' && info.srcUrl) {
    targetUrl = info.srcUrl;
  } else if (info.pageUrl) {
    targetUrl = info.pageUrl;
  } else if (tab?.url) {
    targetUrl = tab.url;
  }

  if (targetUrl) {
    await handleDownloadRequest(targetUrl, title);
  }
});

// Handle keyboard shortcut commands (e.g. Alt+Shift+D)
crossBrowser.commands.onCommand.addListener(async (command) => {
  if (command === 'download_active_tab') {
    const [tab] = await crossBrowser.tabs.query({ active: true, currentWindow: true });
    if (tab && tab.url) {
      await handleDownloadRequest(tab.url, tab.title || '');
    }
  }
});

// Send download request to local YoutubeDlGui desktop app
async function handleDownloadRequest(url, title = '') {
  try {
    const config = await crossBrowser.storage.local.get({
      serverPort: DEFAULT_PORT,
      defaultQuality: 'Best',
      defaultAudioFormat: 'None',
      showNotifications: true,
      downloadDirectory: '',
      sendCookiesDefault: true,
      sendProxyDefault: false,
      selectedPlayerClients: [],
      defaultExtraArgs: ''
    });

    const port = config.serverPort || DEFAULT_PORT;
    const endpoint = `http://127.0.0.1:${port}/api/download`;

    let cookiesText = '';
    if (config.sendCookiesDefault !== false && typeof getNetscapeCookiesForUrl === 'function') {
      try {
        cookiesText = await getNetscapeCookiesForUrl(url);
      } catch (err) {
        console.warn('Falha ao exportar cookies no background:', err);
      }
    }

    let proxyUrl = undefined;
    if (config.sendProxyDefault && typeof getBrowserProxyForUrl === 'function') {
      try {
        const proxyInfo = await getBrowserProxyForUrl(url);
        if (proxyInfo && proxyInfo.hasProxy && proxyInfo.proxyUrl) {
          proxyUrl = proxyInfo.proxyUrl;
        }
      } catch (err) {
        console.warn('Falha ao detectar proxy no background:', err);
      }
    }

    const clients = Array.isArray(config.selectedPlayerClients) ? config.selectedPlayerClients : [];
    const playerClients = clients.length > 0 ? clients.join(',') : undefined;

    const isAudio = Boolean(config.defaultAudioFormat && config.defaultAudioFormat !== 'None');
    const payload = {
      url: url.trim(),
      title: title || url,
      quality: isAudio ? 'Best' : (config.defaultQuality || 'Best'),
      audioFormat: isAudio ? config.defaultAudioFormat : 'None',
      audioOnly: isAudio,
      playlist: false,
      downloadDirectory: config.downloadDirectory || undefined,
      cookiesText: cookiesText || undefined,
      playerClients: playerClients,
      extraOptions: config.defaultExtraArgs || undefined,
      proxy: proxyUrl
    };

    const response = await fetch(endpoint, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(payload)
    });

    if (response.ok) {
      const data = await response.json();
      flashBadge('OK', '#10B981'); // Emerald green
      if (config.showNotifications) {
        showNotification(
          'Download Enfileirado!',
          `O link foi enviado para o YoutubeDL-GUI com sucesso.\n${title || url}`
        );
      }
    } else {
      const err = await response.json().catch(() => ({}));
      flashBadge('ERR', '#EF4444'); // Red
      showNotification(
        'Falha ao Iniciar Download',
        err.message || 'O YoutubeDL-GUI rejeitou a requisição.'
      );
    }
  } catch (error) {
    console.error('Erro de comunicação com o YoutubeDL-GUI:', error);
    flashBadge('OFF', '#F59E0B'); // Amber
    showNotification(
      'YoutubeDL-GUI Desconectado',
      'Não foi possível conectar ao YoutubeDL-GUI. Verifique se o aplicativo desktop está aberto.'
    );
  }
}

// Flash badge text on extension icon
function flashBadge(text, color) {
  crossBrowser.action.setBadgeText({ text });
  crossBrowser.action.setBadgeBackgroundColor({ color });

  setTimeout(() => {
    crossBrowser.action.setBadgeText({ text: '' });
  }, 4000);
}

// Show native desktop notification
function showNotification(title, message) {
  crossBrowser.notifications.create({
    type: 'basic',
    iconUrl: crossBrowser.runtime.getURL('icons/icon-128.png'),
    title: title,
    message: message
  });
}

// Handle message passing between popup/options and background
crossBrowser.runtime.onMessage.addListener((message, sender, sendResponse) => {
  if (message.type === 'CHECK_SERVER_STATUS') {
    (async () => {
      const config = await crossBrowser.storage.local.get({ serverPort: DEFAULT_PORT });
      const port = config.serverPort || DEFAULT_PORT;
      try {
        const res = await fetch(`http://127.0.0.1:${port}/api/ping`);
        if (res.ok) {
          const data = await res.json();
          sendResponse({ isOnline: true, data });
          return;
        }
      } catch (err) {
        // Offline
      }
      sendResponse({ isOnline: false });
    })();
    return true; // Keep channel open for async response
  }
});
