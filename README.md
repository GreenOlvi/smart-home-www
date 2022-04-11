# Smart Home WWW

[![build and test](https://github.com/GreenOlvi/smart-home-www/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/GreenOlvi/smart-home-www/actions/workflows/build-and-test.yml)

Small ASP.NET app to collect data about my home sensors and control some relays

To be run in a docker container. Depends on [main project](https://github.com/GreenOlvi/smart-home).

## Changing DB

From the repository root directory:

 * Install entity framework tools

        dotnet tool install dotnet-ef

 * Create migration

        dotnet ef migrations add <MigrationName> --project .\Server

 * Remove migration

        dotnet ef migrations remove --project .\Server

 * Apply changes

        dotnet ef database update --project .\Server

    Or run `run-db-update.sh` to update running docker container.