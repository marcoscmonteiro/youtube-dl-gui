/**
 * YoutubeDL-GUI Extension - Popup Logic
 */

const crossBrowser = typeof browser !== 'undefined' ? browser : chrome;
const DEFAULT_PORT = 48190;

let currentTab = null;
let serverPort = DEFAULT_PORT;
let downloadDirectory = '';
let isAudioMode = false;
let isConnected = false;

document.addEventListener('DOMContentLoaded', async () => {
  await loadStoredConfig();
  setupUIEventListeners();
  await inspectCurrentTab();
  await checkServerConnection();
});

// Load configuration from extension storage
async function loadStoredConfig() {
  const config = await crossBrowser.storage.local.get({
    serverPort: DEFAULT_PORT,
    defaultQuality: 'Best',
    defaultAudioFormat: 'Mp3',
    downloadPlaylistDefault: false,
    downloadDirectory: '',
    sendCookiesDefault: true,
    selectedPlayerClients: ['android', 'web', 'ios']
  });

  serverPort = config.serverPort || DEFAULT_PORT;
  downloadDirectory = config.downloadDirectory || '';

  const selectQuality = document.getElementById('select-quality');
  if (selectQuality && config.defaultQuality) {
    selectQuality.value = config.defaultQuality;
  }

  const selectAudio = document.getElementById('select-audio-format');
  if (selectAudio && config.defaultAudioFormat) {
    selectAudio.value = config.defaultAudioFormat;
  }

  const chkPlaylist = document.getElementById('chk-playlist');
  if (chkPlaylist) {
    chkPlaylist.checked = config.downloadPlaylistDefault;
  }

  const chkCookies = document.getElementById('chk-cookies');
  if (chkCookies) {
    chkCookies.checked = config.sendCookiesDefault !== false;
  }

  const clients = Array.isArray(config.selectedPlayerClients) ? config.selectedPlayerClients : ['android', 'web', 'ios'];
  document.querySelectorAll('input[name="yt-client"]').forEach(cb => {
    cb.checked = clients.includes(cb.value);
  });
}

// Setup all click and change listeners
function setupUIEventListeners() {
  // Reset client chips to recommended
  document.getElementById('btn-reset-clients')?.addEventListener('click', () => {
    const recommended = ['android', 'web', 'ios'];
    document.querySelectorAll('input[name="yt-client"]').forEach(cb => {
      cb.checked = recommended.includes(cb.value);
    });
  });
  // Format mode toggle (Video / Audio)
  const btnModeVideo = document.getElementById('mode-video');
  const btnModeAudio = document.getElementById('mode-audio');
  const groupVideo = document.getElementById('group-video-quality');
  const groupAudio = document.getElementById('group-audio-format');

  btnModeVideo.addEventListener('click', () => {
    isAudioMode = false;
    btnModeVideo.classList.add('active');
    btnModeAudio.classList.remove('active');
    groupVideo.classList.remove('hidden');
    groupAudio.classList.add('hidden');
  });

  btnModeAudio.addEventListener('click', () => {
    isAudioMode = true;
    btnModeAudio.classList.add('active');
    btnModeVideo.classList.remove('active');
    groupAudio.classList.remove('hidden');
    groupVideo.classList.add('hidden');
  });

  // Action buttons
  document.getElementById('btn-download').addEventListener('click', onDownloadClicked);
  document.getElementById('btn-reconnect').addEventListener('click', checkServerConnection);
  
  // Options page
  document.getElementById('btn-options').addEventListener('click', () => {
    if (crossBrowser.runtime.openOptionsPage) {
      crossBrowser.runtime.openOptionsPage();
    } else {
      window.open(crossBrowser.runtime.getURL('options/options.html'));
    }
  });

  // Copy URL button
  document.getElementById('btn-copy-url').addEventListener('click', async () => {
    if (currentTab?.url) {
      await navigator.clipboard.writeText(currentTab.url);
      const btn = document.getElementById('btn-copy-url');
      btn.style.color = '#10B981';
      setTimeout(() => {
        btn.style.color = '';
      }, 1200);
    }
  });
}

