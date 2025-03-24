# ---------------------------------------
# Etapa 1: Build
# ---------------------------------------
    FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
    WORKDIR /app
    
    # Copia o csproj para restaurar dependências
    COPY Pregiato.API/Pregiato.API.csproj Pregiato.API/
    WORKDIR /app/Pregiato.API
    RUN dotnet restore "Pregiato.API.csproj"
    
    # Copia todo o conteúdo da pasta Pregiato.API
    COPY Pregiato.API/. .
    
    # Publica a aplicação (Release, sem self-contained)
    RUN dotnet publish "Pregiato.API.csproj" -c Release --no-self-contained -o /app/out
    
    # ---------------------------------------
    # Etapa 2: Runtime
    # ---------------------------------------
    FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
    WORKDIR /app
    
    # Se sua aplicação PRECISA rodar o Chromium em produção, instale:
    RUN apt-get update && apt-get install -y \
        chromium \
        libx11-xcb1 libxcomposite1 libxdamage1 libxi6 libxtst6 libnss3 \
        libxrandr2 libasound2 libpangocairo-1.0-0 libatk1.0-0 \
        libatk-bridge2.0-0 libgtk-3-0 libgdiplus libc6-dev libssl-dev \
        libfontconfig1 libcairo2 libjpeg-dev libpango-1.0-0 libgif-dev \
        libicu-dev zlib1g-dev libharfbuzz0b libfreetype6-dev \
        && rm -rf /var/lib/apt/lists/*
    
    # Cria links simbólicos necessários
    RUN ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll && \
        ln -s /lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/libdl.so
    
    # Copia os arquivos publicados da etapa de build
    COPY --from=build /app/out ./
    
    # Define a porta de exposição
    EXPOSE 8080
    
    # Permite que o Railway (ou outro) use a variável PORT, senão usa 8080.
    ENV ASPNETCORE_URLS=http://+:${PORT}
    ENV PORT=8080
    
    # Executa a aplicação
    ENTRYPOINT ["dotnet", "Web.Api.dll"]

    