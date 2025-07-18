using DownloadMusica.Interfaces.Services;
using DownloadMusica.Models;
using DownloadMusica.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddTransient<IYoutubeService, YoutubeService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapPost("/", async ([FromBody] UrlYoutube urlYoutube, IYoutubeService youtubeService) =>
{
    if (string.IsNullOrEmpty(urlYoutube.Url))
        return Results.BadRequest("O link do YouTube é obrigatório.");

    var url = urlYoutube.Url.Split("&list=")[0];
    string? titulo = await youtubeService.ObterTituloAsync(url);
    var fileStream = youtubeService.BaixarMusica(url);

    if (fileStream == null) 
        return Results.BadRequest("Erro ao baixar musica");

    return Results.File(fileStream, "audio/mpeg", $"{titulo}.mp3");

})
.WithName("Download")
.WithTags("Download")
.Accepts<UrlYoutube>("application/json");


app.Run();

