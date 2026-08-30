# Documentação Técnica: Resolução do Smart App Control, Armazenamento e Publicação

Este documento descreve as decisões arquiteturais, mitigações de segurança para o **Smart App Control (SAC)** do Windows 11, o fluxo de armazenamento resiliente de configurações e o funcionamento do pipeline de publicação automatizada com **Single-File**, **versionamento contínuo** e sincronização no **OneDrive**.

---

## 1. Problema: Bloqueios do Smart App Control (SAC)

### 1.1 Sintoma
A aplicação apresentava o erro:
> *"O Smart App Control bloqueou parte deste aplicativo"* (Referência Microsoft: [Smart App Control Support](https://support.microsoft.com/pt-BR/Windows/Security/Threat-Malware-Protection/smart-app-control-has-blocked-part-of-this-app)).

### 1.2 Causa Raiz
O Smart App Control (SAC) do Windows 11 monitora chamadas de execução e carregamento dinâmico de bibliotecas (.DLL):
1. **Falta de Reputação na Nuvem:** Binários recém-compilados localmente não possuem histórico na nuvem de inteligência da Microsoft (*Microsoft Intelligent Security Graph*).
2. **Carregamento de DLLs Múltiplas:** Na compilação tradicional multi-arquivo, o executável carrega dezenas de DLLs avulsas (`YoutubeDlGui.Core.dll`, `YoutubeDlGui.Services.dll`, etc.). O SAC intercepta esse carregamento dinâmico e bloqueia o processo com a mensagem de que *"bloqueou parte deste aplicativo"*.
3. **Certificados Autoassinados:** Certificados locais não são CAs comerciais públicas da lista raiz da Microsoft e, portanto, não fornecem bypass automático do SAC.

---

## 2. Solução para Execução, Empacotamento e Segurança

### 2.1 Publicação em Arquivo Único (*Single-File*)
Para eliminar o problema de carregamento de DLLs avulsas:
* O projeto [YoutubeDlGui.App.csproj](file:///c:/Users/marco/source/youtube-dl-gui/YoutubeDlGui.App/YoutubeDlGui.App.csproj) e o [publish.ps1](file:///c:/Users/marco/source/youtube-dl-gui/publish.ps1) agora publicam em modo **Single-File** (`-p:PublishSingleFile=true`).
* Todas as dependências gerenciadas são compactadas diretamente dentro do `YoutubeDlGui.App.exe` (~798 KB).
* Arquivos residuais (`.dll`, `.deps.json`, `.runtimeconfig.json`) são limpos automaticamente, restando apenas o executável principal e a engine `yt-dlp.exe`.

### 2.2 Otimização de Produção (Sem Arquivos `.pdb`)
* Em modo `Release`, a geração e cópia de símbolos de depuração foi suprimida (`-p:DebugType=none -p:DebugSymbols=false -p:CopyOutputSymbolsToPublishDirectory=false`).
* O diretório publicado contém apenas os binários de produção limpos e otimizados.

### 2.3 Resolução Dinâmica de Destino (OneDrive com Fallback Local)
O publicador [publish.ps1](file:///c:/Users/marco/source/youtube-dl-gui/publish.ps1) resolve dinamicamente o diretório de instalação na seguinte ordem de preferência:
1. **1ª Prioridade:** `$env:OneDriveConsumer\Aplicativos\YtDlpGui` (`C:\Users\<User>\OneDrive\Aplicativos\YtDlpGui`)
2. **2ª Prioridade:** `"$env:USERPROFILE\OneDrive\Aplicativos\YtDlpGui"`
3. **3ª Prioridade:** `$env:OneDrive\Aplicativos\YtDlpGui` (OneDrive Corporativo/Institucional)
4. **Fallback:** `"$env:LOCALAPPDATA\Programs\YtDlpGui"` (quando o OneDrive não estiver disponível)

#### Proteção contra Desidratação (*Files On-Demand*):
Ao publicar no OneDrive, o script aplica o atributo `attrib -u +p /s /d` (*Sempre manter neste dispositivo*), garantindo que o executável permaneça gravado localmente no disco e possa ser aberto mesmo offline.

### 2.4 Desbloqueio de Arquivos (*Mark of the Web*)
* **No `publish.ps1`:** Executa `Unblock-File` recursivamente em todos os arquivos publicados.
* **No `YtDlpEngineService.cs`:** Executa a chamada nativa `UnblockFile` (via `kernel32.dll:DeleteFileW`) imediatamente após o download ou atualização do `yt-dlp.exe` e `qjs.exe`.

### 2.5 Assinatura Digital Local Automática (Authenticode SHA-256)
* Cria/reutiliza o certificado `CN=YtDlpGui Local Development`.
* Registra o certificado em `Cert:\CurrentUser\Root` e `Cert:\CurrentUser\TrustedPublisher` (e no escopo de máquina local se elevado).
* Assina os executáveis com hash SHA-256.

---

## 3. Incremento Automático de Versão

O script gerencia o versionamento semântico do aplicativo automaticamente antes da compilação:
* **Leitura e Escrita:** Atualiza a tag `<Version>` dentro do `YoutubeDlGui.App.csproj`.
* **Exibição na UI:** A propriedade `AppVersion` em [MainViewModel.cs](file:///c:/Users/marco/source/youtube-dl-gui/YoutubeDlGui.App/ViewModels/MainViewModel.cs) exibe `vMajor.Minor.Build` na barra de status inferior da janela.
* **Tipos de Incremento Suportados:**
  - `Patch` (padrão): `2.0.0` $\rightarrow$ `2.0.1`
  - `Minor`: `2.0.5` $\rightarrow$ `2.1.0`
  - `Major`: `2.1.3` $\rightarrow$ `3.0.0`
  - `CustomVersion`: Define um valor exato (ex: `-CustomVersion "2.5.0"`).
  - `SkipVersionIncrement`: Publica mantendo a versão atual.

---

## 4. Arquitetura de Armazenamento e Sincronização de Configurações

O `JsonSettingsService.cs` gerencia a persistência desacoplada dos dados do usuário:

### 4.1 Hierarquia de Armazenamento
```
                   ┌──────────────────────────────────────────────────────────┐
                   │                     YtDlpGui Storage                     │
                   └─────────────────────────────┬────────────────────────────┘
                                                 │
                  ┌──────────────────────────────┴──────────────────────────────┐
                  ▼                                                             ▼
    ┌───────────────────────────┐                                 ┌───────────────────────────┐
    │ Binários da Aplicação     │                                 │ Configurações & Histórico │
    │ (OneDrive ou LocalAppData)│                                 │ (Sincronizado na Nuvem)   │
    ├───────────────────────────┤                                 ├───────────────────────────┤
    │ • YoutubeDlGui.App.exe    │                                 │ • OneDrive/Aplicativos/   │
    │   (Single-File ~798 KB)   │                                 │   YtDlpGui/Config/        │
    │ • yt-dlp.exe              │                                 │   • settings.json         │
    │ • Logs temporários        │                                 │   • history.json          │
    │                           │                                 │   • Backups (.json.bak)   │
    └───────────────────────────┘                                 └───────────────────────────┘
```

### 4.2 Escrita Atômica Segura (*Atomic Save Pattern*)
1. Gravação inicial em arquivo temporário (`settings.json.tmp`).
2. *Flush* síncrono e forçado no disco (`stream.FlushAsync()`).
3. Cópia de segurança gerada automaticamente (`settings.json.bak`).
4. Substituição atômica (`File.Move(..., overwrite: true)`).

### 4.3 Resiliência e Auto-Recuperação (*Self-Healing*)
* **Leitura Compartilhada (`FileShare.ReadWrite`):** Permite leitura sem conflito de compartilhamento com a sincronização do OneDrive.
* **Retentativas:** Até 5 tentativas com intervalo exponencial (*exponential backoff* de 50ms a 400ms).
* **Auto-Recuperação:** Em caso de corrupção do JSON principal, restaura automaticamente o backup `.bak`.

---

## 5. Guia de Uso do Publicador (`publish.ps1`)

| Objetivo | Comando |
| :--- | :--- |
| **Publicação Padrão** *(Single-File, OneDrive/Fallback, Incremento Patch)* | `.\publish.ps1` |
| **Publicar Mantendo a Versão Atual** | `.\publish.ps1 -SkipVersionIncrement` |
| **Incrementar Versão Menor (Minor)** | `.\publish.ps1 -IncrementType Minor` |
| **Definir Versão Manualmente** | `.\publish.ps1 -CustomVersion "2.5.0"` |
| **Publicar 100% Autocontido** *(com runtime .NET embutido)* | `.\publish.ps1 -SelfContained` |
| **Publicar em Diretório Customizado** | `.\publish.ps1 -TargetDirectory "D:\Ferramentas\YtDlpGui"` |
| **Ignorar Assinatura Digital (CI/CD)** | `.\publish.ps1 -SkipCodeSigning` |

---

## 6. Configuração do Smart App Control para Desenvolvedores

Em ambientes de desenvolvimento no Windows 11 com o Smart App Control ativo:
1. Abra o menu Iniciar e digite **Segurança do Windows**.
2. Acesse **Controle de aplicativos e do navegador** $\rightarrow$ **Configurações do Controle de Aplicativo Inteligente**.
3. Defina como **Desativado** (ou **Avaliação**).


