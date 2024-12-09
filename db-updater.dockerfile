FROM mcr.microsoft.com/dotnet/sdk:9.0.101 AS build

WORKDIR /src
COPY .config .config
RUN dotnet tool restore

COPY ["Server/SmartHomeWWW.Server.csproj", "Server/"]
RUN dotnet restore "Server/SmartHomeWWW.Server.csproj"
COPY . .

WORKDIR /src
