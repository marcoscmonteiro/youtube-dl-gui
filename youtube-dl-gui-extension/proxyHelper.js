/**
 * proxyHelper.js
 * Módulo para detecção e formatação de proxy do navegador (Firefox / Chrome / Chromium)
 * compatível com o parâmetro --proxy do yt-dlp.
 */

const proxyBrowserApi = typeof browser !== 'undefined' ? browser : (typeof chrome !== 'undefined' ? chrome : null);

/**
 * Detecta a configuração de proxy ativa no navegador para uma determinada URL.
 * @param {string} targetUrl - URL do vídeo/mídia a ser verificado
 * @returns {Promise<{ hasProxy: boolean, proxyUrl: string | null, formattedArg: string, description: string, mode: string }>}
 */
async function getBrowserProxyForUrl(targetUrl = '') {
  const result = {
    hasProxy: false,
    proxyUrl: null,
    formattedArg: '',
    description: 'Nenhum proxy detectado (Conexão direta)',
    mode: 'direct'
  };

  if (!proxyBrowserApi) {
    return result;
  }

  // 1. Inspecionar configurações de proxy do navegador (browser.proxy.settings / chrome.proxy.settings)
  if (proxyBrowserApi.proxy && proxyBrowserApi.proxy.settings && typeof proxyBrowserApi.proxy.settings.get === 'function') {
    try {
      const config = await new Promise((resolve) => {
        try {
          const ret = proxyBrowserApi.proxy.settings.get({ incognito: false }, (details) => {
            if (proxyBrowserApi.runtime && proxyBrowserApi.runtime.lastError) {
              resolve(null);
            } else {
              resolve(details);
            }
          });
          // Suporte a Promises do Firefox (WebExtensions API)
          if (ret && typeof ret.then === 'function') {
            ret.then(resolve).catch(() => resolve(null));
          }
        } catch {
          resolve(null);
        }
      });

      if (config && config.value) {
        const val = config.value;

        // --- A. Suporte ao Firefox (val.proxyType: "manual" | "none" | "system" | "autoDetect" | "autoConfig") ---
        if (val.proxyType !== undefined) {
          const pType = String(val.proxyType).toLowerCase();
          result.mode = pType;

          if (pType === 'none') {
            result.hasProxy = false;
            result.description = 'Conexão Direta (Sem proxy)';
            return result;
          }

          if (pType === 'manual') {
            const manualProxy = parseFirefoxManualProxy(val, targetUrl);
            if (manualProxy && manualProxy.hasProxy) {
              result.hasProxy = true;
              result.proxyUrl = manualProxy.proxyUrl;
              result.formattedArg = `--proxy ${manualProxy.proxyUrl}`;
              result.description = manualProxy.description;
              return result;
            } else if (manualProxy && manualProxy.description) {
              result.hasProxy = false;
              result.description = manualProxy.description;
              return result;
            }
          }

          if (pType === 'autoconfig') {
            result.hasProxy = false;
            result.description = val.autoConfigUrl ? `Script PAC (${val.autoConfigUrl})` : 'Script PAC automático';
          }

          if (pType === 'system') {
            result.hasProxy = false;
            result.description = 'Proxy padrão do Sistema (Windows)';
            return result;
          }

          if (pType === 'autodetect') {
            result.hasProxy = false;
            result.description = 'Detecção Automática (WPAD)';
            return result;
          }
        }

        // --- B. Suporte ao Chrome / Chromium (val.mode: "fixed_servers" | "direct" | "system" | "pac_script" | "auto_detect") ---
        if (val.mode !== undefined) {
          const mode = String(val.mode).toLowerCase();
          result.mode = mode;

          if (mode === 'direct') {
            result.hasProxy = false;
            result.description = 'Conexão Direta (Sem proxy)';
            return result;
          }

          if (mode === 'fixed_servers' && val.rules) {
            const proxyRule = selectChromeProxyRule(val.rules, targetUrl);
            if (proxyRule) {
              const scheme = (proxyRule.scheme || 'http').toLowerCase();
              const host = proxyRule.host;
              const port = proxyRule.port ? `:${proxyRule.port}` : '';
              const proxyUrl = `${scheme}://${host}${port}`;

              result.hasProxy = true;
              result.proxyUrl = proxyUrl;
              result.formattedArg = `--proxy ${proxyUrl}`;
              result.description = `${scheme.toUpperCase()} (${host}${port})`;
              return result;
            }
          }

          if (mode === 'pac_script' && val.pacScript) {
            result.hasProxy = false;
            result.description = val.pacScript.url ? `Script PAC (${val.pacScript.url})` : 'Script PAC automático';
          }

          if (mode === 'system') {
            result.hasProxy = false;
            result.description = 'Proxy padrão do Sistema (Windows)';
            return result;
          }

          if (mode === 'auto_detect') {
            result.hasProxy = false;
            result.description = 'Detecção Automática (WPAD)';
            return result;
          }
        }
      }
    } catch (err) {
      console.warn('Falha ao obter proxy.settings:', err);
    }
  }

  // 2. Fallback: Tentar Firefox findProxyForURL se disponível (especialmente útil em PAC ou autoConfig)
  if (proxyBrowserApi.findProxyForURL && typeof proxyBrowserApi.findProxyForURL === 'function') {
    try {
      const pacResult = await proxyBrowserApi.findProxyForURL(targetUrl || 'https://www.youtube.com');
      if (pacResult && typeof pacResult === 'string') {
        const parsed = parsePacString(pacResult);
        if (parsed.hasProxy && parsed.proxyUrl) {
          result.hasProxy = true;
          result.proxyUrl = parsed.proxyUrl;
          result.formattedArg = `--proxy ${parsed.proxyUrl}`;
          result.description = parsed.description;
          result.mode = 'pac';
          return result;
        }
      }
    } catch (e) {
      console.warn('Falha em findProxyForURL:', e);
    }
  }

  return result;
}

