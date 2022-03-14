#FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
FROM sdk6.0-fixed AS build

WORKDIR /src
COPY .config .config
RUN dotnet tool restore

COPY ["Server/SmartHomeWWW.Server.csproj", "Server/"]
RUN dotnet restore "Server/SmartHomeWWW.Server.csproj"
COPY . .

WORKDIR /src
