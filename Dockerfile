FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia o projeto e restaura as dependências
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /out


FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .


ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production


EXPOSE 8080

ENTRYPOINT ["dotnet", "Pregiato.API.dll"]