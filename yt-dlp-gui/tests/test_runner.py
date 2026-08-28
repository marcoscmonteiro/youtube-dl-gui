import asyncio
import os
import sys
import unittest

# Ensure yt-dlp-gui root is in sys.path
current_dir = os.path.dirname(os.path.abspath(__file__))
ytdlp_gui_dir = os.path.abspath(os.path.join(current_dir, ".."))
workspace_dir = os.path.abspath(os.path.join(current_dir, "..", ".."))

for p in [ytdlp_gui_dir, workspace_dir]:
    if p not in sys.path:
        sys.path.insert(0, p)

from tests.test_domain_models import (
    test_app_settings_default_values,
    test_download_item_path_computation,
    test_download_item_serialization_roundtrip,
)
from tests.test_settings_service import (
    test_json_settings_save_and_load,
    test_json_settings_history_save_and_load,
)
from tests.test_ytdlp_wrapper import (
    test_format_helpers,
    test_build_format_string,
    test_cancellation_flags,
    test_build_ydl_options_js_runtimes_validity,
)
from tests.test_queue_manager import (
    test_queue_manager_enqueue_item,
)
from tests.test_api_server import (
    test_api_server_endpoints,
)


class TestYtDlpGui(unittest.TestCase):
    def test_01_domain_models(self):
        test_app_settings_default_values()
        test_download_item_path_computation()
        test_download_item_serialization_roundtrip()

    def test_02_ytdlp_wrapper(self):
        test_format_helpers()
        test_build_format_string()
        test_cancellation_flags()
        test_build_ydl_options_js_runtimes_validity()

    def test_03_settings_service(self):
        asyncio.run(test_json_settings_save_and_load())
        asyncio.run(test_json_settings_history_save_and_load())

    def test_04_queue_manager(self):
        asyncio.run(test_queue_manager_enqueue_item())

    def test_05_api_server(self):
        asyncio.run(test_api_server_endpoints())


if __name__ == "__main__":
    unittest.main()
