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
    var fileStream = youtubeService.BaixarMusica(url);

    if (fileStream == null) 
        return Results.BadRequest("Erro ao baixar musica");

    return Results.File(fileStream, "audio/mpeg", $"{titulo}.mp3");

})
.WithName("Download")
.WithTags("Download")
.Accepts<UrlYoutube>("application/json");


app.Run();

