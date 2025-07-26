

namespace DownloadMusica.Interfaces.Services;

public interface IYoutubeService
{
    Stream? BaixarMusica(string urlYoutube);
    Task<string?> BaixarMusicaAsync(string url, string destino);
    Task<string?> ObterTituloAsync(string urlYoutube);
}
