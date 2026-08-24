/**
 * YoutubeDL-GUI Options Page JavaScript
 */

const crossBrowser = typeof browser !== 'undefined' ? browser : chrome;
const DEFAULT_PORT = 48190;
let detectedDownloadsFolder = '';

document.addEventListener('DOMContentLoaded', async () => {
  await loadOptions();
  setupEventListeners();
});

async function loadOptions() {
  const data = await crossBrowser.storage.local.get({
    serverPort: DEFAULT_PORT,
    defaultQuality: 'Best',
    defaultAudioFormat: 'Mp3',
    downloadPlaylistDefault: false,
    showNotifications: true,
    downloadDirectory: ''
  });

  const port = data.serverPort || DEFAULT_PORT;
  document.getElementById('server-port').value = port;
  document.getElementById('default-quality').value = data.defaultQuality || 'Best';
  document.getElementById('default-audio-format').value = data.defaultAudioFormat || 'Mp3';
  document.getElementById('chk-playlist-default').checked = data.downloadPlaylistDefault || false;
  document.getElementById('chk-notifications').checked = data.showNotifications !== false;

  const dirInput = document.getElementById('download-directory');
  if (data.downloadDirectory) {
    dirInput.value = data.downloadDirectory;
  }

  // Attempt to fetch default downloads folder from connected desktop app
  try {
    const res = await fetch(`http://127.0.0.1:${port}/api/status`);
    if (res.ok) {
      const stats = await res.json();
      if (stats && stats.defaultDownloadsFolder) {
        detectedDownloadsFolder = stats.defaultDownloadsFolder;
        dirInput.placeholder = stats.defaultDownloadsFolder;
        if (!data.downloadDirectory) {
          dirInput.value = stats.defaultDownloadsFolder;
        }
      }
    }
  } catch {
    // If offline, fallback to generic placeholder
    if (!dirInput.placeholder) {
      dirInput.placeholder = '%USERPROFILE%\\Downloads';
    }
  }
}

function setupEventListeners() {
  document.getElementById('btn-save').addEventListener('click', saveOptions);
  document.getElementById('btn-test-connection').addEventListener('click', testConnection);
  document.getElementById('btn-use-browser-downloads').addEventListener('click', () => {
    const dirInput = document.getElementById('download-directory');
    if (detectedDownloadsFolder) {
      dirInput.value = detectedDownloadsFolder;
    } else {
      dirInput.value = '%USERPROFILE%\\Downloads';
    }
  });
}

async function saveOptions() {
  const port = parseInt(document.getElementById('server-port').value, 10) || DEFAULT_PORT;
  const defaultQuality = document.getElementById('default-quality').value;
  const defaultAudioFormat = document.getElementById('default-audio-format').value;
  const downloadPlaylistDefault = document.getElementById('chk-playlist-default').checked;
  const showNotifications = document.getElementById('chk-notifications').checked;
  const downloadDirectory = document.getElementById('download-directory').value.trim();

  await crossBrowser.storage.local.set({
    serverPort: port,
    defaultQuality,
    defaultAudioFormat,
    downloadPlaylistDefault,
    showNotifications,
    downloadDirectory
  });

  const statusEl = document.getElementById('save-status');
  statusEl.textContent = '✓ Configurações salvas com sucesso!';
  setTimeout(() => {
    statusEl.textContent = '';
  }, 2500);
}

async function testConnection() {
  const port = parseInt(document.getElementById('server-port').value, 10) || DEFAULT_PORT;
  const resultEl = document.getElementById('test-result');

  resultEl.className = 'test-result';
  resultEl.textContent = 'Testando conexão com 127.0.0.1:' + port + '...';
  resultEl.classList.remove('hidden');

  try {
    const res = await fetch(`http://127.0.0.1:${port}/api/ping`);
    if (res.ok) {
      const data = await res.json();
      resultEl.className = 'test-result success';
      resultEl.textContent = `✓ Conexão bem-sucedida! Aplicativo "${data.app || 'YoutubeDlGui'}" detectado e pronto para downloads.`;
      
      // Update detected downloads folder
      const statusRes = await fetch(`http://127.0.0.1:${port}/api/status`);
      if (statusRes.ok) {
        const stats = await statusRes.json();
        if (stats && stats.defaultDownloadsFolder) {
          detectedDownloadsFolder = stats.defaultDownloadsFolder;
          const dirInput = document.getElementById('download-directory');
          dirInput.placeholder = stats.defaultDownloadsFolder;
          if (!dirInput.value) {
            dirInput.value = stats.defaultDownloadsFolder;
          }
        }
      }
      return;
    }
  } catch (err) {
    // Offline
  }

  resultEl.className = 'test-result error';
  resultEl.textContent = `✕ Não foi possível conectar ao YoutubeDL-GUI na porta ${port}. Verifique se o aplicativo desktop está aberto.`;
}
