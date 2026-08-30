<#
.SYNOPSIS
    Script de publicação, assinatura e criação de atalho para o yt-dlp GUI Modern.

.DESCRIPTION
    Incrementa a versão do projeto, compila a aplicação em modo Release (ou outro especificado),
    publica os binários no diretório de destino informado (ou no padrão local de programas),
    remove streams Zone.Identifier (Mark of the Web), assina os binários com certificado
    Authenticode local e cria/atualiza o atalho na Área de Trabalho (Desktop).

.PARAMETER TargetDirectory
    Diretório de destino para a publicação.
    Se não especificado, detecta dinamicamente a pasta do OneDrive:
    1º: $env:OneDriveConsumer\Aplicativos\YtDlpGui
    2º: $env:USERPROFILE\OneDrive\Aplicativos\YtDlpGui
    3º: $env:OneDrive\Aplicativos\YtDlpGui
    Fallback (desconectado): "$env:LOCALAPPDATA\Programs\YtDlpGui"

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
    Gera a publicação tradicional multi-arquivo (separando DLLs) ao invés do padrão Single-File.

.PARAMETER SelfContained
    Gera uma publicação totalmente autocontida (Single-File com runtime do .NET embutido).

.EXAMPLE
    .\publish.ps1
    Publica no OneDrive (ou fallback local) em Single-File com incremento de versão patch.

.EXAMPLE
    .\publish.ps1 -SelfContained
    Gera um executável único 100% autocontido contendo o runtime .NET embutido.

.EXAMPLE
    .\publish.ps1 -IncrementType Minor
    Incrementa a versão menor (ex: 2.0.5 -> 2.1.0) e publica.

.EXAMPLE
    .\publish.ps1 -CustomVersion "2.5.0"
    Define a versão explicitamente para 2.5.0 e publica.

.EXAMPLE
    .\publish.ps1 -TargetDirectory "D:\Ferramentas\YtDlpGui" -SkipVersionIncrement
    Publica no diretório especificado mantendo a versão atual.
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0, Mandatory = $false)]
    [string]$TargetDirectory = "",

    [Parameter(Position = 1, Mandatory = $false)]
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
    [switch]$SelfContained = $false
)

$ErrorActionPreference = "Stop"

$IsSingleFile = -not $NoSingleFile

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "       yt-dlp GUI Modern - Publicação e Instalação        " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Resolver e criar caminho de destino
Write-Host "[1/7] Resolvendo diretório de destino..." -ForegroundColor Yellow

$ResolvedTarget = $null
$IsOneDriveTarget = $false

if (-not [string]::IsNullOrWhiteSpace($TargetDirectory)) {
    $ResolvedTarget = [System.IO.Path]::GetFullPath($TargetDirectory)
    Write-Host "      Diretório informado manualmente: $ResolvedTarget" -ForegroundColor Gray
}
else {
    # Ordem de preferência para OneDrive:
    # 1º: $env:OneDriveConsumer
    # 2º: $env:USERPROFILE\OneDrive
    # 3º: $env:OneDrive
    $OneDriveBase = $null

    if ($env:OneDriveConsumer -and (Test-Path -Path $env:OneDriveConsumer)) {
        $OneDriveBase = $env:OneDriveConsumer
    }
    elseif (Test-Path -Path "$env:USERPROFILE\OneDrive") {
        $OneDriveBase = "$env:USERPROFILE\OneDrive"
    }
    elseif ($env:OneDrive -and (Test-Path -Path $env:OneDrive)) {
        $OneDriveBase = $env:OneDrive
    }

    if ($OneDriveBase) {
        $OneDriveTarget = Join-Path $OneDriveBase "Aplicativos\YtDlpGui"
        try {
            if (-not (Test-Path -Path $OneDriveTarget)) {
                New-Item -ItemType Directory -Path $OneDriveTarget -Force | Out-Null
            }
            $ResolvedTarget = [System.IO.Path]::GetFullPath($OneDriveTarget)
            $IsOneDriveTarget = $true
            Write-Host "      OneDrive conectado: $ResolvedTarget" -ForegroundColor Green
        }
        catch {
            Write-Warning "      Falha ao acessar diretório do OneDrive ($OneDriveTarget): $_"
        }
    }

    if (-not $ResolvedTarget) {
        $FallbackTarget = "$env:LOCALAPPDATA\Programs\YtDlpGui"
        $ResolvedTarget = [System.IO.Path]::GetFullPath($FallbackTarget)
        Write-Host "      OneDrive não disponível. Usando diretório local padrão: $ResolvedTarget" -ForegroundColor Yellow
    }
}

if (-not (Test-Path -Path $ResolvedTarget)) {
    Write-Host "      Criando diretório de destino: $ResolvedTarget..." -ForegroundColor Gray
    New-Item -ItemType Directory -Path $ResolvedTarget -Force | Out-Null
}

