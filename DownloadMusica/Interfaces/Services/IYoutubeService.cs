

namespace DownloadMusica.Interfaces.Services;

public interface IYoutubeService
{
    Task<Stream?> BaixarMusicaAsync(string urlYoutube);
    Task<string?> BaixarMusicaAsync(string url, string destino);
    Task<string?> ObterTituloAsync(string urlYoutube);
}
