<#
.SYNOPSIS
    Script de publicação em rede CIFS/SMB e distribuição da aplicação yt-dlp GUI Modern.

.DESCRIPTION
    Compila a aplicação em modo Single-File (sem arquivos de depuração .pdb), empacota as
    extensões para navegadores, incrementa a versão do projeto e disponibiliza o pacote
    completo em um compartilhamento de rede local (CIFS/SMB) com scripts de auto-atualização
    e instalador 1-clique para estações de trabalho.

.PARAMETER NetworkShare
    Caminho UNC ou pasta do compartilhamento de rede onde o pacote de distribuição será publicado.
    Padrão: "\\server.cm.dev.br\Compartilhar\Apps\YtDlpGui"

.PARAMETER ClientInstallDir
    Diretório padrão de instalação nas estações de trabalho locais.
    Padrão: "$env:LOCALAPPDATA\Programs\YtDlpGui"

.PARAMETER Configuration
    Configuração de compilação (.NET).
    Padrão: "Release"

.PARAMETER SkipCodeSigning
    Ignora a etapa de assinatura de código local.

.PARAMETER IncrementType
    Tipo de incremento de versão (Patch, Minor, Major, Revision).
    Padrão: "Patch"

.PARAMETER CustomVersion
    Define uma versão específica diretamente (ex: "2.1.0"), sobrepondo o incremento automático.

.PARAMETER SkipVersionIncrement
    Ignora a etapa de incremento de versão, mantendo a versão atual configurada no projeto.

.PARAMETER RuntimeIdentifier
    Identificador de runtime da plataforma alvo.
    Padrão: "win-x64"

.PARAMETER NoSingleFile
    Gera a publicação tradicional multi-arquivo ao invés do padrão Single-File.

.PARAMETER SelfContained
    Gera uma publicação totalmente autocontida (Single-File com runtime do .NET embutido).

.PARAMETER SkipEngineDownload
    Ignora o download das últimas versões do yt-dlp e QuickJS do GitHub, utilizando os binários locais existentes.

.PARAMETER InstallLocally
    Atualiza também a instalação local na máquina onde o script é executado.
    Padrão: $true

.EXAMPLE
    .\publish-net.ps1
    Publica no compartilhamento padrão (\\server.cm.dev.br\Compartilhar\Apps\YtDlpGui) com incremento patch.

.EXAMPLE
    .\publish-net.ps1 -NetworkShare "D:\Compartilhamentos\Apps\YtDlpGui"
    Publica no caminho de rede especificado.

.EXAMPLE
    .\publish-net.ps1 -IncrementType Minor
    Incrementa a versão menor (ex: 2.0.5 -> 2.1.0) e publica na rede.

.EXAMPLE
    .\publish-net.ps1 -CustomVersion "2.5.0"
    Define a versão explicitamente para 2.5.0 e publica.
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0, Mandatory = $false)]
    [string]$NetworkShare = "\\server.cm.dev.br\Compartilhar\Apps\YtDlpGui",

    [Parameter(Position = 1, Mandatory = $false)]
    [string]$ClientInstallDir = "$env:LOCALAPPDATA\Programs\YtDlpGui",

    [Parameter(Mandatory = $false)]
    [string]$Configuration = "Release",

    [Parameter(Mandatory = $false)]
    [switch]$SkipCodeSigning = $false,

    [Parameter(Mandatory = $false)]
    [ValidateSet("Patch", "Minor", "Major", "Revision")]
    [string]$IncrementType = "Patch",

    [Parameter(Mandatory = $false)]
    [string]$CustomVersion = "",

    [Parameter(Mandatory = $false)]
    [switch]$SkipVersionIncrement = $false,

    [Parameter(Mandatory = $false)]
    [string]$RuntimeIdentifier = "win-x64",

    [Parameter(Mandatory = $false)]
    [switch]$NoSingleFile = $false,

    [Parameter(Mandatory = $false)]
    [switch]$SelfContained = $false,

    [Parameter(Mandatory = $false)]
    [switch]$SkipEngineDownload = $false,

    [Parameter(Mandatory = $false)]
    [bool]$InstallLocally = $true
)

$ErrorActionPreference = "Stop"

