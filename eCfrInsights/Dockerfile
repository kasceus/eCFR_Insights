# =========================
# Build stage (.NET)
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
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

COPY ["eCfrInsights/ecfrInsights.csproj","eCfrInsights/"]
RUN dotnet restore "./eCfrInsights/ecfrInsights.csproj"
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

# Copy published app
COPY --from=publish /app/publish .

COPY ecfrInsights/ecfrInsights.pfx /https/ecfrInsights.pfx

ENTRYPOINT ["dotnet", "ecfrInsights.dll"]