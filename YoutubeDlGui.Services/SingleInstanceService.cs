using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using YoutubeDlGui.Core.Interfaces;

namespace YoutubeDlGui.Services;

public class SingleInstanceService : ISingleInstanceService
{
    private const string MutexName = "YoutubeDlGui_Application_Mutex_SingleInstance";
    private const string PipeName = "YoutubeDlGui_Application_NamedPipe";

    private readonly Mutex _mutex;
    private readonly bool _isFirstInstance;
    private CancellationTokenSource? _cts;
    private Task? _pipeServerTask;

    public bool IsFirstInstance => _isFirstInstance;

    public event EventHandler<string[]>? ArgumentsReceived;

    public SingleInstanceService()
    {
        _mutex = new Mutex(true, MutexName, out _isFirstInstance);
    }

    public void StartListening()
    {
        if (!_isFirstInstance) return;

        _cts = new CancellationTokenSource();
        _pipeServerTask = Task.Run(() => ServerLoopAsync(_cts.Token));
    }

    private async Task ServerLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                using var pipeServer = new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.In,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await pipeServer.WaitForConnectionAsync(token).ConfigureAwait(false);

                using var reader = new StreamReader(pipeServer, Encoding.UTF8);
                var content = await reader.ReadToEndAsync(token).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(content))
                {
                    try
                    {
                        var args = JsonSerializer.Deserialize<string[]>(content);
                        if (args != null && args.Length > 0)
                        {
                            ArgumentsReceived?.Invoke(this, args);
                        }
                    }
                    catch
                    {
                        // Single raw argument fallback
                        ArgumentsReceived?.Invoke(this, new[] { content.Trim() });
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // Continue waiting for subsequent connections
                await Task.Delay(200, token).ConfigureAwait(false);
            }
        }
    }

    public async Task<bool> SendArgsToFirstInstanceAsync(string[] args, int timeoutMs = 2000)
    {
        try
        {
            using var pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            await pipeClient.ConnectAsync(timeoutMs).ConfigureAwait(false);

            var json = JsonSerializer.Serialize(args);
            var bytes = Encoding.UTF8.GetBytes(json);

            await pipeClient.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            await pipeClient.FlushAsync().ConfigureAwait(false);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        if (_isFirstInstance)
        {
            try
            {
                _mutex.ReleaseMutex();
            }
            catch
            {
                // Mutex might already be released
            }
        }

        _mutex.Dispose();
        GC.SuppressFinalize(this);
    }
}
