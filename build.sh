#!/bin/sh
dotnet restore --verbosity minimal || exit 1
dotnet build src/Website/Website.csproj || exit 1
dotnet test tests/Website.Tests/Website.Tests.csproj || exit 1
dotnet publish src/Website/Website.csproj || exit 1
