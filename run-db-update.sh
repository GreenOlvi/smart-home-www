#!/usr/bin/bash

docker build -t db-updater -f db-updater.dockerfile .
docker run --rm -it -v smart-home_www-data:/data db-updater dotnet ef database update --project SmartHomeWWW
