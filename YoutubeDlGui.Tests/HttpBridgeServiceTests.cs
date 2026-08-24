using System.Net;
using System.Net.Http.Json;
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
}
