FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution and project files for restore
COPY echo.sln .
COPY Bootstrapper/Api/Api.csproj Bootstrapper/Api/
COPY Shared/Shared.csproj Shared/
COPY Shared.Contracts/Shared.Contracts.csproj Shared.Contracts/
COPY Modules/Identity/Identity/Identity.csproj Modules/Identity/Identity/

RUN dotnet restore echo.sln

# Copy everything and build
COPY . .
WORKDIR /src/Bootstrapper/Api
RUN dotnet build Api.csproj -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish Api.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
