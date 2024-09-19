#!/bin/bash

# usage:
# ./add-migration.sh "MigrationName"

dotnet ef migrations add "FirstMigration" --project "./Common.Domain/Common.Domain.csproj" --startup-project "./Common.WebApi/Common.WebApi.csproj"
dotnet ef database update --project "./Common.Domain/Common.Domain.csproj" --startup-project "./Common.WebApi/Common.WebApi.csproj"

echo "NOTE: Seed data is inserted in DB the first time WebApi is running"
