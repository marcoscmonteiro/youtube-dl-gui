<#
.SYNOPSIS
    Script de publicação, assinatura e criação de atalho para o yt-dlp GUI Modern.

.DESCRIPTION
    Compila a aplicação em modo Release (ou outro especificado), publica os binários
    no diretório de destino informado (ou no padrão local de programas), remove streams
    Zone.Identifier (Mark of the Web), assina os binários com certificado Authenticode
    local e cria/atualiza o atalho na Área de Trabalho (Desktop).

.PARAMETER TargetDirectory
    Diretório de destino para a publicação.
    Padrão: "$env:LOCALAPPDATA\Programs\YtDlpGui"

.PARAMETER Configuration
    Configuração de compilação (.NET).
    Padrão: "Release"

.PARAMETER SkipCodeSigning
    Ignora a etapa de assinatura de código local.

.EXAMPLE
    .\publish.ps1
    Publica no diretório padrão ($env:LOCALAPPDATA\Programs\YtDlpGui) e cria o atalho no Desktop.

.EXAMPLE
    .\publish.ps1 -TargetDirectory "D:\Ferramentas\YtDlpGui"
    Publica no diretório especificado.
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0, Mandatory = $false)]
    [string]$TargetDirectory = "$env:LOCALAPPDATA\Programs\YtDlpGui",

    [Parameter(Position = 1, Mandatory = $false)]
    [string]$Configuration = "Release",

    [Parameter(Mandatory = $false)]
    [switch]$SkipCodeSigning = $false
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "       yt-dlp GUI Modern - Publicação e Instalação        " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Resolver e criar caminho de destino
$ResolvedTarget = [System.IO.Path]::GetFullPath($TargetDirectory)
Write-Host "[1/6] Diretório de Destino: $ResolvedTarget" -ForegroundColor Yellow

if (-not (Test-Path -Path $ResolvedTarget)) {
    Write-Host "      Criando diretório de destino..." -ForegroundColor Gray
    New-Item -ItemType Directory -Path $ResolvedTarget -Force | Out-Null
}

# 2. Caminho do projeto
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectFile = Join-Path $ScriptDir "YoutubeDlGui.App\YoutubeDlGui.App.csproj"

if (-not (Test-Path -Path $ProjectFile)) {
    Write-Error "Arquivo de projeto não encontrado em: $ProjectFile"
    exit 1
}

# 3. Fechar instâncias ativas se existirem para evitar travamento de DLLs
$RunningProcesses = Get-Process -Name "YoutubeDlGui.App" -ErrorAction SilentlyContinue
if ($RunningProcesses) {
    Write-Host "      Fechando instâncias ativas do YoutubeDlGui.App para atualização dos binários..." -ForegroundColor Gray
    $RunningProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Milliseconds 500
}

# 4. Executar dotnet publish
Write-Host "[2/6] Compilando e publicando a aplicação ($Configuration)..." -ForegroundColor Yellow

try {
    & dotnet publish "$ProjectFile" -c $Configuration -o "$ResolvedTarget" --nologo
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Falha durante o dotnet publish (Código de saída: $LASTEXITCODE)."
        exit $LASTEXITCODE
    }
}
catch {
    Write-Error "Erro ao executar dotnet publish: $_"
    exit 1
}

# 5. Copiar yt-dlp.exe e qjs.exe caso já tenham sido baixados em ambiente de desenvolvimento
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

# 6. Desbloquear arquivos (Remover Mark of the Web / Zone.Identifier)
Write-Host "[3/6] Desbloqueando arquivos e removendo restrições de rede (Zone.Identifier)..." -ForegroundColor Yellow
try {
    Get-ChildItem -Path $ResolvedTarget -Recurse -File | ForEach-Object {
        Unblock-File -Path $_.FullName -ErrorAction SilentlyContinue
    }
    Write-Host "      Arquivos desbloqueados com sucesso." -ForegroundColor Green
}
catch {
    Write-Warning "      Não foi possível desbloquear alguns arquivos: $_"
}

# 7. Assinatura Digital Local (Mitigação do Smart App Control)
if (-not $SkipCodeSigning) {
    Write-Host "[4/6] Verificando assinatura digital local (Smart App Control)..." -ForegroundColor Yellow
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
            Remove-Item -Path $TempCertPath -Force -ErrorAction SilentlyContinue
            Write-Host "      Certificado criado e adicionado a Editores Confiáveis (TrustedPublisher)." -ForegroundColor Green
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
    Write-Host "[4/6] Assinatura de código ignorada (-SkipCodeSigning)." -ForegroundColor Gray
}

# 8. Criar atalho na Área de Trabalho (Desktop)
Write-Host "[5/6] Criando atalho na Área de Trabalho (Desktop)..." -ForegroundColor Yellow

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

# 9. Finalização
Write-Host "[6/6] Concluído!" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " Aplicação publicada com sucesso em:" -ForegroundColor White
Write-Host " $ResolvedTarget" -ForegroundColor Green
Write-Host " Executável principal: $ExePath" -ForegroundColor White
Write-Host "==========================================================" -ForegroundColor Cyan