// Read URL & Title from active tab
async function inspectCurrentTab() {
  try {
    const [tab] = await crossBrowser.tabs.query({ active: true, currentWindow: true });
    currentTab = tab;

    const titleEl = document.getElementById('media-title');
    const urlEl = document.getElementById('media-url');
    const chkPlaylist = document.getElementById('chk-playlist');

    if (tab && tab.url) {
      titleEl.textContent = tab.title || 'Mídia da Aba Atual';
      urlEl.textContent = tab.url;

      // Auto-detect YouTube or other playlist parameter in URL
      if (tab.url.includes('list=') || tab.url.includes('/playlist') || tab.url.includes('album')) {
        chkPlaylist.checked = true;
      }
    } else {
      titleEl.textContent = 'Nenhuma aba ativa compatível detectada';
      urlEl.textContent = 'about:blank';
      document.getElementById('btn-download').disabled = true;
    }
  } catch (err) {
    console.error('Erro ao inspecionar aba ativa:', err);
  }
}

// Ping local HTTP bridge server
async function checkServerConnection() {
  const statusPill = document.getElementById('status-pill');
  const statusText = document.getElementById('status-text');
  const alertBanner = document.getElementById('disconnected-banner');
  const btnDownload = document.getElementById('btn-download');

  statusPill.className = 'status-pill checking';
  statusText.textContent = 'Conectando...';

  try {
    const response = await fetch(`http://127.0.0.1:${serverPort}/api/ping`, {
      method: 'GET',
      headers: { 'Accept': 'application/json' }
    });

    if (response.ok) {
      isConnected = true;
      statusPill.className = 'status-pill online';
      statusText.textContent = 'Conectado';
      alertBanner.classList.add('hidden');
      btnDownload.disabled = false;

      // Fetch queue summary stats
      await fetchQueueStats();
      return;
    }
  } catch (err) {
    // Offline
  }

  isConnected = false;
  statusPill.className = 'status-pill offline';
  statusText.textContent = 'Desconectado';
  alertBanner.classList.remove('hidden');
}

// Fetch active download stats
async function fetchQueueStats() {
  try {
    const res = await fetch(`http://127.0.0.1:${serverPort}/api/status`);
    if (res.ok) {
      const stats = await res.json();
      if (stats) {
        document.getElementById('stat-active').textContent = stats.active ?? 0;
        document.getElementById('stat-queued').textContent = stats.queued ?? 0;
        document.getElementById('stat-completed').textContent = stats.completed ?? 0;
      }
    }
  } catch {
    // Ignore stats polling failure
  }
}

// Trigger download request
async function onDownloadClicked() {
  if (!currentTab || !currentTab.url) return;

  const btn = document.getElementById('btn-download');
  const btnText = document.getElementById('btn-text');
  const quality = document.getElementById('select-quality').value;
  const audioFormat = document.getElementById('select-audio-format').value;
  const playlist = document.getElementById('chk-playlist').checked;
  const sendCookies = document.getElementById('chk-cookies')?.checked ?? true;

  btn.disabled = true;
  btnText.textContent = 'Enviando...';

  let cookiesText = '';
  if (sendCookies && typeof getNetscapeCookiesForUrl === 'function') {
    try {
      cookiesText = await getNetscapeCookiesForUrl(currentTab.url);
    } catch (e) {
      console.warn('Falha ao exportar cookies:', e);
    }
  }

  const selectedClients = Array.from(document.querySelectorAll('input[name="yt-client"]:checked'))
    .map(cb => cb.value);
  const playerClients = selectedClients.length > 0 ? selectedClients.join(',') : undefined;

  const payload = {
    url: currentTab.url.trim(),
    title: currentTab.title || currentTab.url,
    quality: isAudioMode ? 'Best' : quality,
    audioFormat: isAudioMode ? audioFormat : 'None',
    audioOnly: isAudioMode,
    playlist: playlist,
    downloadDirectory: downloadDirectory || undefined,
    cookiesText: cookiesText || undefined,
    playerClients: playerClients
  };

  try {
    const response = await fetch(`http://127.0.0.1:${serverPort}/api/download`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(payload)
    });

    if (response.ok) {
      btn.classList.add('success');
      btnText.textContent = '✓ Download Iniciado!';
      
      await fetchQueueStats();

      setTimeout(() => {
        btn.classList.remove('success');
        btnText.textContent = 'Baixar no YoutubeDL-GUI';
        btn.disabled = false;
      }, 2200);
    } else {
      const err = await response.json().catch(() => ({}));
      alert(err.message || 'Erro ao iniciar download no aplicativo.');
      btnText.textContent = 'Baixar no YoutubeDL-GUI';
      btn.disabled = false;
    }
  } catch (error) {
    alert('Não foi possível se comunicar com o YoutubeDL-GUI.\nCertifique-se de que o aplicativo desktop está em execução.');
    btnText.textContent = 'Baixar no YoutubeDL-GUI';
    btn.disabled = false;
    await checkServerConnection();
  }
}