/**
 * Analisa o objeto de proxy manual do Firefox (val.proxyType === 'manual')
 * Propriedades possíveis: socks, socksPort, socksVersion, proxyDNS, http, httpPort, ssl, sslPort, passthrough
 */
function parseFirefoxManualProxy(val, targetUrl = '') {
  if (!val) return null;

  // Verificar se a URL atual está na lista de exceções (passthrough)
  if (targetUrl && val.passthrough && isUrlBypassed(targetUrl, val.passthrough)) {
    return {
      hasProxy: false,
      proxyUrl: null,
      description: 'URL na lista de exceções do proxy (Bypass)'
    };
  }

  // 1. SOCKS Proxy (prioritário se configurado e utilizado para todo o tráfego)
  if (val.socks && typeof val.socks === 'string' && val.socks.trim()) {
    let cleanAddress = val.socks.trim().replace(/^(socks5|socks4|socks|http|https):\/\//i, '');
    const port = val.socksPort || val.socks_port;
    if (!cleanAddress.includes(':') && port) {
      cleanAddress += `:${port}`;
    }

    const version = val.socksVersion === 4 ? 4 : 5;
    const scheme = version === 4 ? 'socks4' : 'socks5';
    const proxyUrl = `${scheme}://${cleanAddress}`;

    return {
      hasProxy: true,
      proxyUrl: proxyUrl,
      description: `SOCKS${version} (${cleanAddress})`
    };
  }

  const isHttps = targetUrl.startsWith('https://');

  // 2. SSL / HTTPS Proxy
  if (isHttps && val.ssl && typeof val.ssl === 'string' && val.ssl.trim()) {
    let clean = val.ssl.trim().replace(/^(https?):\/\//i, '');
    const port = val.sslPort || val.ssl_port;
    if (!clean.includes(':') && port) {
      clean += `:${port}`;
    }
    return {
      hasProxy: true,
      proxyUrl: `http://${clean}`,
      description: `HTTPS Proxy (${clean})`
    };
  }

  // 3. HTTP Proxy
  if (val.http && typeof val.http === 'string' && val.http.trim()) {
    let clean = val.http.trim().replace(/^(https?):\/\//i, '');
    const port = val.httpPort || val.http_port;
    if (!clean.includes(':') && port) {
      clean += `:${port}`;
    }
    return {
      hasProxy: true,
      proxyUrl: `http://${clean}`,
      description: `HTTP Proxy (${clean})`
    };
  }

  return null;
}

/**
 * Seleciona a regra de proxy apropriada no Chrome / Chromium
 */
function selectChromeProxyRule(rules, targetUrl = '') {
  if (!rules) return null;

  if (targetUrl && rules.bypassList && isUrlBypassed(targetUrl, rules.bypassList)) {
    return null;
  }

  if (rules.singleProxy && rules.singleProxy.host) {
    return rules.singleProxy;
  }

  const isHttps = targetUrl.startsWith('https://');
  if (isHttps && rules.proxyForHttps && rules.proxyForHttps.host) {
    return rules.proxyForHttps;
  }

  if (!isHttps && rules.proxyForHttp && rules.proxyForHttp.host) {
    return rules.proxyForHttp;
  }

  if (rules.fallbackProxy && rules.fallbackProxy.host) {
    return rules.fallbackProxy;
  }

  return null;
}

/**
 * Verifica se a URL alvo corresponde a uma regra de bypass/exceção
 */
function isUrlBypassed(targetUrl, passthrough) {
  if (!passthrough || !targetUrl) return false;
  try {
    const urlObj = new URL(targetUrl);
    const hostname = urlObj.hostname.toLowerCase();
    const bypassList = (Array.isArray(passthrough) ? passthrough : String(passthrough).split(/[,;]/))
      .map(s => s.trim().toLowerCase())
      .filter(Boolean);

    for (const rule of bypassList) {
      if (rule === '<local>' && (!hostname.includes('.') || hostname === 'localhost')) return true;
      if (rule === hostname) return true;
      if (rule.startsWith('.') && hostname.endsWith(rule)) return true;
      if (rule.startsWith('*.')) {
        const domain = rule.substring(2);
        if (hostname === domain || hostname.endsWith('.' + domain)) return true;
      }
    }
  } catch {}
  return false;
}

/**
 * Analisa uma string PAC retornada pelo Firefox (ex: "PROXY 192.168.1.1:8080; DIRECT")
 */
function parsePacString(pacString) {
  if (!pacString || typeof pacString !== 'string') {
    return { hasProxy: false };
  }

  const entries = pacString.split(';').map(s => s.trim()).filter(Boolean);
  for (const entry of entries) {
    const parts = entry.split(/\s+/);
    const type = parts[0]?.toUpperCase();
    const addr = parts[1];

    if (!addr || type === 'DIRECT') {
      continue;
    }

    if (type === 'PROXY' || type === 'HTTP') {
      return {
        hasProxy: true,
        proxyUrl: `http://${addr}`,
        description: `HTTP (${addr})`
      };
    }

    if (type === 'HTTPS') {
      return {
        hasProxy: true,
        proxyUrl: `https://${addr}`,
        description: `HTTPS (${addr})`
      };
    }

    if (type === 'SOCKS' || type === 'SOCKS4') {
      return {
        hasProxy: true,
        proxyUrl: `socks4://${addr}`,
        description: `SOCKS4 (${addr})`
      };
    }

    if (type === 'SOCKS5') {
      return {
        hasProxy: true,
        proxyUrl: `socks5://${addr}`,
        description: `SOCKS5 (${addr})`
      };
    }
  }

  return { hasProxy: false };
}
