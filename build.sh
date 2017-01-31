#!/bin/sh
dotnet restore src/Website/Website.csproj --verbosity minimal || exit 1
dotnet restore tests/Website.Tests/Website.Tests.csproj --verbosity minimal || exit 1
dotnet build  src/Website/Website.csproj || exit 1
dotnet test tests/Website.Tests/Website.Tests.csproj || exit 1
dotnet publish src/Website/Website.csproj || exit 1
