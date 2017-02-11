#!/bin/sh
export artifacts=$(dirname "$(readlink -f "$0")")/artifacts

dotnet restore --verbosity minimal || exit 1
dotnet build --output $artifacts || exit 1
dotnet test tests/Website.Tests/Website.Tests.csproj --output $artifacts || exit 1
dotnet publish src/Website/Website.csproj --output $artifacts || exit 1
