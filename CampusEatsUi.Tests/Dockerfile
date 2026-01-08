FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CampusEatsUi.Tests/CampusEatsUi.Tests.csproj", "CampusEatsUi.Tests/"]
COPY ["CampusEatsUI/CampusEatsUI.csproj", "CampusEatsUI/"]
RUN dotnet restore "CampusEatsUi.Tests/CampusEatsUi.Tests.csproj"
COPY . .
WORKDIR "/src/CampusEatsUi.Tests"
RUN dotnet build "./CampusEatsUi.Tests.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CampusEatsUi.Tests.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CampusEatsUi.Tests.dll"]