$IsSingleFile = -not $NoSingleFile
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectFile = Join-Path $ScriptDir "YoutubeDlGui.App\YoutubeDlGui.App.csproj"

if (-not (Test-Path -Path $ProjectFile)) {
    Write-Error "Arquivo de projeto não encontrado em: $ProjectFile"
    exit 1
}

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   yt-dlp GUI Modern - Publicador para Rede Local (SMB)   " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# -----------------------------------------------------------------------------
# 1. Gerenciamento e Incremento de Versão
# -----------------------------------------------------------------------------
Write-Host "[1/6] Gerenciando versão da aplicação..." -ForegroundColor Yellow

$CsprojContent = Get-Content -Path $ProjectFile -Raw -Encoding UTF8
$CurrentVersion = "2.0.0"
$HasVersionTag = $false

if ($CsprojContent -match '<Version>(.*?)</Version>') {
    $CurrentVersion = $Matches[1].Trim()
    $HasVersionTag = $true
}

$NewVersion = $CurrentVersion

if (-not [string]::IsNullOrWhiteSpace($CustomVersion)) {
    $NewVersion = $CustomVersion.Trim()
    Write-Host "      Definindo versão personalizada: $NewVersion" -ForegroundColor Gray
}
elseif (-not $SkipVersionIncrement) {
    try {
        $Parts = $CurrentVersion.Split('.')
        $Major = if ($Parts.Count -ge 1 -and [int]::TryParse($Parts[0], [ref]$null)) { [int]$Parts[0] } else { 2 }
        $Minor = if ($Parts.Count -ge 2 -and [int]::TryParse($Parts[1], [ref]$null)) { [int]$Parts[1] } else { 0 }
        $Patch = if ($Parts.Count -ge 3 -and [int]::TryParse($Parts[2], [ref]$null)) { [int]$Parts[2] } else { 0 }
        $Revision = if ($Parts.Count -ge 4 -and [int]::TryParse($Parts[3], [ref]$null)) { [int]$Parts[3] } else { -1 }

        switch ($IncrementType) {
            "Major" {
                $Major++
                $Minor = 0
                $Patch = 0
                $NewVersion = if ($Revision -ge 0) { "$Major.$Minor.$Patch.0" } else { "$Major.$Minor.$Patch" }
            }
            "Minor" {
                $Minor++
                $Patch = 0
                $NewVersion = if ($Revision -ge 0) { "$Major.$Minor.$Patch.0" } else { "$Major.$Minor.$Patch" }
            }
            "Revision" {
                if ($Revision -ge 0) {
                    $Revision++
                    $NewVersion = "$Major.$Minor.$Patch.$Revision"
                } else {
                    $Patch++
                    $NewVersion = "$Major.$Minor.$Patch"
                }
            }
            default { # "Patch"
                $Patch++
                $NewVersion = if ($Revision -ge 0) { "$Major.$Minor.$Patch.$Revision" } else { "$Major.$Minor.$Patch" }
            }
        }
        Write-Host "      Incrementando versão ($IncrementType): $CurrentVersion -> $NewVersion" -ForegroundColor Green
    }
    catch {
        Write-Warning "      Não foi possível calcular o incremento automaticamente. Mantendo versão: $CurrentVersion"
        $NewVersion = $CurrentVersion
    }
}
else {
    Write-Host "      Incremento de versão ignorado (-SkipVersionIncrement). Versão atual: $CurrentVersion" -ForegroundColor Gray
}

# Atualizar o arquivo .csproj com a versão
if ($NewVersion -ne $CurrentVersion -or -not $HasVersionTag) {
    if ($HasVersionTag) {
        $UpdatedCsproj = [regex]::Replace($CsprojContent, '<Version>(.*?)</Version>', "<Version>$NewVersion</Version>")
    } else {
        $UpdatedCsproj = [regex]::Replace($CsprojContent, '(?s)(<PropertyGroup\b[^>]*>)', "`$1`r`n    <Version>$NewVersion</Version>")
    }
    [System.IO.File]::WriteAllText($ProjectFile, $UpdatedCsproj, [System.Text.Encoding]::UTF8)
    Write-Host "      Arquivo do projeto atualizado com a versão $NewVersion." -ForegroundColor Gray
}

