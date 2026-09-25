# Padrão de Arquitetura: Distribuição em Rede Local (SMB) e Auto-Atualização Nativa para Aplicações Desktop .NET

Este documento estabelece o **padrão de desenvolvimento e publicação** consolidado no projeto **yt-dlp GUI Modern**, servindo como **guia arquitetural de referência** para novos projetos e aplicações desktop (WPF, WinForms, WinUI, Avalonia, .NET MAUI) que necessitem de distribuição centralizada em rede local (CIFS/SMB) com auto-atualização contínua, alta performance e compatibilidade total com os mecanismos de segurança do Windows 10 e 11.

---

## 1. Visão Geral e Princípios Arquiteturais

### 1.1 O Padrão: Repositório Central SMB + Cache Local + In-App Auto-Updater

Tradicionalmente, a distribuição de softwares em rede local esbarra em problemas críticos:
1. **Travamento de Arquivo em Uso (*File Locking*):** Se a aplicação for executada diretamente de um compartilhamento de rede (`\\servidor\share\app.exe`), o Windows trava o arquivo em memória. O desenvolvedor fica impedido de compilar ou publicar novas versões enquanto houver estações com o programa aberto.
2. **Incompatibilidade com a Barra de Tarefas:** Se a atualização depender de scripts iniciadores externos (`.vbs`/`.cmd`), o usuário inevitavelmente fixará o `.exe` na Barra de Tarefas do Windows, ignorando o iniciador nas próximas execuções.
3. **Bloqueios de Segurança (Smart App Control & Mark of the Web):** Softwares sincronizados via nuvem (ex: OneDrive) recebem fluxos NTFS `ZoneId=3` (Internet), acionando bloqueios severos do Smart App Control do Windows 11.
4. **Dependência de Conexão Constante:** Se a rede oscilar ou um notebook for retirado da empresa/casa, aplicações que rodam direto da rede travam instantaneamente.

### 1.2 A Solução Arquitetural
```
 ┌───────────────────────────────────────────────────────────────────────────┐
 │                   ESTAÇÃO DO DESENVOLVEDOR / CI-CD                        │
 │  • Incrementa versão no .csproj                                           │
 │  • dotnet publish (Single-File, sem .pdb, otimizado)                      │
 │  • Assinatura Authenticode SHA-256                                        │
 │  • Gera version.json + Instalar.ps1                                       │
 └─────────────────────────────────────┬─────────────────────────────────────┘
                                       │ (publish-net.ps1 via robocopy)
                                       ▼
 ┌───────────────────────────────────────────────────────────────────────────┐
 │               COMPARTILHAMENTO DE REDE (CIFS / SMB)                       │
 │               Ex: \\servidor\Compartilhar\Apps\<AppName>\                 │
 │  ├── AppName.exe          (Binário Single-File oficial)                   │
 │  ├── version.json         (Manifesto: versão, data, hash, share)          │
 │  ├── Instalar.ps1         (Instalador 1-clique para novos PCs)            │
 │  └── Extensoes/Recursos   (Pacotes adicionais se houver)                  │
 └─────────────────────────────────────┬─────────────────────────────────────┘
                                       │
                  ┌────────────────────┴────────────────────┐
                  │ (Instalação 1x via Instalar.ps1)        │ (Verificação In-App)
                  ▼                                         ▼
 ┌───────────────────────────────────┐     ┌───────────────────────────────────┐
 │       ESTAÇÃO DE TRABALHO 1       │     │       ESTAÇÃO DE TRABALHO 2       │
 │  %LOCALAPPDATA%\Programs\<App>\   │     │  %LOCALAPPDATA%\Programs\<App>\   │
 │  • Executa local (SSD/NVMe)       │     │  • Executa local (SSD/NVMe)       │
 │  • Atalho no Desktop / Taskbar    │     │  • Atalho no Desktop / Taskbar    │
 │  • In-App Background Update Check │     │  • In-App Background Update Check │
 └───────────────────────────────────┘     └───────────────────────────────────┘
```

---

## 2. Passo a Passo de Implementação em Novos Projetos

### Passo 1: Configuração do Projeto (.csproj)

