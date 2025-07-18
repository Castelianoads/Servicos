FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

# Instala yt-dlp e ffmpeg
USER root
#RUN apt-get update && apt-get install -y python3 python3-pip python3-venv wget ffmpeg
#RUN python3 -m venv /opt/venv
#RUN /opt/venv/bin/pip install --upgrade yt-dlp
#RUN pip install yt-dlp 

RUN apt-get update && \
    apt-get install -y ffmpeg curl && \
    curl -L https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp -o /usr/local/bin/yt-dlp && \
    chmod a+rx /usr/local/bin/yt-dlp && \
    yt-dlp --version


USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DownloadMusica.csproj", "."]
RUN dotnet restore "./DownloadMusica.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./DownloadMusica.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DownloadMusica.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final


WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DownloadMusica.dll"]


# docker build -t musica-api .
# docker run -d -p 8080:8080 -p 8081:8081 --name musica-api musica-api