# -----------------------------------------------------------------------------
# 2. Compilação e Empacotamento em Área de Staging
# -----------------------------------------------------------------------------
$StagingDir = Join-Path $env:TEMP "YtDlpGui_NetStaging_$(Get-Random)"
if (Test-Path $StagingDir) {
    Remove-Item -Path $StagingDir -Recurse -Force -ErrorAction SilentlyContinue
}
New-Item -ItemType Directory -Path $StagingDir -Force | Out-Null

$PublishModeDesc = if ($IsSingleFile) { if ($SelfContained) { "Single-File Self-Contained" } else { "Single-File Framework-Dependent" } } else { "Multi-File" }
Write-Host "[2/6] Compilando e publicando a aplicação v$NewVersion ($Configuration, $PublishModeDesc)..." -ForegroundColor Yellow

try {
    $PublishArgs = @(
        "publish", "$ProjectFile",
        "-c", $Configuration,
        "-r", $RuntimeIdentifier,
        "-p:PublishSingleFile=$IsSingleFile",
        "-p:DebugType=none",
        "-p:DebugSymbols=false",
        "-p:CopyOutputSymbolsToPublishDirectory=false",
        "--self-contained", $(if ($SelfContained) { "true" } else { "false" }),
        "-o", "$StagingDir",
        "--nologo"
    )
    & dotnet @PublishArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Falha durante o dotnet publish (Código de saída: $LASTEXITCODE)."
        exit $LASTEXITCODE
    }

    # Remover arquivos de símbolos residuais
    Get-ChildItem -Path $StagingDir -Recurse -Include "*.pdb" -File | Remove-Item -Force -ErrorAction SilentlyContinue
}
catch {
    Write-Error "Erro ao executar dotnet publish: $_"
    exit 1
}

# -----------------------------------------------------------------------------
# Obter as versões mais recentes das engines (yt-dlp e QuickJS)
# -----------------------------------------------------------------------------
$TargetYtDlp = Join-Path $StagingDir "yt-dlp.exe"
$TargetQjs = Join-Path $StagingDir "qjs.exe"

$YtDlpGitHubUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe"
$QuickJsGitHubUrl = "https://github.com/quickjs-ng/quickjs/releases/latest/download/qjs-windows-x86_64.exe"

[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12 -bor [System.Net.SecurityProtocolType]::Tls13

function Download-EngineFile {
    param(
        [string]$Url,
        [string]$DestinationPath,
        [string]$EngineName,
        [string]$FallbackSourcePath
    )

    Write-Host "      Baixando $EngineName mais recente do GitHub..." -ForegroundColor Cyan
    Write-Host "      URL: $Url" -ForegroundColor Gray

    $Success = $false
    try {
        $WebClient = New-Object System.Net.WebClient
        $WebClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) yt-dlp-gui")
        $WebClient.DownloadFile($Url, $DestinationPath)
        $WebClient.Dispose()

        if ((Test-Path -Path $DestinationPath) -and ((Get-Item $DestinationPath).Length -gt 0)) {
            $SizeMb = [Math]::Round((Get-Item $DestinationPath).Length / 1MB, 2)
            Write-Host "      [Sucesso] $EngineName baixado com sucesso ($SizeMb MB)." -ForegroundColor Green
            $Success = $true
        }
    }
    catch {
        Write-Warning "      Falha ao baixar $EngineName do GitHub: $_"
        if (Test-Path $DestinationPath) { Remove-Item -Path $DestinationPath -Force -ErrorAction SilentlyContinue }
    }

    if (-not $Success) {
        if ($FallbackSourcePath -and (Test-Path -Path $FallbackSourcePath)) {
            Write-Host "      Utilizando fallback local para $EngineName ($FallbackSourcePath)..." -ForegroundColor Yellow
            Copy-Item -Path $FallbackSourcePath -Destination $DestinationPath -Force
        }
        else {
            Write-Warning "      Nenhuma versão local pré-existente de $EngineName encontrada para fallback."
        }
    }
}

$DebugYtDlp = Join-Path $ScriptDir "YoutubeDlGui.App\bin\Debug\net8.0-windows\yt-dlp.exe"
$DebugQjs = Join-Path $ScriptDir "YoutubeDlGui.App\bin\Debug\net8.0-windows\qjs.exe"

