#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Server/Application.Server.API/Application.Server.API.csproj", "Server/Application.Server.API/"]
RUN dotnet restore "Server/Application.Server.API/Application.Server.API.csproj"
COPY . .
WORKDIR "/src/Server/Application.Server.API"
RUN dotnet build "Application.Server.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Application.Server.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Application.Server.API.dll"]