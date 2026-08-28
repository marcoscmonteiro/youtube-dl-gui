<#
.SYNOPSIS
    Script de compilação e empacotamento do yt-dlp-gui para Windows.
#>

param(
    [string]$TargetDir = "$PSScriptRoot\..\dist\windows"
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "         yt-dlp-gui - Empacotamento para Windows          " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$ResolvedTarget = [System.IO.Path]::GetFullPath($TargetDir)
if (-not (Test-Path -Path $ResolvedTarget)) {
    New-Item -ItemType Directory -Path $ResolvedTarget -Force | Out-Null
}

$ProjectRoot = [System.IO.Path]::GetFullPath("$PSScriptRoot\..")
$IconPath = Join-Path $ProjectRoot "frontend\assets\VideoDownload.ico"
if (-not (Test-Path -Path $IconPath)) {
    $IconPath = Join-Path $PSScriptRoot "..\..\YoutubeDlGui.App\VideoDownload.ico"
}

Write-Host "[1/2] Compilando executável standalone com PyInstaller..." -ForegroundColor Yellow

$PyInstallerArgs = @(
    "--noconfirm",
    "--onedir",
    "--windowed",
    "--name", "yt-dlp-gui",
    "--paths", $ProjectRoot,
    "--distpath", $ResolvedTarget,
    "--workpath", "$ResolvedTarget\build",
    "--specpath", "$ResolvedTarget"
)

if (Test-Path -Path $IconPath) {
    $PyInstallerArgs += @("--icon", $IconPath)
}

# Add stylesheets and assets
$PyInstallerArgs += @(
    "--add-data", "$ProjectRoot\frontend\styles;frontend\styles",
    "$ProjectRoot\frontend\main.py"
)

try {
    & python -m PyInstaller $PyInstallerArgs
    Write-Host "[2/2] Sucesso! Binários gerados em: $ResolvedTarget\yt-dlp-gui" -ForegroundColor Green
}
catch {
    Write-Warning "Falha ao executar PyInstaller. Verifique se o PyInstaller está instalado via: pip install pyinstaller"
}

