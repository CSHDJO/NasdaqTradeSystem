dotnet restore ..\..\..\Source\NasdaqTraderSystem.sln
dotnet build ..\..\..\Source\NasdaqTraderSystem.sln

Get-ChildItem -Path ..\..\ -Recurse -Filter *.csproj | ForEach-Object { Push-Location $_.Directory; dotnet restore; dotnet build; Pop-Location }