Configure o arquivo `.csproj` da aplicação para empacotamento em arquivo único (*Single-File*) e supressão de arquivos de depuração em `Release`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWPF>true</UseWPF>
    
    <!-- Versionamento Semântico gerenciado pelo publicador -->
    <Version>1.0.0</Version>
    
    <!-- Garante que bibliotecas nativas sejam embutidas no executável -->
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
  </PropertyGroup>

  <!-- Otimização estrita para Produção (Sem PDBs) -->
  <PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <DebugSymbols>false</DebugSymbols>
    <DebugType>none</DebugType>
    <CopyOutputSymbolsToPublishDirectory>false</CopyOutputSymbolsToPublishDirectory>
  </PropertyGroup>

</Project>
```

---

### Passo 2: Contrato do Serviço de Atualização (`INetworkUpdateService.cs`)

Crie a interface na camada de abstrações/Core (`Core/Interfaces/INetworkUpdateService.cs`):

```csharp
namespace MeuProjeto.Core.Interfaces;

/// <summary>
/// Serviço responsável pela verificação e aplicação de atualizações em rede local.
/// </summary>
public interface INetworkUpdateService
{
    /// <summary>Indica se há versão mais recente disponível no servidor.</summary>
    bool IsUpdateAvailable { get; }

    /// <summary>Versão mais recente disponível (ex: "v1.1.0").</summary>
    string AvailableVersion { get; }

    /// <summary>Caminho UNC do repositório de rede configurado.</summary>
    string NetworkRepositoryPath { get; }

    /// <summary>Verificação assíncrona e não-bloqueante de novas versões.</summary>
    Task<bool> CheckForUpdatesAsync(CancellationToken cancellationToken = default);

    /// <summary>Aplica a atualização do servidor e reinicia o aplicativo.</summary>
    Task<bool> ApplyUpdateAndRestartAsync();
}
```

---

### Passo 3: Implementação do Serviço (`NetworkUpdateService.cs`)

Implemente a lógica na camada de serviços (`Services/NetworkUpdateService.cs`). Este serviço lê o manifesto `version.json` local/remoto e executa o *self-replace* atômico:

```csharp
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using MeuProjeto.Core.Interfaces;

namespace MeuProjeto.Services;

public class NetworkUpdateService : INetworkUpdateService
{
    // Caminho padrão de fallback caso version.json não contenha a chave networkShare
    private const string DefaultNetworkShare = @"\\meuservidor\Compartilhar\Apps\MeuProjeto";

    public bool IsUpdateAvailable { get; private set; }
    public string AvailableVersion { get; private set; } = string.Empty;
    public string NetworkRepositoryPath { get; private set; } = DefaultNetworkShare;

    public NetworkUpdateService()
    {
        ResolveNetworkRepositoryPath();
    }

    private void ResolveNetworkRepositoryPath()
    {
        try
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string localVersionFile = Path.Combine(baseDir, "version.json");
            if (File.Exists(localVersionFile))
            {
                string json = File.ReadAllText(localVersionFile);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("networkShare", out var shareElem))
                {
                    string? share = shareElem.GetString();
                    if (!string.IsNullOrWhiteSpace(share))
                    {
                        NetworkRepositoryPath = share.Trim();
                    }
                }
            }
        }
        catch
        {
            // Mantém DefaultNetworkShare em caso de erro de leitura
        }
    }

    public async Task<bool> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NetworkRepositoryPath) || !Directory.Exists(NetworkRepositoryPath))
                {
                    return false;
                }

                string remoteVersionFile = Path.Combine(NetworkRepositoryPath, "version.json");
                if (!File.Exists(remoteVersionFile))
                {
                    return false;
                }

                string json = File.ReadAllText(remoteVersionFile);
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("version", out var verElem))
                {
                    return false;
                }

                string? remoteVerStr = verElem.GetString();
                if (string.IsNullOrWhiteSpace(remoteVerStr))
                {
                    return false;
                }

                Version currentVer = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(1, 0, 0);
                if (Version.TryParse(remoteVerStr, out var remoteVer))
                {
                    if (remoteVer > currentVer)
                    {
                        IsUpdateAvailable = true;
                        AvailableVersion = $"v{remoteVer.Major}.{remoteVer.Minor}.{Math.Max(0, remoteVer.Build)}";
                        return true;
                    }
                }
            }
            catch
            {
                // Falha de rede ou servidor offline é tratada silenciosamente sem travar o app
            }

            return false;
        }, cancellationToken);
    }

    public async Task<bool> ApplyUpdateAndRestartAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');
                string exeName = Process.GetCurrentProcess().MainModule?.ModuleName ?? "MeuProjeto.exe";
                string exePath = Path.Combine(baseDir, exeName);
                int currentPid = Environment.ProcessId;

                // Gera script auxiliar em pasta temporária
                string updaterScriptPath = Path.Combine(Path.GetTempPath(), $"AppUpdate_{Guid.NewGuid():N}.cmd");

                string scriptContent = $@"@echo off
