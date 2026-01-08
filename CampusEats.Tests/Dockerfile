FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CampusEats.Tests/CampusEats.Tests.csproj", "CampusEats.Tests/"]
COPY ["CampusEats/CampusEats.csproj", "CampusEats/"]
RUN dotnet restore "CampusEats.Tests/CampusEats.Tests.csproj"
COPY . .
WORKDIR "/src/CampusEats.Tests"
RUN dotnet build "./CampusEats.Tests.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CampusEats.Tests.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CampusEats.Tests.dll"]
