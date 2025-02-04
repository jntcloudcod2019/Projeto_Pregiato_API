# Usando a imagem base do .NET SDK para compilar e publicar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiando os arquivos do projeto e restaurando dependências
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /out

# Criando a imagem final baseada no ASP.NET Core Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

# Definir variáveis de ambiente para o runtime
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Development

# Expor a porta 5000
EXPOSE 5000

# Comando para iniciar a API
ENTRYPOINT ["dotnet", "Pregiato.API.dll"]
