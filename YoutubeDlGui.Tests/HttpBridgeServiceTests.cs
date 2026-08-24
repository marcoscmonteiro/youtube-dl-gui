using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using YoutubeDlGui.Core.Models;
using YoutubeDlGui.Services;
using Xunit;

namespace YoutubeDlGui.Tests;

public class HttpBridgeServiceTests
{
    [Fact]
    public async Task HttpBridgeService_Ping_ReturnsOk()
    {
        int testPort = 48195;
        using var bridge = new HttpBridgeService();
        await bridge.StartAsync(testPort);

        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"http://127.0.0.1:{testPort}/api/ping");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("YoutubeDlGui", content);
            Assert.Contains("ok", content);
        }
        finally
        {
            await bridge.StopAsync();
        }
    }

    [Fact]
    public async Task HttpBridgeService_DownloadEndpoint_FiresEvent_AndReturnsSuccess()
    {
        int testPort = 48196;
        using var bridge = new HttpBridgeService();
        await bridge.StartAsync(testPort);

        ExternalDownloadRequest? receivedReq = null;
        bridge.DownloadRequested += (s, req) =>
        {
            receivedReq = req;
        };

        try
        {
            using var httpClient = new HttpClient();
            var payload = new
            {
                url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                quality = "FHD_1080p",
                audioOnly = false,
                playlist = false
            };

            var response = await httpClient.PostAsJsonAsync($"http://127.0.0.1:{testPort}/api/download", payload);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Give a few ms for event to fire
            await Task.Delay(100);

            Assert.NotNull(receivedReq);
            Assert.Equal("https://www.youtube.com/watch?v=dQw4w9WgXcQ", receivedReq!.Url);
            Assert.Equal("FHD_1080p", receivedReq.Quality);
        }
        finally
        {
            await bridge.StopAsync();
        }
    }

    [Fact]
    public async Task HttpBridgeService_InvalidUrl_ReturnsBadRequest()
    {
        int testPort = 48197;
        using var bridge = new HttpBridgeService();
        await bridge.StartAsync(testPort);

        try
        {
            using var httpClient = new HttpClient();
            var payload = new
            {
                url = "invalid-not-http-url"
            };

            var response = await httpClient.PostAsJsonAsync($"http://127.0.0.1:{testPort}/api/download", payload);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        finally
        {
            await bridge.StopAsync();
        }
    }

    [Fact]
    public async Task HttpBridgeService_DownloadWithDirectory_PassesDirectoryCorrectly()
    {
        int testPort = 48198;
        using var bridge = new HttpBridgeService();
        await bridge.StartAsync(testPort);

        ExternalDownloadRequest? receivedReq = null;
        bridge.DownloadRequested += (s, req) =>
        {
            receivedReq = req;
        };

        try
        {
            using var httpClient = new HttpClient();
            var payload = new
            {
                url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                quality = "Best",
                downloadDirectory = @"C:\Users\test\Downloads"
            };

            var response = await httpClient.PostAsJsonAsync($"http://127.0.0.1:{testPort}/api/download", payload);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await Task.Delay(100);

            Assert.NotNull(receivedReq);
            Assert.Equal(@"C:\Users\test\Downloads", receivedReq!.DownloadDirectory);
        }
        finally
        {
            await bridge.StopAsync();
        }
    }

    [Fact]
    public async Task HttpBridgeService_DownloadWithCookies_PassesCookiesTextCorrectly()
    {
        int testPort = 48199;
        using var bridge = new HttpBridgeService();
        await bridge.StartAsync(testPort);

        ExternalDownloadRequest? receivedReq = null;
        bridge.DownloadRequested += (s, req) =>
        {
            receivedReq = req;
        };

        try
        {
            using var httpClient = new HttpClient();
            string mockNetscapeCookies = "# Netscape HTTP Cookie File\n.youtube.com\tTRUE\t/\tTRUE\t1798765432\tSID\tsample_session_cookie_value";

            var payload = new
            {
                url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                quality = "Best",
                cookiesText = mockNetscapeCookies
            };

            var response = await httpClient.PostAsJsonAsync($"http://127.0.0.1:{testPort}/api/download", payload);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await Task.Delay(100);

            Assert.NotNull(receivedReq);
            Assert.Equal(mockNetscapeCookies, receivedReq!.CookiesText);
        }
        finally
        {
            await bridge.StopAsync();
        }
    }

    [Fact]
    public void CookieFile_WrittenWithoutBom_IsValidForPython()
    {
        string sampleCookies = "# Netscape HTTP Cookie File\n# http://curl.haxx.se/rfc/cookie_spec.html\n.youtube.com\tTRUE\t/\tTRUE\t1798765432\tSID\tsample_cookie_value\n";
        string tempPath = Path.Combine(Path.GetTempPath(), $"ydl_cookie_test_{Guid.NewGuid():N}.txt");

        try
        {
            var utf8WithoutBom = new UTF8Encoding(false);
            File.WriteAllText(tempPath, sampleCookies, utf8WithoutBom);

            byte[] bytes = File.ReadAllBytes(tempPath);
            Assert.True(bytes.Length >= 3);
            // Ensure first bytes are '#' (0x23), NOT UTF-8 BOM (0xEF, 0xBB, 0xBF)
            Assert.Equal((byte)'#', bytes[0]);
            Assert.Equal((byte)' ', bytes[1]);
            Assert.Equal((byte)'N', bytes[2]);
            Assert.NotEqual(0xEF, bytes[0]);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Fact]
    public async Task HttpBridgeService_DownloadWithPlayerClients_PassesPlayerClientsCorrectly()
    {
        int testPort = 48197;
        using var bridge = new HttpBridgeService();
        await bridge.StartAsync(testPort);

        ExternalDownloadRequest? receivedReq = null;
        bridge.DownloadRequested += (s, req) =>
        {
            receivedReq = req;
        };

        try
        {
            using var httpClient = new HttpClient();
            var payload = new
            {
                url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                quality = "Best",
                playerClients = "android,web,ios"
            };

            var response = await httpClient.PostAsJsonAsync($"http://127.0.0.1:{testPort}/api/download", payload);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await Task.Delay(100);

            Assert.NotNull(receivedReq);
            Assert.Equal("android,web,ios", receivedReq!.PlayerClients);
        }
        finally
        {
            await bridge.StopAsync();
        }
    }

    [Fact]
    public async Task HttpBridgeService_DownloadWithExtraOptions_PassesExtraOptionsCorrectly()
    {
        int testPort = 48196;
        using var bridge = new HttpBridgeService();
        await bridge.StartAsync(testPort);

        ExternalDownloadRequest? receivedReq = null;
        bridge.DownloadRequested += (s, req) =>
        {
            receivedReq = req;
        };

        try
        {
            using var httpClient = new HttpClient();
            var payload = new
            {
                url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                quality = "Best",
                extraOptions = "--limit-rate 5M --embed-subs"
            };

            var response = await httpClient.PostAsJsonAsync($"http://127.0.0.1:{testPort}/api/download", payload);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await Task.Delay(100);

            Assert.NotNull(receivedReq);
            Assert.Equal("--limit-rate 5M --embed-subs", receivedReq!.ExtraOptions);
        }
        finally
        {
            await bridge.StopAsync();
        }
    }
}
