#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Server/SmartHomeWWW.Server.csproj", "Server/"]
COPY ["Core/SmartHomeWWW.Core.csproj", "Core/"]
COPY ["Client/SmartHomeWWW.Client.csproj", "Client/"]
RUN dotnet restore "Server/SmartHomeWWW.Server.csproj"
COPY . .
WORKDIR "/src/Server"
RUN dotnet build "SmartHomeWWW.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SmartHomeWWW.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p "/data"
RUN mkdir -p "/firmwares"
ENTRYPOINT ["dotnet", "SmartHomeWWW.Server.dll"]