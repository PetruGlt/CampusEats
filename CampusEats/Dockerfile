#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["CampusEats.csproj", "CampusEats/"]
RUN dotnet restore "CampusEats/CampusEats.csproj"
COPY . .
WORKDIR "/src/CampusEats"
RUN dotnet build "CampusEats.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CampusEats.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CampusEats.dll"]
