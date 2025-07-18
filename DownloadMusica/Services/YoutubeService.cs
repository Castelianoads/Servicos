using DownloadMusica.Interfaces.Services;
using System.Diagnostics;

namespace DownloadMusica.Services;

public class YoutubeService : IYoutubeService
{
    public async Task<string?> ObterTituloAsync(string urlYoutube)
    {
        try
        {
            var tituloProcessoInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $"--get-title {urlYoutube}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var processoTitulo = Process.Start(tituloProcessoInfo);
            if (processoTitulo == null) return string.Empty;

            var tituloResposta = await processoTitulo.StandardOutput.ReadToEndAsync();
            processoTitulo.WaitForExit();
            return tituloResposta;
        }
        catch (Exception e)
        {
            return string.Empty;
        }        
    }

    public Stream? BaixarMusica(string urlYoutube)
    {
        try
        {
            var processoInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $"-x --audio-format mp3 --audio-quality 0 --output - {urlYoutube}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var processoBaixar = Process.Start(processoInfo);
            if (processoBaixar == null)
                return null;

            return processoBaixar.StandardOutput.BaseStream;
        }
        catch (Exception e)
        {
            return null;
        }
    }
}
