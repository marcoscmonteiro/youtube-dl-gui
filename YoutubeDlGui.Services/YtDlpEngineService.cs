using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using YoutubeDlGui.Core.Enums;
using YoutubeDlGui.Core.Interfaces;
using YoutubeDlGui.Core.Models;

namespace YoutubeDlGui.Services;

public class YtDlpEngineService : IDownloadEngineService
{
    private const string GitHubLatestReleaseUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe";
    private static readonly HttpClient HttpClient = new(new HttpClientHandler
    {
        AllowAutoRedirect = true,
        MaxAutomaticRedirections = 10
    })
    {
        Timeout = TimeSpan.FromMinutes(5)
    };

    static YtDlpEngineService()
    {
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) yt-dlp-gui");
    }

    private static readonly Regex ProgressRegex = new(
        @"\[download\]\s+(?<percent>\d+(?:\.\d+)?)\s*%\s+of\s+~?(?<size>[^\s]+)\s+at\s+(?<speed>[^\s]+)\s+ETA\s+(?<eta>[^\s]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex DestinationRegex = new(
        @"\[(?:download|ffmpeg|Merger)\]\s+(?:Destination:\s*|Merging formats into\s*""?)(?<filename>[^""]+?)""?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string ResolveEngineExecutablePath(string customNameOrPath)
    {
        if (string.IsNullOrWhiteSpace(customNameOrPath))
        {
            customNameOrPath = "yt-dlp.exe";
        }

        if (Path.IsPathRooted(customNameOrPath) && File.Exists(customNameOrPath))
        {
            return customNameOrPath;
        }

        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string inBaseDir = Path.Combine(baseDir, customNameOrPath);
        if (File.Exists(inBaseDir))
        {
            return inBaseDir;
        }

        // Check for yt-dlp.exe then youtube-dl.exe in base dir
        string ytdlpBase = Path.Combine(baseDir, "yt-dlp.exe");
        if (File.Exists(ytdlpBase)) return ytdlpBase;

        string ytdlBase = Path.Combine(baseDir, "youtube-dl.exe");
        if (File.Exists(ytdlBase)) return ytdlBase;

        // Try PATH environment variable
        string? pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (pathEnv != null)
        {
            foreach (string path in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                string fullPath = Path.Combine(path.Trim(), customNameOrPath);
                if (File.Exists(fullPath)) return fullPath;
                if (!customNameOrPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    string fullPathExe = Path.Combine(path.Trim(), customNameOrPath + ".exe");
                    if (File.Exists(fullPathExe)) return fullPathExe;
                }
            }
        }

        return inBaseDir;
    }

    public bool IsEngineInstalled(string customNameOrPath = "")
    {
        string path = ResolveEngineExecutablePath(customNameOrPath);
        return File.Exists(path);
    }

    public async Task<bool> DownloadLatestFromGitHubAsync(
        string? targetDirectory = null, 
        IProgress<string>? outputProgress = null, 
        CancellationToken cancellationToken = default)
    {
        string targetDir = string.IsNullOrWhiteSpace(targetDirectory)
            ? AppDomain.CurrentDomain.BaseDirectory
            : targetDirectory;

        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        string finalExePath = Path.Combine(targetDir, "yt-dlp.exe");
        string tempDownloadPath = Path.Combine(targetDir, "yt-dlp.exe.download");

        outputProgress?.Report($"Conectando ao GitHub para baixar a versão mais recente...");
        outputProgress?.Report($"URL: {GitHubLatestReleaseUrl}");

        try
        {
            using var response = await HttpClient.GetAsync(GitHubLatestReleaseUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            long? totalBytes = response.Content.Headers.ContentLength;
            string totalSizeStr = totalBytes.HasValue ? $"{totalBytes.Value / (1024.0 * 1024.0):F2} MB" : "Tamanho desconhecido";
            outputProgress?.Report($"Tamanho do download: {totalSizeStr}");

            await using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
            await using (var fileStream = new FileStream(tempDownloadPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
            {
                var buffer = new byte[81920];
                long totalBytesRead = 0;
                int bytesRead;
                var stopwatch = Stopwatch.StartNew();
                var lastReportTime = Stopwatch.StartNew();

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    totalBytesRead += bytesRead;

                    if (lastReportTime.ElapsedMilliseconds > 500)
                    {
                        double elapsedSec = stopwatch.Elapsed.TotalSeconds;
                        double speedMBps = elapsedSec > 0 ? (totalBytesRead / (1024.0 * 1024.0)) / elapsedSec : 0;

                        if (totalBytes.HasValue && totalBytes.Value > 0)
                        {
                            double percent = (double)totalBytesRead / totalBytes.Value * 100.0;
                            outputProgress?.Report($"[Download] {totalBytesRead / (1024.0 * 1024.0):F1} MB / {totalSizeStr} ({percent:F1}%) - {speedMBps:F1} MB/s");
                        }
                        else
                        {
                            outputProgress?.Report($"[Download] {totalBytesRead / (1024.0 * 1024.0):F1} MB baixados - {speedMBps:F1} MB/s");
                        }

                        lastReportTime.Restart();
                    }
                }
            }

            // Move temp file to destination
            if (File.Exists(finalExePath))
            {
                try
                {
                    File.Delete(finalExePath);
                }
                catch
                {
                    string oldBackup = finalExePath + ".old";
                    if (File.Exists(oldBackup)) File.Delete(oldBackup);
                    File.Move(finalExePath, oldBackup);
                }
            }

            File.Move(tempDownloadPath, finalExePath);

            outputProgress?.Report($"\n[Sucesso] yt-dlp.exe instalado com sucesso em:");
            outputProgress?.Report($"{finalExePath}");

            // Verify installation by getting version
            string version = await GetHelpAsync(finalExePath, cancellationToken);
            if (!string.IsNullOrEmpty(version))
            {
                outputProgress?.Report($"\nEngine pronta para uso!");
            }

            return true;
        }
        catch (OperationCanceledException)
        {
            outputProgress?.Report("\n[Cancelado] Download cancelado pelo usuário.");
            if (File.Exists(tempDownloadPath))
            {
                try { File.Delete(tempDownloadPath); } catch { }
            }
            return false;
        }
        catch (Exception ex)
        {
            outputProgress?.Report($"\n[Erro] Falha ao baixar yt-dlp do GitHub: {ex.Message}");
            if (File.Exists(tempDownloadPath))
            {
                try { File.Delete(tempDownloadPath); } catch { }
            }
            return false;
        }
    }

    public async Task<bool> DownloadAsync(
        DownloadItem item, 
        IProgress<DownloadProgressReport> progress, 
        CancellationToken cancellationToken = default)
    {
        string workDir = string.IsNullOrWhiteSpace(item.OutputDirectory)
            ? Environment.CurrentDirectory
            : item.OutputDirectory.Trim();

        if (!Directory.Exists(workDir))
        {
            Directory.CreateDirectory(workDir);
        }

        string exePath = ResolveEngineExecutablePath(string.Empty);
        if (!File.Exists(exePath))
        {
            item.Log = $"Executável da engine não encontrado em: {exePath}\nTentando baixar automaticamente do GitHub...";
            var report = new DownloadProgressReport
            {
                StatusText = "Baixando yt-dlp.exe do GitHub..."
            };
            progress.Report(report);

            var downloadProgress = new Progress<string>(line =>
            {
                progress.Report(new DownloadProgressReport { StatusText = line, RawLogLine = line });
            });

            bool downloaded = await DownloadLatestFromGitHubAsync(AppDomain.CurrentDomain.BaseDirectory, downloadProgress, cancellationToken);
            if (!downloaded || !File.Exists(exePath))
            {
                item.Log += "\nNão foi possível baixar yt-dlp.exe automaticamente. Por favor, clique em 'Atualizar Engine' na barra superior.";
                return false;
            }
        }

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = item.CommandLineArguments,
            WorkingDirectory = workDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        var logBuilder = new StringBuilder();
        logBuilder.AppendLine($"Executable: {exePath}");
        logBuilder.AppendLine($"Arguments : {item.CommandLineArguments}");
        logBuilder.AppendLine($"Work Dir  : {workDir}");
        logBuilder.AppendLine($"Started   : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        logBuilder.AppendLine(new string('-', 50));

        void HandleOutput(string? data)
        {
            if (string.IsNullOrEmpty(data)) return;

            lock (logBuilder)
            {
                logBuilder.AppendLine(data);
            }

            var report = new DownloadProgressReport
            {
                RawLogLine = data
            };

            var matchDest = DestinationRegex.Match(data);
            if (matchDest.Success)
            {
                report.ExtractedFileName = matchDest.Groups["filename"].Value.Trim().Trim('\"');
            }

            var matchProg = ProgressRegex.Match(data);
            if (matchProg.Success)
            {
                if (double.TryParse(matchProg.Groups["percent"].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double p))
                {
                    report.Percentage = p;
                }
                report.TotalSize = matchProg.Groups["size"].Value;
                report.Speed = matchProg.Groups["speed"].Value;
                report.Eta = matchProg.Groups["eta"].Value;
                report.StatusText = $"Downloading {report.Percentage:F1}%";
            }
            else
            {
                report.StatusText = data;
            }

            progress.Report(report);
        }

        process.OutputDataReceived += (s, e) => HandleOutput(e.Data);
        process.ErrorDataReceived += (s, e) => HandleOutput(e.Data);

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using var ctr = cancellationToken.Register(() =>
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(true);
                    }
                }
                catch { }
            });

            await process.WaitForExitAsync(cancellationToken);

            item.Log = logBuilder.ToString();
            return process.ExitCode == 0;
        }
        catch (OperationCanceledException)
        {
            logBuilder.AppendLine("Download cancelled by user.");
            item.Log = logBuilder.ToString();
            return false;
        }
        catch (Exception ex)
        {
            logBuilder.AppendLine($"Error: {ex.Message}");
            item.Log = logBuilder.ToString();
            return false;
        }
    }

    public async Task<string> GetHelpAsync(string engineExecutable, CancellationToken cancellationToken = default)
    {
        string exePath = ResolveEngineExecutablePath(engineExecutable);
        if (!File.Exists(exePath))
        {
            return $"Engine executable not found at: {exePath}\nClique em 'Atualizar Engine' na barra superior para instalar automaticamente a versão mais recente.";
        }

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = "--help",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        process.Start();
        string output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return output;
    }

    public async Task<string> UpdateEngineAsync(string engineExecutable, IProgress<string>? outputProgress = null, CancellationToken cancellationToken = default)
    {
        string exePath = ResolveEngineExecutablePath(engineExecutable);
        
        if (!File.Exists(exePath))
        {
            outputProgress?.Report($"[Aviso] O executável '{engineExecutable}' não foi encontrado localmente.");
            outputProgress?.Report("Baixando a versão oficial mais recente diretamente do repositório do yt-dlp no GitHub...\n");

            bool success = await DownloadLatestFromGitHubAsync(AppDomain.CurrentDomain.BaseDirectory, outputProgress, cancellationToken);
            return success ? "Download concluído com sucesso." : "Falha no download do GitHub.";
        }

        outputProgress?.Report($"Executando verificação de atualização: {exePath} -U\n");

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = "-U",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        var output = new StringBuilder();
        bool hasError = false;

        process.OutputDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                output.AppendLine(e.Data);
                outputProgress?.Report(e.Data);
            }
        };
        process.ErrorDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                output.AppendLine(e.Data);
                outputProgress?.Report(e.Data);
                if (e.Data.Contains("ERROR:", StringComparison.OrdinalIgnoreCase) || e.Data.Contains("fail", StringComparison.OrdinalIgnoreCase))
                {
                    hasError = true;
                }
            }
        };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0 || hasError)
            {
                outputProgress?.Report("\n[Aviso] A atualização via '-U' não pôde ser concluída.");
                outputProgress?.Report("Tentando baixar a versão binária mais recente do GitHub...\n");
                await DownloadLatestFromGitHubAsync(AppDomain.CurrentDomain.BaseDirectory, outputProgress, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            outputProgress?.Report($"\n[Aviso] Erro ao executar -U ({ex.Message}).");
            outputProgress?.Report("Baixando a versão oficial mais recente do GitHub...\n");
            await DownloadLatestFromGitHubAsync(AppDomain.CurrentDomain.BaseDirectory, outputProgress, cancellationToken);
        }

        return output.ToString();
    }
}
