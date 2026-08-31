namespace YoutubeDlGui.Core.Interfaces;

/// <summary>
/// Serviço responsável por verificar e aplicar atualizações da aplicação a partir do compartilhamento de rede local.
/// </summary>
public interface INetworkUpdateService
{
    /// <summary>
    /// Indica se há uma versão mais recente disponível no servidor de rede.
    /// </summary>
    bool IsUpdateAvailable { get; }

    /// <summary>
    /// Versão mais recente disponível no servidor de rede (ex: "v2.1.0").
    /// </summary>
    string AvailableVersion { get; }

    /// <summary>
    /// Caminho do repositório/compartilhamento de rede (UNC).
    /// </summary>
    string NetworkRepositoryPath { get; }

    /// <summary>
    /// Verifica em segundo plano se há atualizações disponíveis no servidor de rede.
    /// </summary>
    Task<bool> CheckForUpdatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Aplica a atualização a partir do servidor de rede e reinicia o aplicativo.
    /// </summary>
    Task<bool> ApplyUpdateAndRestartAsync();
}

