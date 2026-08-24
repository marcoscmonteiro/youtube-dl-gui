namespace YoutubeDlGui.Core.Interfaces;

public interface ISingleInstanceService : IDisposable
{
    bool IsFirstInstance { get; }
    void StartListening();
    Task<bool> SendArgsToFirstInstanceAsync(string[] args, int timeoutMs = 2000);
    event EventHandler<string[]>? ArgumentsReceived;
}
