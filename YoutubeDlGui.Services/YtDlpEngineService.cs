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
    private const string QuickJsLatestReleaseUrl = "https://github.com/quickjs-ng/quickjs/releases/latest/download/qjs-windows-x86_64.exe";

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
        @"\[(?:download|ffmpeg|Merger|ExtractAudio|VideoConvertor|FixupM3u8)\]\s+(?:Destination:\s*|Merging formats into\s*""?|Converting video from [^;]+;\s*Destination:\s*)(?<filename>[^""]+?)""?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex AlreadyDownloadedRegex = new(
        @"\[download\]\s+(?<filename>[^\r\n]+?)\s+has already been downloaded",
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

    public string? ResolveQuickJsExecutablePath()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string qjsInBaseDir = Path.Combine(baseDir, "qjs.exe");
        if (File.Exists(qjsInBaseDir)) return qjsInBaseDir;

        string enginePath = ResolveEngineExecutablePath(string.Empty);
        if (!string.IsNullOrEmpty(enginePath) && File.Exists(enginePath))
        {
            string? engineDir = Path.GetDirectoryName(enginePath);
            if (!string.IsNullOrEmpty(engineDir))
            {
                string qjsInEngineDir = Path.Combine(engineDir, "qjs.exe");
                if (File.Exists(qjsInEngineDir)) return qjsInEngineDir;
            }
        }

        // Try PATH environment variable
        string? pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (pathEnv != null)
        {
            foreach (string path in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                string fullPath = Path.Combine(path.Trim(), "qjs.exe");
                if (File.Exists(fullPath)) return fullPath;
            }
        }

        return null;
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

        outputProgress?.Report($"Conectando ao GitHub para baixar a versão mais recente do yt-dlp...");
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
                            outputProgress?.Report($"[yt-dlp] {totalBytesRead / (1024.0 * 1024.0):F1} MB / {totalSizeStr} ({percent:F1}%) - {speedMBps:F1} MB/s");
                        }
                        else
                        {
                            outputProgress?.Report($"[yt-dlp] {totalBytesRead / (1024.0 * 1024.0):F1} MB baixados - {speedMBps:F1} MB/s");
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

            return true;
        }
        catch (OperationCanceledException)
        {
            outputProgress?.Report("\n[Cancelado] Download do yt-dlp cancelado pelo usuário.");
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

    public async Task<bool> DownloadQuickJsFromGitHubAsync(
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

        string finalExePath = Path.Combine(targetDir, "qjs.exe");
        string tempDownloadPath = Path.Combine(targetDir, "qjs.exe.download");

        outputProgress?.Report($"\nConectando ao GitHub para baixar o interpretador QuickJS (qjs.exe)...");
        outputProgress?.Report($"URL: {QuickJsLatestReleaseUrl}");

        try
        {
            using var response = await HttpClient.GetAsync(QuickJsLatestReleaseUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            long? totalBytes = response.Content.Headers.ContentLength;
            string totalSizeStr = totalBytes.HasValue ? $"{totalBytes.Value / (1024.0 * 1024.0):F2} MB" : "Tamanho desconhecido";
            outputProgress?.Report($"Tamanho do QuickJS: {totalSizeStr}");

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
                            outputProgress?.Report($"[QuickJS] {totalBytesRead / (1024.0 * 1024.0):F1} MB / {totalSizeStr} ({percent:F1}%) - {speedMBps:F1} MB/s");
                        }
                        else
                        {
                            outputProgress?.Report($"[QuickJS] {totalBytesRead / (1024.0 * 1024.0):F1} MB baixados - {speedMBps:F1} MB/s");
                        }

                        lastReportTime.Restart();
                    }
                }
            }

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

            outputProgress?.Report($"\n[Sucesso] qjs.exe instalado com sucesso em:");
            outputProgress?.Report($"{finalExePath}");

            return true;
        }
        catch (OperationCanceledException)
        {
            outputProgress?.Report("\n[Cancelado] Download do QuickJS cancelado pelo usuário.");
            if (File.Exists(tempDownloadPath))
            {
                try { File.Delete(tempDownloadPath); } catch { }
            }
            return false;
        }
        catch (Exception ex)
        {
            outputProgress?.Report($"\n[Erro] Falha ao baixar qjs.exe do GitHub: {ex.Message}");
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

            string targetDir = AppDomain.CurrentDomain.BaseDirectory;
            bool downloaded = await DownloadLatestFromGitHubAsync(targetDir, downloadProgress, cancellationToken);
            _ = await DownloadQuickJsFromGitHubAsync(targetDir, downloadProgress, cancellationToken);

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

        var startTime = DateTime.Now;
        var logBuilder = new StringBuilder();
        logBuilder.AppendLine("[yt-dlp GUI] ======================================================================");
        logBuilder.AppendLine($"[yt-dlp GUI] Início da Execução: {startTime:dd/MM/yyyy HH:mm:ss}");
        logBuilder.AppendLine($"[yt-dlp GUI] URL: {item.Url}");
        logBuilder.AppendLine($"[yt-dlp GUI] Executável: {exePath}");
        logBuilder.AppendLine($"[yt-dlp GUI] Diretório de Saída: {workDir}");
        logBuilder.AppendLine($"[yt-dlp GUI] Argumentos: {item.CommandLineArguments}");
        logBuilder.AppendLine("[yt-dlp GUI] ======================================================================");

        lock (logBuilder)
        {
            item.Log = logBuilder.ToString();
        }

        progress.Report(new DownloadProgressReport
        {
            RawLogLine = logBuilder.ToString().TrimEnd(),
            StatusText = "Iniciando download..."
        });

        process.OutputDataReceived += (s, e) =>
        {
            if (string.IsNullOrEmpty(e.Data)) return;

            string line = e.Data;
            lock (logBuilder)
            {
                logBuilder.AppendLine(line);
                item.Log = logBuilder.ToString();
            }

            var report = new DownloadProgressReport
            {
                RawLogLine = line
            };

            var matchProg = ProgressRegex.Match(line);
            if (matchProg.Success)
            {
                if (double.TryParse(matchProg.Groups["percent"].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double p))
                {
                    report.Percentage = p;
                }
                report.TotalSize = matchProg.Groups["size"].Value;
                report.Speed = matchProg.Groups["speed"].Value;
                report.Eta = matchProg.Groups["eta"].Value;
                report.StatusText = "Downloading...";
            }
            else
            {
                var matchDest = DestinationRegex.Match(line);
                if (matchDest.Success)
                {
                    report.ExtractedFileName = Path.GetFileName(matchDest.Groups["filename"].Value.Trim());
                    report.StatusText = "Processing...";
                }
                else
                {
                    var matchAlready = AlreadyDownloadedRegex.Match(line);
                    if (matchAlready.Success)
                    {
                        report.ExtractedFileName = Path.GetFileName(matchAlready.Groups["filename"].Value.Trim());
                        report.StatusText = "Already downloaded";
                    }
                    else if (line.Contains("[ExtractAudio]", StringComparison.OrdinalIgnoreCase))
                    {
                        report.StatusText = "Converting audio...";
                    }
                    else if (line.Contains("[Merger]", StringComparison.OrdinalIgnoreCase))
                    {
                        report.StatusText = "Merging formats...";
                    }
                }
            }

            progress.Report(report);
        };

        process.ErrorDataReceived += (s, e) =>
        {
            if (string.IsNullOrEmpty(e.Data)) return;

            string line = e.Data;
            string errLine = $"[STDERR] {line}";
            lock (logBuilder)
            {
                logBuilder.AppendLine(errLine);
                item.Log = logBuilder.ToString();
            }

            progress.Report(new DownloadProgressReport
            {
                RawLogLine = errLine,
                StatusText = line.Length > 40 ? line[..40] + "..." : line
            });
        };

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

            var elapsed = DateTime.Now - startTime;
            string completionStatus = process.ExitCode == 0 ? "Sucesso (Código 0)" : $"Falha (Código {process.ExitCode})";
            string footer = $"[yt-dlp GUI] ----------------------------------------------------------------------\n[yt-dlp GUI] Finalizado: {DateTime.Now:dd/MM/yyyy HH:mm:ss} | Status: {completionStatus} | Duração: {elapsed:mm\\:ss}\n[yt-dlp GUI] ======================================================================";
            
            lock (logBuilder)
            {
                logBuilder.AppendLine(footer);
                item.Log = logBuilder.ToString();
            }

            progress.Report(new DownloadProgressReport
            {
                RawLogLine = footer,
                StatusText = process.ExitCode == 0 ? "Concluído" : "Falhou"
            });

            return process.ExitCode == 0;
        }
        catch (OperationCanceledException)
        {
            var elapsed = DateTime.Now - startTime;
            string cancelMsg = $"[yt-dlp GUI] ----------------------------------------------------------------------\n[yt-dlp GUI] Cancelado pelo usuário em: {DateTime.Now:dd/MM/yyyy HH:mm:ss} (Duração: {elapsed:mm\\:ss})\n[yt-dlp GUI] ======================================================================";
            lock (logBuilder)
            {
                logBuilder.AppendLine(cancelMsg);
                item.Log = logBuilder.ToString();
            }
            progress.Report(new DownloadProgressReport { RawLogLine = cancelMsg, StatusText = "Cancelado" });
            return false;
        }
        catch (Exception ex)
        {
            var elapsed = DateTime.Now - startTime;
            string errMsg = $"[yt-dlp GUI] ----------------------------------------------------------------------\n[yt-dlp GUI] Erro de execução: {ex.Message} (Duração: {elapsed:mm\\:ss})\n[yt-dlp GUI] ======================================================================";
            lock (logBuilder)
            {
                logBuilder.AppendLine(errMsg);
                item.Log = logBuilder.ToString();
            }
            progress.Report(new DownloadProgressReport { RawLogLine = errMsg, StatusText = "Erro" });
            return false;
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(item.TemporaryCookieFilePath))
            {
                try
                {
                    if (File.Exists(item.TemporaryCookieFilePath))
                    {
                        File.Delete(item.TemporaryCookieFilePath);
                    }
                }
                catch { }
            }
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
        string targetDir = Path.GetDirectoryName(exePath) ?? AppDomain.CurrentDomain.BaseDirectory;
        var output = new StringBuilder();
        
        if (!File.Exists(exePath))
        {
            outputProgress?.Report($"[Aviso] O executável '{engineExecutable}' não foi encontrado localmente.");
            outputProgress?.Report("Baixando a versão oficial mais recente diretamente do repositório do yt-dlp no GitHub...\n");

            bool success = await DownloadLatestFromGitHubAsync(targetDir, outputProgress, cancellationToken);
            output.AppendLine(success ? "Download do yt-dlp concluído com sucesso." : "Falha no download do yt-dlp do GitHub.");
        }
        else
        {
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
                    await DownloadLatestFromGitHubAsync(targetDir, outputProgress, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                outputProgress?.Report($"\n[Aviso] Erro ao executar -U ({ex.Message}).");
                outputProgress?.Report("Baixando a versão oficial mais recente do GitHub...\n");
                await DownloadLatestFromGitHubAsync(targetDir, outputProgress, cancellationToken);
            }
        }

        // Always download / update QuickJS (qjs.exe) in the same directory
        outputProgress?.Report("\n--------------------------------------------------");
        outputProgress?.Report("Verificando / Atualizando o interpretador QuickJS (qjs.exe)...");
        bool qjsSuccess = await DownloadQuickJsFromGitHubAsync(targetDir, outputProgress, cancellationToken);
        if (qjsSuccess)
        {
            output.AppendLine("QuickJS (qjs.exe) atualizado com sucesso.");
        }

        return output.ToString();
    }
}
