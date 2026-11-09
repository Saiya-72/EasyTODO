# Multi-stage Dockerfile for the C# API.
#
#   target "dev"   -> used by docker-compose.yml. Runs `dotnet watch` so edits to .cs
#                     files on your machine rebuild and restart the API automatically.
#   target "final" -> small runtime-only image for deployment:
#                       docker build --target final -t todo-api ./backend
#
# If you switch the project to net8.0, change every 10.0 below to 8.0.

# ---------- dev ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dev
WORKDIR /src/TodoApi

# Restore first so NuGet packages are cached until the .csproj changes.
COPY TodoApi/TodoApi.csproj ./
RUN dotnet restore

COPY TodoApi/ ./

ENV ASPNETCORE_ENVIRONMENT=Development \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    DOTNET_WATCH_RESTART_ON_RUDE_EDIT=true
EXPOSE 8080

CMD ["dotnet", "watch", "run", "--no-launch-profile"]

# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src/TodoApi
COPY TodoApi/TodoApi.csproj ./
RUN dotnet restore
COPY TodoApi/ ./
RUN dotnet publish -c Release -o /app/publish --no-restore

# ---------- final ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "TodoApi.dll"]
