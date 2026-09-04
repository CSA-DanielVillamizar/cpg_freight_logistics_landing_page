# ---- build ----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0-noble AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props global.json ./
COPY CPG.sln ./
COPY src/CPG.Domain/CPG.Domain.csproj src/CPG.Domain/
COPY src/CPG.Application/CPG.Application.csproj src/CPG.Application/
COPY src/CPG.Infrastructure/CPG.Infrastructure.csproj src/CPG.Infrastructure/
COPY src/CPG.Api/CPG.Api.csproj src/CPG.Api/
RUN dotnet restore src/CPG.Api/CPG.Api.csproj

COPY src/ src/
RUN dotnet publish src/CPG.Api/CPG.Api.csproj -c Release -o /app --no-restore /p:UseAppHost=false

# ---- runtime -------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble-chiseled AS runtime
WORKDIR /app

# Chiseled image runs as a non-root user and listens on 8080 by default.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app ./
ENTRYPOINT ["dotnet", "CPG.Api.dll"]
