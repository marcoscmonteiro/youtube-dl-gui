#!/usr/bin/env bash
set -e

echo "=========================================================="
echo "          yt-dlp-gui - Empacotamento para Linux           "
echo "=========================================================="

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
DIST_DIR="$PROJECT_ROOT/dist/linux"

mkdir -p "$DIST_DIR"

echo "[1/2] Gerando executável standalone para Linux..."

python3 -m PyInstaller \
    --noconfirm \
    --onedir \
    --windowed \
    --name "yt-dlp-gui" \
    --paths "$PROJECT_ROOT" \
    --distpath "$DIST_DIR" \
    --workpath "$DIST_DIR/build" \
    --specpath "$DIST_DIR" \
    --add-data "$PROJECT_ROOT/frontend/styles:frontend/styles" \
    "$PROJECT_ROOT/frontend/main.py"

echo "[2/2] Binários gerados com sucesso em: $DIST_DIR/yt-dlp-gui"

