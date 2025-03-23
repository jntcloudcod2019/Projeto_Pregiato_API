FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

RUN apt-get update && apt-get install -y chromium \
    libx11-xcb1 libxcomposite1 libxdamage1 libxi6 libxtst6 libnss3 \
    libxrandr2 libasound2 libpangocairo-1.0-0 libatk1.0-0 \
    libatk-bridge2.0-0 libgtk-3-0 libgdiplus libc6-dev libssl-dev \
    libfontconfig1 libcairo2 libjpeg-dev libpango-1.0-0 libgif-dev \
    libicu-dev zlib1g-dev libharfbuzz0b libfreetype6-dev \
    && rm -rf /var/lib/apt/lists/*

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

# Restaurar dependências corretamente
COPY Pregiato.API/Pregiato.API.csproj ./
RUN dotnet restore

# Copiar restante do projeto
COPY Pregiato.API/ ./

# Publicar aplicação
RUN dotnet publish -c Release --no-self-contained -o /app/publish

# Copiar Templates e arquivos adicionais explicitamente (se existirem)
RUN cp -r /app/Templates /app/publish/Templates && \
    cp /app/Files/Select.Html.dep /app/publish/

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y chromium \
    libx11-xcb1 libxcomposite1 libxdamage1 libxi6 libxtst6 libnss3 \
    libxrandr2 libasound2 libpangocairo-1.0-0 libatk1.0-0 \
    libatk-bridge2.0-0 libgtk-3-0 libgdiplus libc6-dev libssl-dev \
    libfontconfig1 libcairo2 libjpeg-dev libpango-1.0-0 libgif-dev \
    libicu-dev zlib1g-dev libharfbuzz0b libfreetype6-dev \
    && rm -rf /var/lib/apt/lists/*

RUN ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll && \
    ln -s /lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/libdl.so

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Pregiato.API.dll"]
