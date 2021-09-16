#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["SmartHomeWWW/SmartHomeWWW.csproj", "SmartHomeWWW/"]
RUN dotnet restore "SmartHomeWWW/SmartHomeWWW.csproj"
COPY . .
WORKDIR "/src/SmartHomeWWW"
RUN dotnet build "SmartHomeWWW.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SmartHomeWWW.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY raspi3.local.pfx /app/raspi3.local.pfx
ENTRYPOINT ["dotnet", "SmartHomeWWW.dll"]