setlocal
timeout /t 1 /nobreak >nul
taskkill /F /PID {currentPid} >nul 2>&1
timeout /t 1 /nobreak >nul

robocopy ""{NetworkRepositoryPath}"" ""{baseDir}"" *.* /XO /FFT /R:2 /W:2 /NP >nul 2>&1
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-ChildItem '{baseDir}' -Recurse -File | Unblock-File -ErrorAction SilentlyContinue"" >nul 2>&1

start """" ""{exePath}""
del ""%~f0"" >nul 2>&1
";

                File.WriteAllText(updaterScriptPath, scriptContent);

                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{updaterScriptPath}\"",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true
                };

                Process.Start(psi);
                Environment.Exit(0);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao aplicar atualização: {ex.Message}");
                return false;
            }
        });
    }
}
```

---

### Passo 4: Integração com Dependency Injection e ViewModel

1. **No `App.xaml.cs`:**
```csharp
services.AddSingleton<INetworkUpdateService, NetworkUpdateService>();
```

2. **No `MainViewModel.cs`:**
```csharp
public partial class MainViewModel : ObservableObject
{
    private readonly INetworkUpdateService _networkUpdateService;

    [ObservableProperty]
    private bool _isUpdateAvailable;

    [ObservableProperty]
    private string _availableVersion = string.Empty;

    public MainViewModel(INetworkUpdateService networkUpdateService)
    {
        _networkUpdateService = networkUpdateService;

        // Dispara checagem não-bloqueante em background na inicialização
        _ = CheckForUpdatesInBackgroundAsync();
    }

    private async Task CheckForUpdatesInBackgroundAsync()
    {
        try
        {
            if (await _networkUpdateService.CheckForUpdatesAsync())
            {
                IsUpdateAvailable = true;
                AvailableVersion = _networkUpdateService.AvailableVersion;
            }
        }
        catch { }
    }

    [RelayCommand]
    private async Task ApplyUpdateAsync()
    {
        var confirm = MessageBox.Show(
            $"Deseja reiniciar a aplicação agora para aplicar a versão {AvailableVersion}?",
            "Atualização Disponível",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm == MessageBoxResult.Yes)
        {
            await _networkUpdateService.ApplyUpdateAndRestartAsync();
        }
    }
}
```

3. **Na View (`MainWindow.xaml`):**
Adicione o botão/badge vinculado a `IsUpdateAvailable` na barra de status:
```xml
<Button Command="{Binding ApplyUpdateCommand}" 
        Visibility="{Binding IsUpdateAvailable, Converter={StaticResource BooleanToVisibilityConverter}}"
        Background="#2E7D32" Foreground="White" BorderThickness="0"
        Cursor="Hand" Margin="0,0,12,0" Padding="8,3">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="⬆️ Atualização disponível (" FontSize="11" FontWeight="SemiBold"/>
        <TextBlock Text="{Binding AvailableVersion}" FontSize="11" FontWeight="Bold" Foreground="#A5D6A7"/>
        <TextBlock Text=") - Atualizar" FontSize="11" FontWeight="SemiBold"/>
    </StackPanel>
</Button>
```

---

### Passo 5: Script de Publicação Automatizada (`publish-net.ps1`)

Crie o script de publicação na raiz do repositório. Ele orquestra o versionamento, compilação, assinatura e publicação na rede:

```powershell
<#
.SYNOPSIS
    Script de publicação contínua em rede local (SMB) para aplicações .NET.
#>
[CmdletBinding()]
param(
    [Parameter(Position = 0, Mandatory = $false)]
    [string]$NetworkShare = "\\meuservidor\Compartilhar\Apps\MeuProjeto",

    [Parameter(Position = 1, Mandatory = $false)]
    [string]$ClientInstallDir = "$env:LOCALAPPDATA\Programs\MeuProjeto",

    [Parameter(Mandatory = $false)]
    [string]$Configuration = "Release",

    [Parameter(Mandatory = $false)]
    [ValidateSet("Patch", "Minor", "Major", "Revision")]
    [string]$IncrementType = "Patch",

    [Parameter(Mandatory = $false)]
    [string]$CustomVersion = "",

    [Parameter(Mandatory = $false)]
    [switch]$SkipVersionIncrement = $false,

    [Parameter(Mandatory = $false)]
    [switch]$SkipCodeSigning = $false,

    [Parameter(Mandatory = $false)]
    [bool]$InstallLocally = $true
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectFile = Join-Path $ScriptDir "MeuProjeto\MeuProjeto.csproj"

# 1. Incremento de Versão no .csproj
$CsprojContent = Get-Content -Path $ProjectFile -Raw -Encoding UTF8
$CurrentVersion = if ($CsprojContent -match '<Version>(.*?)</Version>') { $Matches[1].Trim() } else { "1.0.0" }
$NewVersion = $CurrentVersion

if (-not [string]::IsNullOrWhiteSpace($CustomVersion)) {
    $NewVersion = $CustomVersion.Trim()
} elseif (-not $SkipVersionIncrement) {
    $Parts = $CurrentVersion.Split('.')
    $Major = [int]$Parts[0]; $Minor = [int]$Parts[1]; $Patch = [int]$Parts[2]
    switch ($IncrementType) {
        "Major" { $Major++; $Minor = 0; $Patch = 0; $NewVersion = "$Major.$Minor.$Patch" }
        "Minor" { $Minor++; $Patch = 0; $NewVersion = "$Major.$Minor.$Patch" }
        default { $Patch++; $NewVersion = "$Major.$Minor.$Patch" }
    }
}
$UpdatedCsproj = [regex]::Replace($CsprojContent, '<Version>(.*?)</Version>', "<Version>$NewVersion</Version>")
[System.IO.File]::WriteAllText($ProjectFile, $UpdatedCsproj, [System.Text.Encoding]::UTF8)

# 2. Área de Staging e dotnet publish
$StagingDir = Join-Path $env:TEMP "NetStaging_$(Get-Random)"
New-Item -ItemType Directory -Path $StagingDir -Force | Out-Null

& dotnet publish "$ProjectFile" `
    -c $Configuration `
    -r win-x64 `
    -p:PublishSingleFile=true `
    -p:DebugType=none `
    -p:DebugSymbols=false `
    -p:CopyOutputSymbolsToPublishDirectory=false `
    --self-contained false `
    -o "$StagingDir" `
    --nologo

# Limpar PDBs residuais
Get-ChildItem -Path $StagingDir -Recurse -Include "*.pdb" -File | Remove-Item -Force -ErrorAction SilentlyContinue

# 3. Desbloqueio e Assinatura Authenticode SHA-256
Get-ChildItem -Path $StagingDir -Recurse -File | ForEach-Object { Unblock-File -Path $_.FullName -ErrorAction SilentlyContinue }

if (-not $SkipCodeSigning) {
    $Cert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert | Select-Object -First 1
    if ($Cert) {
        Get-ChildItem -Path $StagingDir -Include *.exe, *.dll -Recurse | ForEach-Object {
            Set-AuthenticodeSignature -FilePath $_.FullName -Certificate $Cert -HashAlgorithm SHA256 | Out-Null
        }
    }
}

# 4. Geração de version.json e Instalar.ps1
$Sha256 = (Get-FileHash -Path (Join-Path $StagingDir "MeuProjeto.exe") -Algorithm SHA256).Hash
@{
    version      = $NewVersion
    releaseDate  = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
    executable   = "MeuProjeto.exe"
    sha256       = $Sha256
    networkShare = $NetworkShare
} | ConvertTo-Json | Set-Content (Join-Path $StagingDir "version.json") -Encoding UTF8

# Gerar Instalar.ps1 para clientes
$InstallerScript = @"
`$ErrorActionPreference = "Stop"
`$TargetDir = "`$env:LOCALAPPDATA\Programs\MeuProjeto"
if (-not (Test-Path `$TargetDir)) { New-Item -ItemType Directory -Path `$TargetDir -Force | Out-Null }
Get-Process -Name "MeuProjeto" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
robocopy "`$PSScriptRoot" "`$TargetDir" *.* /XO /FFT /R:2 /W:2 /NDL /NFL /NJH /NJS | Out-Null
Get-ChildItem -Path `$TargetDir -Recurse -File | ForEach-Object { Unblock-File -Path `$_.FullName -ErrorAction SilentlyContinue }

`$WshShell = New-Object -ComObject WScript.Shell
`$Shortcut = `$WshShell.CreateShortcut([Environment]::GetFolderPath('Desktop') + '\MeuProjeto.lnk')
`$Shortcut.TargetPath = Join-Path `$TargetDir "MeuProjeto.exe"
`$Shortcut.Arguments = ""
`$Shortcut.WorkingDirectory = `$TargetDir
`$Shortcut.IconLocation = (Join-Path `$TargetDir "MeuProjeto.exe") + ",0"
`$Shortcut.Save()
Write-Host "Instalação concluída com sucesso!" -ForegroundColor Green
"@
Set-Content -Path (Join-Path $StagingDir "Instalar.ps1") -Value $InstallerScript -Encoding UTF8

# 5. Publicação no Servidor de Rede
if (-not (Test-Path -Path $NetworkShare)) { New-Item -ItemType Directory -Path $NetworkShare -Force | Out-Null }
robocopy "$StagingDir" "$NetworkShare" /E /XO /FFT /R:2 /W:2 /NDL /NFL /NJH /NJS | Out-Null

# 6. Atualização Local (Opcional)
if ($InstallLocally) {
    robocopy "$StagingDir" "$ClientInstallDir" /E /XO /FFT /R:2 /W:2 /NDL /NFL /NJH /NJS | Out-Null
    $WshShell = New-Object -ComObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut([Environment]::GetFolderPath('Desktop') + '\MeuProjeto.lnk')
    $Shortcut.TargetPath = Join-Path $ClientInstallDir "MeuProjeto.exe"
    $Shortcut.Arguments = ""
    $Shortcut.WorkingDirectory = $ClientInstallDir
    $Shortcut.Save()
}

Remove-Item -Path $StagingDir -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "Publicação v$NewVersion concluída com sucesso em: $NetworkShare" -ForegroundColor Green
```

---

## 3. Instalação e Experiência nas Estações de Trabalho

Em qualquer estação conectada à rede local:

1. O usuário executa uma única vez no PowerShell:
```powershell
powershell -ExecutionPolicy Bypass -File "\\meuservidor\Compartilhar\Apps\MeuProjeto\Instalar.ps1"
```
2. O instalador:
   - Cria `%LOCALAPPDATA%\Programs\MeuProjeto`.
   - Copia os binários.
   - Remove restrições de segurança do Windows (`Unblock-File`).
   - Cria o atalho na Área de Trabalho apontando diretamente para `MeuProjeto.exe`.
3. O usuário pode fixar o aplicativo na **Barra de Tarefas do Windows** ou no **Menu Iniciar** livremente.
4. Quando uma nova versão for publicada no servidor de rede, a aplicação exibirá automaticamente a notificação de atualização na próxima execução e aplicará o update em 1 clique.

---

## 4. Checklist de Adoção para Novos Projetos

- [ ] `.csproj` configurado com `PublishSingleFile=true`, `DebugType=none` e tag `<Version>`.
- [ ] Interface `INetworkUpdateService` e classe `NetworkUpdateService` adicionadas ao projeto.
- [ ] `NetworkUpdateService` registrado como Singleton no container de injeção de dependência.
- [ ] ViewModel principal disparando `CheckForUpdatesAsync()` em background no startup.
- [ ] Botão ou banner de atualização condicional implementado na View principal.
- [ ] Script de publicação `publish-net.ps1` parametrizado com o caminho UNC da rede.
- [ ] Atalho do cliente apontando diretamente para o `.exe` nativo (sem scripts `.vbs` intermediários).
- [ ] Testes unitários adicionados para validar a detecção de versão e resiliência quando o servidor de rede estiver offline.

