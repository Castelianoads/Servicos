using DownloadMusica.Interfaces.Services;
using DownloadMusica.Models;
using DownloadMusica.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Baixar musica API", Version = "v1" });
});

builder.Services.AddTransient<IYoutubeService, YoutubeService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
   
}

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Baixar musica API V1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();


app.MapPost("/", async ([FromBody] UrlYoutube urlYoutube, IYoutubeService youtubeService) =>
{
    if (string.IsNullOrEmpty(urlYoutube.Url))
        return Results.BadRequest("O link do YouTube é obrigatório.");

    var url = urlYoutube.Url.Split("&list=")[0];
    string? titulo = await youtubeService.ObterTituloAsync(url);
    if (string.IsNullOrEmpty(titulo))
        titulo = "Titulo desconhecido";


    var tituloSeguro = string.Concat(titulo.Split(Path.GetInvalidFileNameChars()));
    var fileStream = await youtubeService.BaixarMusicaAsync(url);

    if (fileStream == null) 
        return Results.BadRequest("Erro ao baixar musica");

    return Results.File(fileStream, "audio/mpeg", $"{tituloSeguro}.mp3", enableRangeProcessing: true);

})
.WithName("Download")
.WithTags("Download")
.Accepts<UrlYoutube>("application/json");

app.MapPost("/lista", async ([FromBody] UrlYoutubeLista urlYoutube, IYoutubeService youtubeService) =>
{
    if (urlYoutube.Urls == null || !urlYoutube.Urls.Any())
        return Results.BadRequest("Lista de URLs está vazia.");

    if(urlYoutube.Urls.Count > 5)
        return Results.BadRequest("Permitido maximo de 5 musicas.");

    var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    Directory.CreateDirectory(tempDir);

    var arquivosBaixados = new List<string>();
    foreach (var urlCompleta in urlYoutube.Urls)
    {
        var url = urlCompleta.Split("&list=")[0];
        var musicaPath = await youtubeService.BaixarMusicaAsync(url, tempDir);
        if (!string.IsNullOrWhiteSpace(musicaPath))
            arquivosBaixados.Add(musicaPath);
    }

    if (!arquivosBaixados.Any())
        return Results.BadRequest("Nenhuma música foi baixada com sucesso.");

    var memoriaZip = new MemoryStream();
    using (var zip = new System.IO.Compression.ZipArchive(memoriaZip, System.IO.Compression.ZipArchiveMode.Create, leaveOpen: true))
    {
        foreach (var file in arquivosBaixados)
        {
            var entry = zip.CreateEntry(Path.GetFileName(file));
            using var entryStream = entry.Open();
            using var fileStream = File.OpenRead(file);
            await fileStream.CopyToAsync(entryStream);
        }
    }

    memoriaZip.Position = 0;
    try { Directory.Delete(tempDir, true); } catch { }

    return Results.File(memoriaZip, "application/zip", "musicas.zip");
})
.WithName("DownloadVarios")
.WithTags("DownloadVarios")
.Accepts<UrlYoutubeLista>("application/json");


app.Run();