if (-not $SkipEngineDownload) {
    # Download direto das últimas versões oficiais do GitHub
    Download-EngineFile -Url $YtDlpGitHubUrl -DestinationPath $TargetYtDlp -EngineName "yt-dlp.exe" -FallbackSourcePath $DebugYtDlp
    Download-EngineFile -Url $QuickJsGitHubUrl -DestinationPath $TargetQjs -EngineName "qjs.exe (QuickJS)" -FallbackSourcePath $DebugQjs
}
else {
    Write-Host "      Download de engines ignorado (-SkipEngineDownload). Verificando binários locais..." -ForegroundColor Gray
    if ((Test-Path -Path $DebugYtDlp) -and (-not (Test-Path -Path $TargetYtDlp))) {
        Copy-Item -Path $DebugYtDlp -Destination $TargetYtDlp -Force
    }
    if ((Test-Path -Path $DebugQjs) -and (-not (Test-Path -Path $TargetQjs))) {
        Copy-Item -Path $DebugQjs -Destination $TargetQjs -Force
    }
}

# Desbloquear arquivos na área de staging
Get-ChildItem -Path $StagingDir -Recurse -File | ForEach-Object {
    Unblock-File -Path $_.FullName -ErrorAction SilentlyContinue
}

# Assinatura digital local dos binários em staging
if (-not $SkipCodeSigning) {
    try {
        $CertSubject = "CN=YtDlpGui Local Development"
        $Cert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert -ErrorAction SilentlyContinue |
            Where-Object { $_.Subject -eq $CertSubject } |
            Select-Object -First 1

        if (-not $Cert) {
            $Cert = New-SelfSignedCertificate `
                -Type CodeSigningCert `
                -Subject $CertSubject `
                -CertStoreLocation "Cert:\CurrentUser\My" `
                -NotAfter (Get-Date).AddYears(5)

            $TempCertPath = Join-Path $env:TEMP "YtDlpGuiLocalDev.cer"
            Export-Certificate -Cert $Cert -FilePath $TempCertPath | Out-Null
            Import-Certificate -CertStoreLocation "Cert:\CurrentUser\TrustedPublisher" -FilePath $TempCertPath | Out-Null
            Import-Certificate -CertStoreLocation "Cert:\CurrentUser\Root" -FilePath $TempCertPath | Out-Null
            Remove-Item -Path $TempCertPath -Force -ErrorAction SilentlyContinue
        }

        if ($Cert) {
            $BinariesToSign = Get-ChildItem -Path $StagingDir -Include *.exe, *.dll -Recurse -File
            foreach ($file in $BinariesToSign) {
                Set-AuthenticodeSignature -FilePath $file.FullName -Certificate $Cert -HashAlgorithm SHA256 | Out-Null
            }
            Write-Host "      Binários assinados digitalmente com sucesso (SHA-256)." -ForegroundColor Green
        }
    }
    catch {
        Write-Warning "      Não foi possível aplicar a assinatura de código automática: $_"
    }
}

# -----------------------------------------------------------------------------
# 3. Empacotamento das Extensões para Navegadores
# -----------------------------------------------------------------------------
Write-Host "[3/6] Empacotando extensões para navegadores (Chrome, Edge, Firefox)..." -ForegroundColor Yellow

$ExtDir = Join-Path $ScriptDir "youtube-dl-gui-extension"
$ExtDistDir = Join-Path $ExtDir "dist"
$ExtBuildScript = Join-Path $ExtDir "build.ps1"

if (Test-Path $ExtBuildScript) {
    try {
        & powershell.exe -ExecutionPolicy Bypass -File "$ExtBuildScript" | Out-Null
    }
    catch {
        Write-Warning "      Aviso ao gerar extensões: $_"
    }
}

$StagingExtDir = Join-Path $StagingDir "Extensoes-Navegadores"
New-Item -ItemType Directory -Path $StagingExtDir -Force | Out-Null

if (Test-Path $ExtDistDir) {
    Get-ChildItem -Path "$ExtDistDir\*" -Include "*.zip", "*.xpi" -File | ForEach-Object {
        Copy-Item -Path $_.FullName -Destination $StagingExtDir -Force
    }
}

