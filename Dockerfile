FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY global.json .
COPY src/UrlShortener.Api/UrlShortener.Api.csproj src/UrlShortener.Api/
RUN dotnet restore src/UrlShortener.Api/UrlShortener.Api.csproj

COPY . .
RUN dotnet publish src/UrlShortener.Api/UrlShortener.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

USER app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "UrlShortener.Api.dll"]
