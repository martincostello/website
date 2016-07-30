#!/bin/sh
dotnet restore --verbosity minimal
dotnet build src/Website
dotnet test tests/Website.Tests
dotnet publish src/Website
