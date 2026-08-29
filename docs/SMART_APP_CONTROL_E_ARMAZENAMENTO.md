# Documentação Técnica: Resolução do Smart App Control e Arquitetura de Armazenamento

Este documento descreve as causas, decisões arquiteturais e melhorias implementadas no projeto **yt-dlp GUI Modern** para mitigar bloqueios de segurança do **Smart App Control (SAC)** do Windows 11 e estabelecer um armazenamento seguro, resiliente e sincronizado na nuvem (OneDrive) para configurações e histórico.

---

## 1. Problema: Bloqueios do Smart App Control (SAC)

### 1.1 Sintoma
A aplicação apresentava esporadicamente o erro:
> *"O Smart App Control bloqueou parte deste aplicativo"* (Código de referência da Microsoft: [Smart App Control Support](https://support.microsoft.com/pt-BR/Windows/Security/Threat-Malware-Protection/smart-app-control-has-blocked-part-of-this-app)).

O erro desaparecia temporariamente ao recompilar e publicar o projeto novamente via `publish.ps1`.

### 1.2 Causa Raiz
O Smart App Control do Windows 11 atua sob dois pilares: **Reputação na Nuvem da Microsoft** e **Assinatura Digital Válida (Authenticode)**. Se um binário não possuir reputação conhecida nem assinatura digital reconhecida, o SAC bloqueia a execução.

No projeto, isso ocorria por uma combinação de fatores:
1. **Publicação dentro do OneDrive:** O diretório padrão anterior (`$env:USERPROFILE\OneDrive\Aplicativos\YtDlpGui`) fazia com que o Windows marcasse os executáveis com *Mark of the Web* (`Zone.Identifier: ZoneId=3`), acionando regras muito mais estritas de bloqueio do SAC.
2. **Atualizações dinâmicas da engine (`yt-dlp.exe` / `qjs.exe`):** Binários recém-lançados baixados do GitHub possuem hashes novos sem reputação prévia na nuvem da Microsoft.
3. **Ausência de Assinatura Digital:** Os binários .NET gerados localmente não continham assinatura Authenticode.

---

## 2. Solução para Execução e Segurança dos Binários

### 2.1 Novo Diretório Padrão de Publicação
Os binários da aplicação foram migrados para o diretório de programas de usuário padrão do Windows:
* **Caminho:** `%LOCALAPPDATA%\Programs\YtDlpGui` (`C:\Users\<User>\AppData\Local\Programs\YtDlpGui`).
* **Benefício:** Elimina a interferência do mecanismo de sincronização do OneDrive e impede a inserção de metadados de rede (*Mark of the Web*) nos binários executáveis e DLLs.

### 2.2 Desbloqueio Automático de Arquivos (`Unblock-File`)
* **No `publish.ps1`:** Adicionada a rotina de desbloqueio recursivo para remover o Alternate Data Stream (`:Zone.Identifier`) de todos os binários publicados.
* **No `YtDlpEngineService.cs`:** Implementada a chamada nativa `UnblockFile` (via `kernel32.dll:DeleteFileW`) executada imediatamente após o download ou atualização do `yt-dlp.exe` e `qjs.exe`.

### 2.3 Assinatura Digital Local Automática (Authenticode SHA-256)
No `publish.ps1`, foi integrada uma rotina de assinatura digital:
* Cria e gerencia automaticamente um certificado local de assinatura de código (`CN=YtDlpGui Local Development`) no repositório `Cert:\CurrentUser\My`.
* Registra o certificado em *Editores Confiáveis* (`Cert:\CurrentUser\TrustedPublisher`).
* Assina todos os arquivos `.exe` e `.dll` (`YoutubeDlGui.App.exe`, `yt-dlp.exe`, `qjs.exe` e bibliotecas de dependência) com hash SHA-256.

---

## 3. Arquitetura de Armazenamento e Sincronização de Configurações

Para atender às melhores práticas de mercado e manter as configurações e o histórico de downloads sincronizados entre dispositivos via **OneDrive**, sem comprometer a estabilidade da aplicação, foram aplicadas as seguintes soluções no `JsonSettingsService.cs`:

### 3.1 Hierarquia de Armazenamento
```
                   ┌──────────────────────────────────────────────────────────┐
                   │                     YtDlpGui Storage                     │
                   └─────────────────────────────┬────────────────────────────┘
                                                 │
                  ┌──────────────────────────────┴──────────────────────────────┐
                  ▼                                                             ▼
    ┌───────────────────────────┐                                 ┌───────────────────────────┐
    │ Binários & Cache Local    │                                 │ Configurações & Histórico │
    │ (%LOCALAPPDATA%)          │                                 │ (Sincronizado na Nuvem)   │
    ├───────────────────────────┤                                 ├───────────────────────────┤
    │ • YoutubeDlGui.App.exe    │                                 │ • OneDrive/Apps/YtDlpGui/ │
    │ • yt-dlp.exe / qjs.exe    │                                 │   Config/settings.json    │
    │ • Logs e caches temporários │                               │ • history.json            │
    └───────────────────────────┘                                 │ • Backups automáticos     │
                                                                  │   (.json.bak)             │
                                                                  └───────────────────────────┘
```

### 3.2 Detecção Inteligente e Multilíngue do OneDrive
O serviço localiza automaticamente a pasta do OneDrive ativa no sistema operacional:
1. Avalia as variáveis `OneDriveConsumer`, `OneDrive` e `OneDriveCommercial`.
2. Verifica se já existem as estruturas `OneDrive\Aplicativos\YtDlpGui\Config` ou `OneDrive\Apps\YtDlpGui\Config`.
3. Caso seja a primeira execução, cria a estrutura adequada respeitando o idioma do sistema (`Aplicativos` em PT-BR / `Apps` em outros idiomas).
4. Se o OneDrive não estiver configurado na máquina, utiliza o fallback seguro `%APPDATA%\YoutubeDlGui`.

### 3.3 Escrita Atômica Segura (*Atomic Save Pattern*)
Para evitar corrupção de arquivos ou arquivos vazios (0 bytes) caso a aplicação seja fechada repentinamente ou o OneDrive bloqueie o arquivo para sincronização:
1. Os dados são gravados primeiro em um arquivo temporário (`settings.json.tmp` / `history.json.tmp`).
2. É feito o *flush* síncrono e forçado no disco (`stream.FlushAsync()`).
3. Uma cópia do arquivo atual é mantida como backup (`settings.json.bak` / `history.json.bak`).
4. O arquivo `.tmp` substitui atomicamente o arquivo principal (`File.Move(..., overwrite: true)`).

### 3.4 Resiliência a Bloqueios de Concorrência e *Backoff* Exponencial
* **Leitura Compartilhada (`FileShare.ReadWrite`):** Ao ler os arquivos, o aplicativo permite que o OneDrive Sync Engine continue acessando o arquivo em paralelo sem lançar `IOException` de violação de compartilhamento.
* **Retentativas:** Em caso de trava momentânea, o serviço realiza até 5 tentativas de leitura/escrita com intervalo exponencial (*exponential backoff* de 50ms a 400ms).

### 3.5 Auto-Recuperação de Corrupção (*Self-Healing*)
Se um arquivo `.json` for corrompido por conflitos de sincronização entre múltiplas máquinas ou desligamento abrupto:
* O `JsonSettingsService` intercepta a falha de desserialização.
* Lê automaticamente a versão anterior a partir do `.bak`.
* Restaura a integridade do arquivo principal, evitando que o usuário perca suas configurações e histórico.

---

## 4. Resumo dos Arquivos Modificados

| Arquivo | Descrição das Mudanças |
| :--- | :--- |
| [`publish.ps1`](file:///c:/Users/marco/source/youtube-dl-gui/publish.ps1) | Publicação em `%LOCALAPPDATA%`, desbloqueio de rede (`Unblock-File`) e assinatura Authenticode SHA-256 com certificado local. |
| [`YtDlpEngineService.cs`](file:///c:/Users/marco/source/youtube-dl-gui/YoutubeDlGui.Services/YtDlpEngineService.cs) | Adição do método `UnblockFile` (Win32) para remoção do *Mark of the Web* nos binários `yt-dlp.exe` e `qjs.exe`. |
| [`JsonSettingsService.cs`](file:///c:/Users/marco/source/youtube-dl-gui/YoutubeDlGui.Services/JsonSettingsService.cs) | Escrita atômica, backup automático (`.bak`), leitura compartilhada (`FileShare.ReadWrite`), auto-recuperação e suporte inteligente ao OneDrive. |
| [`ISettingsService.cs`](file:///c:/Users/marco/source/youtube-dl-gui/YoutubeDlGui.Core/Interfaces/ISettingsService.cs) | Exposição das propriedades `StorageFolder` e `IsCloudSynced`. |
| [`ServiceTests.cs`](file:///c:/Users/marco/source/youtube-dl-gui/YoutubeDlGui.Tests/ServiceTests.cs) | Inclusão de testes unitários para escrita atômica, geração de `.bak` e recuperação contra JSONs corrompidos. |

---

## 5. Como Publicar o Projeto

Para compilar, assinar e publicar a versão mais recente com atalho atualizado na Área de Trabalho:

```powershell
.\publish.ps1
```

Para ignorar a etapa de assinatura em pipelines automatizados de CI/CD:
```powershell
.\publish.ps1 -SkipCodeSigning
```

