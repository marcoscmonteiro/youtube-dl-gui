<#
.SYNOPSIS
    Script de publicação e criação de atalho para o yt-dlp GUI Modern.

.DESCRIPTION
    Compila a aplicação em modo Release (ou outro especificado), publica os binários
    no diretório de destino informado (ou no padrão) e cria um atalho na Área de Trabalho (Desktop).

.PARAMETER TargetDirectory
    Diretório de destino para a publicação.
    Padrão: "$env:USERPROFILE\OneDrive\Aplicativos\YtDlpGui"

.PARAMETER Configuration
    Configuração de compilação (.NET).
    Padrão: "Release"

.EXAMPLE
    .\publish.ps1
    Publica no diretório padrão ($env:USERPROFILE\OneDrive\Aplicativos\YtDlpGui) e cria o atalho no Desktop.

.EXAMPLE
    .\publish.ps1 -TargetDirectory "D:\Ferramentas\YtDlpGui"
    Publica no diretório especificado.
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0, Mandatory = $false)]
    [string]$TargetDirectory = "$env:USERPROFILE\OneDrive\Aplicativos\YtDlpGui",

    [Parameter(Position = 1, Mandatory = $false)]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "       yt-dlp GUI Modern - Publicação e Instalação        " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Resolver e criar caminho de destino
$ResolvedTarget = [System.IO.Path]::GetFullPath($TargetDirectory)
Write-Host "[1/4] Diretório de Destino: $ResolvedTarget" -ForegroundColor Yellow

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

# 3. Executar dotnet publish
Write-Host "[2/4] Compilando e publicando a aplicação ($Configuration)..." -ForegroundColor Yellow

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

# 4. Copiar yt-dlp.exe e qjs.exe caso já tenham sido baixados em ambiente de desenvolvimento
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

# 5. Criar atalho na Área de Trabalho (Desktop)
Write-Host "[3/4] Criando atalho na Área de Trabalho (Desktop)..." -ForegroundColor Yellow

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

# 6. Finalização
Write-Host "[4/4] Concluído!" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " Aplicação publicada com sucesso em:" -ForegroundColor White
Write-Host " $ResolvedTarget" -ForegroundColor Green
Write-Host " Executável principal: $ExePath" -ForegroundColor White
Write-Host "==========================================================" -ForegroundColor Cyan