# Criar pasta descompactada pronta para "Carregar sem compactação" no Chrome/Edge
$UnpackedDir = Join-Path $StagingExtDir "chrome-descompactada"
New-Item -ItemType Directory -Path $UnpackedDir -Force | Out-Null
$ExtFilesToInclude = @("manifest.json", "background.js", "cookieHelper.js", "proxyHelper.js", "icons", "popup", "options")
foreach ($f in $ExtFilesToInclude) {
    $srcPath = Join-Path $ExtDir $f
    if (Test-Path $srcPath) {
        Copy-Item -Path $srcPath -Destination $UnpackedDir -Recurse -Force
    }
}

# Gerar arquivo com instruções claras de instalação das extensões
$InstructionsContent = @"
========================================================================
 GUIA DE INSTALACAO DA EXTENSAO YT-DLP GUI MODERN NOS NAVEGADORES
========================================================================

A extensao permite enviar videos do navegador diretamente para a fila
de downloads do yt-dlp GUI Modern com apenas 1 clique.

---
1. GOOGLE CHROME / MICROSOFT EDGE / BRAVE / OPERA:
---
Opcao A (Mais facil - Pasta descompactada):
  1. Abra o navegador e acesse a pagina de extensoes:
     - No Chrome: chrome://extensions
     - No Edge:   edge://extensions
     - No Brave:  brave://extensions
  2. Ative o "Modo de desenvolvedor" (Developer mode) no canto superior direito.
  3. Clique no botao "Carregar sem compactacao" (Load unpacked).
  4. Selecione a pasta "chrome-descompactada" presente neste diretorio.
  5. Pronto! A extensao estara instalada e ativa.

Opcao B (Arquivo .zip):
  1. Descompacte o arquivo "youtube-dl-gui-chrome.zip" em uma pasta de sua preferencia.
  2. Siga os mesmos passos acima selecionando a pasta descompactada.

---
2. MOZILLA FIREFOX:
---
  1. Abra o Firefox e acesse: about:debugging#/runtime/this-firefox
  2. Clique em "Carregar extensao temporaria..." (Load Temporary Add-on).
  3. Selecione o arquivo "youtube-dl-gui-firefox.xpi" ou "youtube-dl-gui-firefox.zip".
  4. A extensao sera carregada no navegador.

---
3. INTEGRACAO COM A APLICACAO:
---
  Certifique-se de que a opcao "Habilitar integracao com extensao do navegador"
  esteja marcada nas Configuracoes do yt-dlp GUI Modern (porta padrao: 9191).
========================================================================
"@
[System.IO.File]::WriteAllText((Join-Path $StagingExtDir "COMO-INSTALAR.txt"), $InstructionsContent, [System.Text.Encoding]::UTF8)
Write-Host "      Extensões empacotadas com sucesso em Extensoes-Navegadores." -ForegroundColor Green

# -----------------------------------------------------------------------------
# 4. Geração de Metadados e Instalador Cliente
# -----------------------------------------------------------------------------
Write-Host "[4/6] Gerando metadados de versão e instalador cliente..." -ForegroundColor Yellow

# 4.1 version.json
$ExePathInStaging = Join-Path $StagingDir "YoutubeDlGui.App.exe"
$Sha256Hash = if (Test-Path $ExePathInStaging) { (Get-FileHash -Path $ExePathInStaging -Algorithm SHA256).Hash } else { "" }
$VersionMetadata = @{
    version      = $NewVersion
    releaseDate  = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
    executable   = "YoutubeDlGui.App.exe"
    sha256       = $Sha256Hash
    networkShare = $NetworkShare
} | ConvertTo-Json -Depth 3
[System.IO.File]::WriteAllText((Join-Path $StagingDir "version.json"), $VersionMetadata, [System.Text.Encoding]::UTF8)

# 4.2 Instalar.ps1 (Instalador 1-clique para estações de trabalho da rede)
$InstalarPs1Content = @"
# =============================================================================
# Script de Instalacao do yt-dlp GUI Modern na Estacao de Trabalho
# =============================================================================
`$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   Instalacao do yt-dlp GUI Modern na Estacao Local       " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

`$SourceDir = `$PSScriptRoot
`$TargetDir = "`$env:LOCALAPPDATA\Programs\YtDlpGui"

