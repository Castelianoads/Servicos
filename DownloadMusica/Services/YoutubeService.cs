using DownloadMusica.Interfaces.Services;
using System.Diagnostics;

namespace DownloadMusica.Services;

public class YoutubeService : IYoutubeService
{
    private readonly ILogger<YoutubeService> _logger;   
    public YoutubeService(ILogger<YoutubeService> logger)
    {
        _logger = logger;
    }   

    public async Task<string?> ObterTituloAsync(string urlYoutube)
    {
        try
        {
            var tituloProcessoInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $"--get-title {urlYoutube}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var processoTitulo = Process.Start(tituloProcessoInfo);
            if (processoTitulo == null)
            {
                _logger.LogError("Processo Info de obter o titulo null.");
                return string.Empty;
            }

            var entradaTask = processoTitulo.StandardOutput.ReadToEndAsync();
            var erroTask = processoTitulo.StandardError.ReadToEndAsync();

            await processoTitulo.WaitForExitAsync();
            
            string saidaResposta = await entradaTask;
            string erroResposta = await erroTask;

            if (!string.IsNullOrWhiteSpace(erroResposta))
            {
                _logger.LogError($"Erro do yt-dlp: {erroResposta}");
                return null;
            }

            if (string.IsNullOrWhiteSpace(saidaResposta))
            {
                _logger.LogWarning($"yt-dlp não retornou título para URL: {urlYoutube}");
                return null;
            }

            return saidaResposta.Trim();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao obter titulo da musica.");
            return string.Empty;
        }        
    }

    public async Task<Stream?> BaixarMusicaAsync(string urlYoutube)
    {
        try
        {
            string arquivoTemporario = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mp3");

            var processoInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $"-f bestaudio --extract-audio --audio-format mp3 --audio-quality 0 -o \"{arquivoTemporario}\" {urlYoutube}",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var processoBaixar = Process.Start(processoInfo);
            if (processoBaixar == null)
            {
                _logger.LogError("Processo Info de baixar musica null.");
                return null;
            }

            await processoBaixar.WaitForExitAsync();

            if (!File.Exists(arquivoTemporario))
            {
                _logger.LogError($"yt-dlp falhou. Arquivo {arquivoTemporario} não existe.");
                return null;
            }

            var memoryStream = new MemoryStream();
            await using (var fileStream = File.OpenRead(arquivoTemporario))
            {
                await fileStream.CopyToAsync(memoryStream);
            }

            File.Delete(arquivoTemporario);
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao baixar musica.");
            return null;
        }
    }

    public async Task<string?> BaixarMusicaAsync(string url, string destino)
    {
        try
        {
            var processoInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $"-f bestaudio --extract-audio --audio-format mp3 --audio-quality 0 -o \"{Path.Combine(destino, "%(title)s.%(ext)s")}\" {url}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var processoBaixar = Process.Start(processoInfo);
            if (processoBaixar == null)
                return null;

            await processoBaixar.WaitForExitAsync();
            var arquivo = Directory.GetFiles(destino)
                                   .OrderByDescending(File.GetCreationTime)
                                   .FirstOrDefault();

            return arquivo;
        }
        catch
        {
            return null;
        }
    }
}
