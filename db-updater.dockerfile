FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

WORKDIR /src
COPY .config .config
RUN dotnet tool restore

COPY ["SmartHomeWWW/SmartHomeWWW.csproj", "SmartHomeWWW/"]
RUN dotnet restore "SmartHomeWWW/SmartHomeWWW.csproj"
COPY . .

WORKDIR /src
ENTRYPOINT dotnet ef database update --project SmartHomeWWW
