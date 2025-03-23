# Etapa 1: Construção da aplicação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Definir argumentos de construção
ARG SERVER_EMAIL
ARG SERVER_EMAIL_PORT
ARG SECRET_KEY_DATABASE
ARG SERVER_EMAIL_USERNAME
ARG SERVER_EMAIL_PASSWORD
ARG SECRETKEY_JWT_TOKEN
ARG ISSUER_JWT
ARG AUDIENCE_JWT

# Converter ARG em ENV
ENV SERVER_EMAIL=${SERVER_EMAIL}
ENV SERVER_EMAIL_PORT=${SERVER_EMAIL_PORT}
ENV SECRET_KEY_DATABASE=${SECRET_KEY_DATABASE}
ENV SERVER_EMAIL_USERNAME=${SERVER_EMAIL_USERNAME}
ENV SERVER_EMAIL_PASSWORD=${SERVER_EMAIL_PASSWORD}
ENV SECRETKEY_JWT_TOKEN=${SECRETKEY_JWT_TOKEN}
ENV ISSUER_JWT=${ISSUER_JWT}
ENV AUDIENCE_JWT=${AUDIENCE_JWT}

# Restaurar dependências
COPY Pregiato.API/Pregiato.API.csproj ./
RUN dotnet restore

# Copiar o restante do código fonte
COPY Pregiato.API/ ./

# Publicar a aplicação
RUN dotnet publish -c Release --no-self-contained -o /app/publish

# Copiar explicitamente a pasta Templates e arquivo Select.Html.dep necessários para SelectPdf
RUN cp -r /app/Templates /app/publish/Templates && \
    cp /app/Files/Select.Html.dep /app/publish/

# Etapa 2: Execução da aplicação
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Instalar dependências necessárias completas (bibliotecas gráficas, pdf, SSL, criptografia, PuppeteerSharp)
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
    libgdiplus \
    libc6-dev \
    libssl-dev \
    libfontconfig1 \
    libcairo2 \
    libjpeg-dev \
    libpango-1.0-0 \
    libgif-dev \
    libicu-dev \
    zlib1g-dev \
    libharfbuzz0b \
    libfreetype6-dev \
    && rm -rf /var/lib/apt/lists/*

# Links simbólicos necessários para o correto funcionamento de libgdiplus e libdl
RUN ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll && \
    ln -s /lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/libdl.so

# Copiar arquivos publicados (incluindo Templates e arquivos SelectPdf necessários)
COPY --from=build /app/publish .

# Expor a porta e iniciar a aplicação
EXPOSE 8080
ENTRYPOINT ["dotnet", "Pregiato.API.dll"]