# 2. Caminho do projeto
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectFile = Join-Path $ScriptDir "YoutubeDlGui.App\YoutubeDlGui.App.csproj"

if (-not (Test-Path -Path $ProjectFile)) {
    Write-Error "Arquivo de projeto não encontrado em: $ProjectFile"
    exit 1
}

# 3. Gerenciamento e Incremento de Versão
Write-Host "[2/7] Gerenciando versão da aplicação..." -ForegroundColor Yellow

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

# Atualizar o arquivo .csproj se a versão mudou ou a tag não existia
if ($NewVersion -ne $CurrentVersion -or -not $HasVersionTag) {
    if ($HasVersionTag) {
        $UpdatedCsproj = [regex]::Replace($CsprojContent, '<Version>(.*?)</Version>', "<Version>$NewVersion</Version>")
    } else {
        $UpdatedCsproj = [regex]::Replace($CsprojContent, '(?s)(<PropertyGroup\b[^>]*>)', "`$1`r`n    <Version>$NewVersion</Version>")
    }
    [System.IO.File]::WriteAllText($ProjectFile, $UpdatedCsproj, [System.Text.Encoding]::UTF8)
    Write-Host "      Arquivo do projeto atualizado com a versão $NewVersion." -ForegroundColor Gray
}

# 4. Fechar instâncias ativas se existirem para evitar travamento de DLLs
$RunningProcesses = Get-Process -Name "YoutubeDlGui.App" -ErrorAction SilentlyContinue
if ($RunningProcesses) {
    Write-Host "      Fechando instâncias ativas do YoutubeDlGui.App para atualização dos binários..." -ForegroundColor Gray
    $RunningProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Milliseconds 500
}

# Limpar arquivos residuais (DLLs e PDBs) de publicações anteriores
Get-ChildItem -Path $ResolvedTarget -Recurse -Include "*.pdb" -File | Remove-Item -Force -ErrorAction SilentlyContinue
if ($IsSingleFile -and $SelfContained) {
    Get-ChildItem -Path $ResolvedTarget -Recurse -Include "YoutubeDlGui.*.dll", "CommunityToolkit*.dll", "Microsoft.Extensions*.dll", "*.deps.json", "*.runtimeconfig.json" -File | Remove-Item -Force -ErrorAction SilentlyContinue
}

# 5. Executar dotnet publish
$PublishModeDesc = if ($IsSingleFile) { if ($SelfContained) { "Single-File Self-Contained" } else { "Single-File Framework-Dependent" } } else { "Multi-File" }
Write-Host "[3/7] Compilando e publicando a aplicação v$NewVersion ($Configuration, $PublishModeDesc, sem símbolos de depuração)..." -ForegroundColor Yellow

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
        "-o", "$ResolvedTarget",
        "--nologo"
    )
    & dotnet @PublishArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Falha durante o dotnet publish (Código de saída: $LASTEXITCODE)."
        exit $LASTEXITCODE
    }

    # Garantir que nenhum arquivo .pdb remanescente permaneça no diretório publicado
    Get-ChildItem -Path $ResolvedTarget -Recurse -Include "*.pdb" -File | Remove-Item -Force -ErrorAction SilentlyContinue
}
catch {
    Write-Error "Erro ao executar dotnet publish: $_"
    exit 1
}

# 6. Copiar yt-dlp.exe e qjs.exe caso já tenham sido baixados em ambiente de desenvolvimento
$DebugYtDlp = Join-Path $ScriptDir "YoutubeDlGui.App\bin\Debug\net8.0-windows\yt-dlp.exe"
$TargetYtDlp = Join-Path $ResolvedTarget "yt-dlp.exe"

if ((Test-Path -Path $DebugYtDlp) -and (-not (Test-Path -Path $TargetYtDlp))) {
    Write-Host "      Copiando yt-dlp.exe existente para a pasta de publicação..." -ForegroundColor Gray
    Copy-Item -Path $DebugYtDlp -Destination $TargetYtDlp -Force
}

$DebugQjs = Join-Path $ScriptDir "YoutubeDlGui.App\bin\Debug\net8.0-windows\qjs.exe"
$TargetQjs = Join-Path $ResolvedTarget "qjs.exe"

if ((Test-Path -Path $DebugQjs) -and (-not (Test-Path -Path $TargetQjs))) {
    Write-Host "      Copiando qjs.exe existente para a pasta de publicação..." -ForegroundColor Gray
    Copy-Item -Path $DebugQjs -Destination $TargetQjs -Force
}

# 7. Desbloquear arquivos (Remover Mark of the Web / Zone.Identifier)
Write-Host "[4/7] Desbloqueando arquivos e removendo restrições de rede (Zone.Identifier)..." -ForegroundColor Yellow
try {
    Get-ChildItem -Path $ResolvedTarget -Recurse -File | ForEach-Object {
        Unblock-File -Path $_.FullName -ErrorAction SilentlyContinue
    }
    Write-Host "      Arquivos desbloqueados com sucesso." -ForegroundColor Green
}
catch {
    Write-Warning "      Não foi possível desbloquear alguns arquivos: $_"
}

