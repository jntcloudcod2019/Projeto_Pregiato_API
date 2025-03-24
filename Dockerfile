# Etapa 1: Base para runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /publish 
EXPOSE 8080

# Etapa 2: Construção da aplicação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /publish 
COPY ["Pregiato.API.csproj", "./"]
RUN dotnet restore "./Pregiato.API.csproj"

# Copiar o restante do projeto
COPY . .
RUN dotnet build "./Pregiato.API.csproj" -c Release -o /build

# Instalar dependências do Chromium
RUN apt-get update && apt-get install -y \
    chromium \
    libx11-xcb1 libxcomposite1 libxdamage1 libxi6 libxtst6 libnss3 \
    libxrandr2 libasound2 libpangocairo-1.0-0 libatk1.0-0 \
    libatk-bridge2.0-0 libgtk-3-0 libgdiplus libc6-dev libssl-dev \
    libfontconfig1 libcairo2 libjpeg-dev libpango-1.0-0 libgif-dev \
    libicu-dev zlib1g-dev libharfbuzz0b libfreetype6-dev \
    && rm -rf /var/lib/apt/lists/*

# Etapa 3: Publicação
FROM build AS publish
RUN dotnet publish "./Pregiato.API.csproj" -c Release -o /publish --no-self-contained

# Copiar pastas adicionais (Templates e Files)
COPY Templates /Templates/
COPY Files/Select.Html.dep /Files/Select.Html.dep

# Etapa 4: Imagem final para runtime
FROM base AS final
WORKDIR /publish 
COPY --from=publish /publish .

# Configurar links simbólicos para dependências
RUN ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll && \
    ln -s /lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/libdl.so

# Definir o comando de entrada
ENTRYPOINT ["dotnet", "Pregiato.API.dll"]