# Script de Empacotamento da Extensão YoutubeDL-GUI
# Gera pacotes prontos para Google Chrome / Chromium e Mozilla Firefox

$extRoot = $PSScriptRoot
$distDir = Join-Path $extRoot "dist"

if (Test-Path $distDir) {
    Remove-Item -Path $distDir -Recurse -Force
}
New-Item -ItemType Directory -Path $distDir -Force | Out-Null

$filesToInclude = @(
    "manifest.json",
    "background.js",
    "cookieHelper.js",
    "proxyHelper.js",
    "icons",
    "popup",
    "options"
)

# 1. Empacotar para Chrome / Chromium (Edge, Brave, Opera)
$chromeZip = Join-Path $distDir "youtube-dl-gui-chrome.zip"
Write-Host "Criando pacote Chrome/Edge: $chromeZip" -ForegroundColor Cyan

$tempChrome = Join-Path $distDir "temp_chrome"
New-Item -ItemType Directory -Path $tempChrome -Force | Out-Null

foreach ($item in $filesToInclude) {
    $src = Join-Path $extRoot $item
    Copy-Item -Path $src -Destination $tempChrome -Recurse -Force
}

Compress-Archive -Path "$tempChrome\*" -DestinationPath $chromeZip -Force
Remove-Item -Path $tempChrome -Recurse -Force

# 2. Empacotar para Mozilla Firefox (.zip e .xpi)
$firefoxZip = Join-Path $distDir "youtube-dl-gui-firefox.zip"
$firefoxXpi = Join-Path $distDir "youtube-dl-gui-firefox.xpi"
Write-Host "Criando pacote Firefox: $firefoxZip" -ForegroundColor Cyan

$tempFirefox = Join-Path $distDir "temp_firefox"
New-Item -ItemType Directory -Path $tempFirefox -Force | Out-Null

foreach ($item in $filesToInclude) {
    $src = Join-Path $extRoot $item
    Copy-Item -Path $src -Destination $tempFirefox -Recurse -Force
}

Compress-Archive -Path "$tempFirefox\*" -DestinationPath $firefoxZip -Force
Copy-Item -Path $firefoxZip -Destination $firefoxXpi -Force
Remove-Item -Path $tempFirefox -Recurse -Force

Write-Host "`nPacotes gerados com sucesso na pasta dist/:" -ForegroundColor Green
Get-ChildItem -Path $distDir | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize
