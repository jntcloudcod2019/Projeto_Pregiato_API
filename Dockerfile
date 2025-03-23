# Etapa 1: Construção da aplicação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Instalar dependências do Chromium e outras bibliotecas
RUN apt-get update && apt-get install -y \
    chromium \
    libx11-xcb1 libxcomposite1 libxdamage1 libxi6 libxtst6 libnss3 \
    libxrandr2 libasound2 libpangocairo-1.0-0 libatk1.0-0 \
    libatk-bridge2.0-0 libgtk-3-0 libgdiplus libc6-dev libssl-dev \
    libfontconfig1 libcairo2 libjpeg-dev libpango-1.0-0 libgif-dev \
    libicu-dev zlib1g-dev libharfbuzz0b libfreetype6-dev \
    && rm -rf /var/lib/apt/lists/*

# Definir variáveis de ambiente (somente para build, se necessário)
ARG SERVER_EMAIL
ARG SERVER_EMAIL_PORT
ARG SECRET_KEY_DATABASE
ARG SERVER_EMAIL_USERNAME
ARG SERVER_EMAIL_PASSWORD
ARG SECRETKEY_JWT_TOKEN
ARG ISSUER_JWT
ARG AUDIENCE_JWT

ENV SERVER_EMAIL=${SERVER_EMAIL}
ENV SERVER_EMAIL_PORT=${SERVER_EMAIL_PORT}
ENV SECRET_KEY_DATABASE=${SECRET_KEY_DATABASE}
ENV SERVER_EMAIL_USERNAME=${SERVER_EMAIL_USERNAME}
ENV SERVER_EMAIL_PASSWORD=${SERVER_EMAIL_PASSWORD}
ENV SECRETKEY_JWT_TOKEN=${SECRETKEY_JWT_TOKEN}
ENV ISSUER_JWT=${ISSUER_JWT}
ENV AUDIENCE_JWT=${AUDIENCE_JWT}

# Copiar e restaurar dependências
COPY Pregiato.API/Pregiato.API.csproj Pregiato.API/
WORKDIR /app/Pregiato.API
RUN dotnet restore "Pregiato.API.csproj"

# Copiar o restante do projeto
COPY Pregiato.API/ .

# Publicar a aplicação
RUN dotnet publish "Pregiato.API.csproj" -c Release --no-self-contained -o /app/publish

# Copiar arquivos adicionais (se existirem)
COPY Pregiato.API/Templates /app/publish/Templates/
COPY Pregiato.API/Files/Select.Html.dep /app/publish/Files/Select.Html.dep

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Instalar dependências do Chromium e outras bibliotecas
RUN apt-get update && apt-get install -y \
    chromium \
    libx11-xcb1 libxcomposite1 libxdamage1 libxi6 libxtst6 libnss3 \
    libxrandr2 libasound2 libpangocairo-1.0-0 libatk1.0-0 \
    libatk-bridge2.0-0 libgtk-3-0 libgdiplus libc6-dev libssl-dev \
    libfontconfig1 libcairo2 libjpeg-dev libpango-1.0-0 libgif-dev \
    libicu-dev zlib1g-dev libharfbuzz0b libfreetype6-dev \
    && rm -rf /var/lib/apt/lists/*

# Criar links simbólicos necessários
RUN ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll && \
    ln -s /lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/libdl.so

# Copiar os arquivos publicados
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Pregiato.API.dll"]