Write-Host "[1/4] Preparando pasta de destino local: `$TargetDir" -ForegroundColor Yellow
if (-not (Test-Path `$TargetDir)) {
    New-Item -ItemType Directory -Path `$TargetDir -Force | Out-Null
}

# Fechar aplicacao se estiver aberta
`$Running = Get-Process -Name "YoutubeDlGui.App" -ErrorAction SilentlyContinue
if (`$Running) {
    Write-Host "      Fechando instancias ativas..." -ForegroundColor Gray
    `$Running | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Milliseconds 500
}

# Limpar scripts legados anteriores se existirem
Remove-Item -Path (Join-Path `$TargetDir "YtDlpGui-Launcher.*") -Force -ErrorAction SilentlyContinue

Write-Host "[2/4] Copiando arquivos mais recentes do servidor..." -ForegroundColor Yellow
robocopy "`$SourceDir" "`$TargetDir" YoutubeDlGui.App.exe yt-dlp.exe qjs.exe version.json /XO /FFT /R:2 /W:2 /NDL /NFL /NJH /NJS | Out-Null

Write-Host "[3/4] Desbloqueando arquivos e restricoes de rede..." -ForegroundColor Yellow
Get-ChildItem -Path `$TargetDir -Recurse -File | ForEach-Object {
    Unblock-File -Path `$_.FullName -ErrorAction SilentlyContinue
}

Write-Host "[4/4] Criando atalho na Area de Trabalho..." -ForegroundColor Yellow
`$DesktopPath = [Environment]::GetFolderPath([Environment+SpecialFolder]::Desktop)
`$ShortcutPath = Join-Path `$DesktopPath "yt-dlp GUI.lnk"
`$ExePath = Join-Path `$TargetDir "YoutubeDlGui.App.exe"

try {
    `$WshShell = New-Object -ComObject WScript.Shell
    `$Shortcut = `$WshShell.CreateShortcut(`$ShortcutPath)
    `$Shortcut.TargetPath = `$ExePath
    `$Shortcut.Arguments = ""
    `$Shortcut.WorkingDirectory = `$TargetDir
    `$Shortcut.Description = "yt-dlp GUI Modern (com auto-atualizacao em rede integrada)"
    `$Shortcut.IconLocation = "`$ExePath,0"
    `$Shortcut.Save()
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject(`$WshShell) | Out-Null
    Write-Host "      Atalho criado com sucesso no Desktop." -ForegroundColor Green
}
catch {
    Write-Warning "Falha ao criar o atalho no Desktop: `$_"
}

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " Instalacao concluida com sucesso!" -ForegroundColor Green
Write-Host " O aplicativo se mantem atualizado nativamente a cada execucao" -ForegroundColor White
Write-Host " mesmo quando fixado na barra de tarefas do Windows." -ForegroundColor White
Write-Host "==========================================================" -ForegroundColor Cyan
"@
[System.IO.File]::WriteAllText((Join-Path $StagingDir "Instalar.ps1"), $InstalarPs1Content, [System.Text.Encoding]::UTF8)

# -----------------------------------------------------------------------------
# 5. Publicação no Compartilhamento de Rede ($NetworkShare)
# -----------------------------------------------------------------------------
Write-Host "[5/6] Publicando pacote no compartilhamento de rede..." -ForegroundColor Yellow
Write-Host "      Destino: $NetworkShare" -ForegroundColor Gray

$PublishToNetworkSucceeded = $false

try {
    if (-not (Test-Path -Path $NetworkShare)) {
        Write-Host "      Criando diretório no servidor de rede..." -ForegroundColor Gray
        New-Item -ItemType Directory -Path $NetworkShare -Force | Out-Null
    }

    # Limpar launchers legados do servidor de rede
    Remove-Item -Path (Join-Path $NetworkShare "YtDlpGui-Launcher.*"), (Join-Path $NetworkShare "Atualizar.cmd") -Force -ErrorAction SilentlyContinue

    # Copiar todos os arquivos da área de staging para o servidor de rede
    robocopy "$StagingDir" "$NetworkShare" /E /XO /FFT /R:2 /W:2 /NDL /NFL /NJH /NJS | Out-Null
    $PublishToNetworkSucceeded = $true
    Write-Host "      Pacote publicado com sucesso no servidor de rede!" -ForegroundColor Green
}
catch {
    Write-Warning "Não foi possível acessar o compartilhamento de rede ($NetworkShare): $_"
    Write-Host "      Os arquivos compilados foram preservados temporariamente em: $StagingDir" -ForegroundColor Yellow
}

