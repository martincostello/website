@echo off
dotnet restore src/Website/project.json tests/Website.Tests/project.json --verbosity minimal
dotnet build src/Website
dotnet test tests/Website.Tests
dotnet publish src/Website
