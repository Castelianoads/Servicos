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
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var processoTitulo = Process.Start(tituloProcessoInfo);
            if (processoTitulo == null) return string.Empty;

            var tituloResposta = await processoTitulo.StandardOutput.ReadToEndAsync();
            var erroResposta = await processoTitulo.StandardError.ReadToEndAsync();

            processoTitulo.WaitForExit();
            if (!string.IsNullOrWhiteSpace(erroResposta))
                Console.WriteLine($"Erro yt-dlp: {erroResposta}");
            

            return tituloResposta.Trim();
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
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var processoBaixar = Process.Start(processoInfo);
            if (processoBaixar == null)
                return null;

            var memStream = new MemoryStream();
            processoBaixar.StandardOutput.BaseStream.CopyTo(memStream);
            processoBaixar.WaitForExit();
            memStream.Position = 0;
            return memStream;
        }
        catch (Exception e)
        {
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
                Arguments = $"-x --audio-format mp3 -o \"{Path.Combine(destino, "%(title)s.%(ext)s")}\" {url}",
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
