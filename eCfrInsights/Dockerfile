# =========================
# Build stage (.NET)
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

# Expose HTTP and HTTPS ports
EXPOSE 80
EXPOSE 443

ENV ASPNETCORE_ENVIRONMENT=Development \
    ASPNETCORE_URLS=http://+:8080;https://+:8443 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["eCfrInsights/ecfrInsights.csproj", "eCfrInsights/"]
COPY ["eCfrInsights/ecfrinsights.db", "eCfrInsights/"]
COPY ["eCfrInsights/ecfrinsights.db-shm", "eCfrInsights/"]
COPY ["eCfrInsights/ecfrinsights.db-wal", "eCfrInsights/"]

RUN dotnet restore "./eCfrInsights/ecfrInsights.csproj"

# Copy the entire project and build
COPY . .
WORKDIR "/src/eCfrInsights"
RUN dotnet build "./ecfrInsights.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ecfrInsights.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false



# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY ["eCfrInsights/ecfrinsights.db", "/app"]
COPY ["eCfrInsights/ecfrinsights.db-shm", "/app"]
COPY ["eCfrInsights/ecfrinsights.db-wal", "/app"]
# Copy published app
COPY --from=publish /app/publish .

# Ensure the certificate file exists
COPY eCfrInsights/ecfrInsights.pfx /https/ecfrInsights.pfx
# Copy the developer cert into the container
COPY devcert.crt /usr/local/share/ca-certificates/devcert.crt

# Update trust store
RUN update-ca-certificates


ENTRYPOINT ["dotnet", "ecfrInsights.dll"]