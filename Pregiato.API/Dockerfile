# Etapa 1: Build da aplicação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar os arquivos do projeto e restaurar dependências
COPY ["Pregiato.API.csproj", "./"]
RUN dotnet restore

# Copiar todo o restante do código e compilar a aplicação
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: Runtime para executar a aplicação
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar os arquivos publicados na etapa anterior
COPY --from=build /app/publish .

# Comando para iniciar a aplicação
ENTRYPOINT ["dotnet", "Pregiato.API.dll"]
