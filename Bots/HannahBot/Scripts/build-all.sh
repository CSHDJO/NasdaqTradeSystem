#!/bin/bash

dotnet restore ../../../Source/NasdaqTraderSystem.sln
dotnet build ../../../Source/NasdaqTraderSystem.sln

find ../../.. -type f -name "*.csproj" | while read csproj; do
    dir=$(dirname "$csproj")
    echo "Processing: $dir"
    pushd "$dir" > /dev/null
    dotnet restore
    dotnet build
    popd > /dev/null
done
