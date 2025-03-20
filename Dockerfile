# Etapa 1: Construção da aplicação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copie o .csproj da subpasta Pregiato.API
COPY Pregiato.API/Pregiato.API.csproj ./
RUN dotnet restore

# Copie o restante do código da subpasta Pregiato.API
COPY Pregiato.API/ ./
RUN dotnet publish -c Release --no-self-contained -o /app/publish

# Etapa 2: Execução da aplicação
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Instalar dependências para PuppeteerSharp (Chromium)
RUN apt-get update && apt-get install -y \
    chromium \
    libx11-xcb1 \
    libxcomposite1 \
    libxdamage1 \
    libxi6 \
    libxtst6 \
    libnss3 \
    libxrandr2 \
    libasound2 \
    libpangocairo-1.0-0 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libgtk-3-0 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "Pregiato.API.dll"]