using System.Net;
using System.Text;
using System.Text.Json;
using YoutubeDlGui.Core.Interfaces;
using YoutubeDlGui.Core.Models;

namespace YoutubeDlGui.Services;

public class HttpBridgeService : IHttpBridgeService
{
    private HttpListener? _listener;
    private CancellationTokenSource? _cts;
    private Task? _listenerTask;
    private readonly Func<object>? _statusProvider;

    public bool IsRunning => _listener?.IsListening ?? false;
    public int Port { get; private set; } = 48190;

    public event EventHandler<ExternalDownloadRequest>? DownloadRequested;

    public HttpBridgeService(Func<object>? statusProvider = null)
    {
        _statusProvider = statusProvider;
    }

    public Task StartAsync(int port = 48190)
    {
        if (IsRunning)
        {
            return Task.CompletedTask;
        }

        Port = port;
        _cts = new CancellationTokenSource();

        try
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://127.0.0.1:{Port}/");
            _listener.Start();

            _listenerTask = Task.Run(() => ListenLoopAsync(_cts.Token));
        }
        catch (Exception ex)
        {
            _listener?.Close();
            _listener = null;
            // Allow caller to handle or log exception
            throw new InvalidOperationException($"Não foi possível iniciar o servidor HTTP na porta {port}: {ex.Message}", ex);
        }

        return Task.CompletedTask;
    }

    private async Task ListenLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested && _listener != null && _listener.IsListening)
        {
            try
            {
                var context = await _listener.GetContextAsync().ConfigureAwait(false);
                _ = Task.Run(() => HandleRequestAsync(context), token);
            }
            catch (HttpListenerException) when (token.IsCancellationRequested || _listener == null || !_listener.IsListening)
            {
                // Expected when listener is stopped
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception)
            {
                // Continue listening
            }
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        // Apply CORS headers to all responses
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With");

        try
        {
            if (request.HttpMethod.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                response.StatusCode = (int)HttpStatusCode.NoContent;
                response.Close();
                return;
            }

            var path = request.Url?.AbsolutePath.TrimEnd('/').ToLowerInvariant() ?? string.Empty;

            if (request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                if (path == "" || path == "/api/ping")
                {
                    await WriteJsonResponseAsync(response, HttpStatusCode.OK, new
                    {
                        status = "ok",
                        app = "YoutubeDlGui",
                        version = "1.0",
                        port = Port
                    });
                    return;
                }

                if (path == "/api/status")
                {
                    object statusData = _statusProvider != null 
                        ? _statusProvider() 
                        : new { status = "running" };

                    await WriteJsonResponseAsync(response, HttpStatusCode.OK, statusData);
                    return;
                }
            }
            else if (request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                if (path == "/api/download")
                {
                    using var reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8);
                    var body = await reader.ReadToEndAsync();

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    ExternalDownloadRequest? downloadReq = null;
                    try
                    {
                        downloadReq = JsonSerializer.Deserialize<ExternalDownloadRequest>(body, options);
                    }
                    catch
                    {
                        // Deserialization failure
                    }

                    if (downloadReq == null || string.IsNullOrWhiteSpace(downloadReq.Url))
                    {
                        await WriteJsonResponseAsync(response, HttpStatusCode.BadRequest, new
                        {
                            success = false,
                            message = "URL inválida ou não fornecida."
                        });
                        return;
                    }

                    string url = downloadReq.Url.Trim();
                    if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                        !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        await WriteJsonResponseAsync(response, HttpStatusCode.BadRequest, new
                        {
                            success = false,
                            message = "A URL deve iniciar com http:// ou https://"
                        });
                        return;
                    }

                    // Fire the event on thread pool
                    DownloadRequested?.Invoke(this, downloadReq);

                    await WriteJsonResponseAsync(response, HttpStatusCode.OK, new
                    {
                        success = true,
                        message = "Download adicionado à fila do YoutubeDL-GUI com sucesso!",
                        url = downloadReq.Url
                    });
                    return;
                }
            }

            // Route not found
            await WriteJsonResponseAsync(response, HttpStatusCode.NotFound, new
            {
                error = "Endpoint não encontrado."
            });
        }
        catch (Exception ex)
        {
            try
            {
                await WriteJsonResponseAsync(response, HttpStatusCode.InternalServerError, new
                {
                    error = ex.Message
                });
            }
            catch
            {
                // Ignore failure in writing error response
            }
        }
    }

    private static async Task WriteJsonResponseAsync(HttpListenerResponse response, HttpStatusCode statusCode, object data)
    {
        response.StatusCode = (int)statusCode;
        response.ContentType = "application/json; charset=utf-8";

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var bytes = Encoding.UTF8.GetBytes(json);
        response.ContentLength64 = bytes.Length;

        await response.OutputStream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        response.Close();
    }

    public Task StopAsync()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        if (_listener != null)
        {
            try
            {
                _listener.Stop();
                _listener.Close();
            }
            catch
            {
                // Ignore listener close exception
            }
            finally
            {
                _listener = null;
            }
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _ = StopAsync();
        GC.SuppressFinalize(this);
    }
}
