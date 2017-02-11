#!/bin/sh
export out_dir=/home/travis/build/martincostello/website/artifacts
dotnet restore --verbosity minimal || exit 1
dotnet build --output $out_dir || exit 1
dotnet test tests/Website.Tests/Website.Tests.csproj --output $out_dir || exit 1
dotnet publish src/Website/Website.csproj --output $out_dir || exit 1
