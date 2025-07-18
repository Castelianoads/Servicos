

namespace DownloadMusica.Interfaces.Services;

public interface IYoutubeService
{
    Stream? BaixarMusica(string urlYoutube);
    Task<string?> ObterTituloAsync(string urlYoutube);
}
