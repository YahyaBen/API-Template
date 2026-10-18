# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . ./
RUN dotnet restore foodiya.app.Identity.Api/foodiya.app.Identity.Api.csproj
RUN dotnet publish foodiya.app.Identity.Api/foodiya.app.Identity.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM build AS migrations
RUN dotnet tool restore
ENTRYPOINT ["dotnet", "tool", "run", "dotnet-ef"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./

RUN mkdir -p /home/app/.aspnet/DataProtection-Keys \
    && chown -R $APP_UID:$APP_UID /home/app/.aspnet

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

USER $APP_UID

HEALTHCHECK --interval=10s --timeout=5s --start-period=30s --retries=5 \
  CMD ["curl", "--fail", "--silent", "http://localhost:8080/health"]

ENTRYPOINT ["dotnet", "foodiya.app.Identity.Api.dll"]
