FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY Directory.Build.props ./
COPY src/backend/SB.Solicitudes.Domain/SB.Solicitudes.Domain.csproj src/backend/SB.Solicitudes.Domain/
COPY src/backend/SB.Solicitudes.Application/SB.Solicitudes.Application.csproj src/backend/SB.Solicitudes.Application/
COPY src/backend/SB.Solicitudes.Infrastructure/SB.Solicitudes.Infrastructure.csproj src/backend/SB.Solicitudes.Infrastructure/
COPY src/backend/SB.Solicitudes.Services/SB.Solicitudes.Services.csproj src/backend/SB.Solicitudes.Services/
COPY src/backend/SB.Solicitudes.Api/SB.Solicitudes.Api.csproj src/backend/SB.Solicitudes.Api/
RUN dotnet restore src/backend/SB.Solicitudes.Api/SB.Solicitudes.Api.csproj

COPY src/backend/ src/backend/
RUN dotnet publish src/backend/SB.Solicitudes.Api/SB.Solicitudes.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/logs /app/App_Data \
    && chown -R app:app /app
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "SB.Solicitudes.Api.dll"]
