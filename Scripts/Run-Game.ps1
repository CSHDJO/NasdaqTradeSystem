param ($githubToken)
$solutionPath = Resolve-Path "$PSScriptRoot/../Source/NasdaqTraderSystem.sln";

#restore and build the solution
dotnet restore $solutionPath
dotnet build $solutionPath

#restore and build all bots
Get-ChildItem -Path "$PSScriptRoot/../Bots" -Recurse -Filter *.csproj | ForEach-Object {
    dotnet restore $_.FullName
    dotnet build $_.FullName
}

$exePath = Resolve-Path "$PSScriptRoot/../Build/NasdaqTrader.CLI.exe";

$dataFolderPath = Resolve-Path "$PSScriptRoot/../Data";
Start-Process -FilePath $exePath -ArgumentList "-t 10000 -d $dataFolderPath -n 100 -t 10000 -silent -s 100000" -NoNewWindow -Wait

Set-Location -Path "$PSScriptRoot/../Build/results"

git add .
git commit -m "Automated commit of build results"
git push https://$githubToken@github.com/CSHDJO/NasdaqResults.git HEAD:main
