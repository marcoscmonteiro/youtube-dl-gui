import os

STYLES_DIR = os.path.dirname(os.path.abspath(__file__))


def load_theme(theme_name: str = "Dark") -> str:
    filename = "theme_dark.qss" if theme_name.lower() != "light" else "theme_light.qss"
    filepath = os.path.join(STYLES_DIR, filename)
    if os.path.exists(filepath):
        with open(filepath, "r", encoding="utf-8") as f:
            return f.read()
    return ""