# -----------------------------------------------------------------------------
# 6. Atualização da Instalação Local (Máquina do Desenvolvedor)
# -----------------------------------------------------------------------------
if ($InstallLocally) {
    Write-Host "[6/6] Atualizando instalação local na estação atual..." -ForegroundColor Yellow
    Write-Host "      Destino Local: $ClientInstallDir" -ForegroundColor Gray

    try {
        if (-not (Test-Path -Path $ClientInstallDir)) {
            New-Item -ItemType Directory -Path $ClientInstallDir -Force | Out-Null
        }

        # Fechar instâncias ativas locais
        $RunningLocal = Get-Process -Name "YoutubeDlGui.App" -ErrorAction SilentlyContinue
        if ($RunningLocal) {
            $RunningLocal | Stop-Process -Force -ErrorAction SilentlyContinue
            Start-Sleep -Milliseconds 500
        }

        # Limpar launchers legados locais
        Remove-Item -Path (Join-Path $ClientInstallDir "YtDlpGui-Launcher.*"), (Join-Path $ClientInstallDir "Atualizar.cmd") -Force -ErrorAction SilentlyContinue

        # Copiar arquivos da staging para o diretório local
        robocopy "$StagingDir" "$ClientInstallDir" /E /XO /FFT /R:2 /W:2 /NDL /NFL /NJH /NJS | Out-Null

        # Desbloquear arquivos locais
        Get-ChildItem -Path $ClientInstallDir -Recurse -File | ForEach-Object {
            Unblock-File -Path $_.FullName -ErrorAction SilentlyContinue
        }

        # Criar / Atualizar atalho direto no Desktop
        $DesktopPath = [Environment]::GetFolderPath([Environment+SpecialFolder]::Desktop)
        $ShortcutPath = Join-Path $DesktopPath "yt-dlp GUI.lnk"
        $LocalExePath = Join-Path $ClientInstallDir "YoutubeDlGui.App.exe"

        $WshShell = New-Object -ComObject WScript.Shell
        $Shortcut = $WshShell.CreateShortcut($ShortcutPath)
        $Shortcut.TargetPath = $LocalExePath
        $Shortcut.Arguments = ""
        $Shortcut.WorkingDirectory = $ClientInstallDir
        $Shortcut.Description = "yt-dlp GUI Modern (com auto-atualizacao em rede integrada)"
        $Shortcut.IconLocation = "$LocalExePath,0"
        $Shortcut.Save()
        [System.Runtime.InteropServices.Marshal]::ReleaseComObject($WshShell) | Out-Null

        Write-Host "      Instalação e atalho local atualizados com sucesso!" -ForegroundColor Green
    }
    catch {
        Write-Warning "      Aviso ao atualizar instalação local: $_"
    }
}
else {
    Write-Host "[6/6] Atualização local ignorada (-InstallLocally:$false)." -ForegroundColor Gray
}

# Limpar staging se a publicação teve sucesso
if ($PublishToNetworkSucceeded) {
    Remove-Item -Path $StagingDir -Recurse -Force -ErrorAction SilentlyContinue
}

# -----------------------------------------------------------------------------
# Resumo Final
# -----------------------------------------------------------------------------
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " Publicação em Rede Concluída com Sucesso!" -ForegroundColor Green
Write-Host " Versão Publicada: v$NewVersion" -ForegroundColor White
Write-Host " Repositório de Rede: $NetworkShare" -ForegroundColor Green
Write-Host " Extensões de Navegador: $NetworkShare\Extensoes-Navegadores" -ForegroundColor White
Write-Host ""
Write-Host " Como instalar nas outras estações de trabalho da rede:" -ForegroundColor Cyan
Write-Host " Execute na estação:" -ForegroundColor White
Write-Host " powershell -ExecutionPolicy Bypass -File `"$NetworkShare\Instalar.ps1`"" -ForegroundColor Yellow
Write-Host "==========================================================" -ForegroundColor Cyan