# 8. Garantir disponibilidade local no OneDrive (Files On-Demand)
if ($IsOneDriveTarget -or ($ResolvedTarget -match 'OneDrive')) {
    try {
        attrib -u +p /s /d "$ResolvedTarget\*" 2>$null
        Write-Host "      OneDrive: Diretório fixado localmente (Sempre manter neste dispositivo)." -ForegroundColor Gray
    }
    catch { }
}

# 8. Assinatura Digital Local (Mitigação do Smart App Control)
if (-not $SkipCodeSigning) {
    Write-Host "[5/7] Verificando assinatura digital local (Smart App Control)..." -ForegroundColor Yellow
    try {
        $CertSubject = "CN=YtDlpGui Local Development"
        $Cert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert -ErrorAction SilentlyContinue |
            Where-Object { $_.Subject -eq $CertSubject } |
            Select-Object -First 1

        if (-not $Cert) {
            Write-Host "      Criando certificado local de assinatura de código..." -ForegroundColor Gray
            $Cert = New-SelfSignedCertificate `
                -Type CodeSigningCert `
                -Subject $CertSubject `
                -CertStoreLocation "Cert:\CurrentUser\My" `
                -NotAfter (Get-Date).AddYears(5)

            $TempCertPath = Join-Path $env:TEMP "YtDlpGuiLocalDev.cer"
            Export-Certificate -Cert $Cert -FilePath $TempCertPath | Out-Null
            Import-Certificate -CertStoreLocation "Cert:\CurrentUser\TrustedPublisher" -FilePath $TempCertPath | Out-Null
            Import-Certificate -CertStoreLocation "Cert:\CurrentUser\Root" -FilePath $TempCertPath | Out-Null
            try {
                Import-Certificate -CertStoreLocation "Cert:\LocalMachine\TrustedPublisher" -FilePath $TempCertPath -ErrorAction SilentlyContinue | Out-Null
                Import-Certificate -CertStoreLocation "Cert:\LocalMachine\Root" -FilePath $TempCertPath -ErrorAction SilentlyContinue | Out-Null
            } catch { }
            Remove-Item -Path $TempCertPath -Force -ErrorAction SilentlyContinue
            Write-Host "      Certificado criado e registrado nas Autoridades Raiz e Editores Confiáveis." -ForegroundColor Green
        }

        if ($Cert) {
            $BinariesToSign = Get-ChildItem -Path $ResolvedTarget -Include *.exe, *.dll -Recurse -File
            $SignedCount = 0
            foreach ($file in $BinariesToSign) {
                Set-AuthenticodeSignature -FilePath $file.FullName -Certificate $Cert -HashAlgorithm SHA256 | Out-Null
                $SignedCount++
            }
            Write-Host "      $SignedCount binários assinados digitalmente com sucesso (SHA-256)." -ForegroundColor Green
        }
    }
    catch {
        Write-Warning "      Não foi possível aplicar a assinatura de código automática: $_"
    }
}
else {
    Write-Host "[5/7] Assinatura de código ignorada (-SkipCodeSigning)." -ForegroundColor Gray
}

# 9. Criar atalho na Área de Trabalho (Desktop)
Write-Host "[6/7] Criando atalho na Área de Trabalho (Desktop)..." -ForegroundColor Yellow

$ExePath = Join-Path $ResolvedTarget "YoutubeDlGui.App.exe"
$DesktopPath = [Environment]::GetFolderPath([Environment+SpecialFolder]::Desktop)
$ShortcutPath = Join-Path $DesktopPath "yt-dlp GUI.lnk"

try {
    $WshShell = New-Object -ComObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut($ShortcutPath)
    $Shortcut.TargetPath = $ExePath
    $Shortcut.WorkingDirectory = $ResolvedTarget
    $Shortcut.Description = "yt-dlp GUI Modern - Downloader com suporte a tema escuro"
    $Shortcut.IconLocation = "$ExePath,0"
    $Shortcut.Save()
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($WshShell) | Out-Null

    Write-Host "      Atalho criado com sucesso: $ShortcutPath" -ForegroundColor Green
}
catch {
    Write-Warning "Não foi possível criar o atalho automaticamente no Desktop: $_"
}

# 10. Finalização
Write-Host "[7/7] Concluído!" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " Aplicação publicada com sucesso em:" -ForegroundColor White
Write-Host " $ResolvedTarget" -ForegroundColor Green
Write-Host " Versão da aplicação: v$NewVersion" -ForegroundColor White
Write-Host " Executável principal: $ExePath" -ForegroundColor White
Write-Host "==========================================================" -ForegroundColor Cyan

