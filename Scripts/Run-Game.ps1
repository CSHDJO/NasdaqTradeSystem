param ($githubToken)
$solutionPath = Resolve-Path "$PSScriptRoot/../Source/NasdaqTraderSystem.sln";

$startPath = Resolve-Path "$PSScriptRoot";
Set-Location -Path "$PSScriptRoot/../"
Write-Host "fetch and pull latest changes from the repository"
git fetch
git pull
Set-Location -Path $startPath

Write-Host "Removing all files from Build/Bots folder"
$botsBuildPath = Resolve-Path "$PSScriptRoot/../Build/Bots"
if (Test-Path $botsBuildPath) {
    Get-ChildItem -Path $botsBuildPath -Recurse -File | Remove-Item -Force
}

Write-Host "Restore and build the solution"
#restore and build the solution
dotnet restore $solutionPath
dotnet build $solutionPath

Write-Host "Restore and build all bots"
#restore and build all bots
Get-ChildItem -Path "$PSScriptRoot/../Bots" -Recurse -Filter *.csproj | ForEach-Object {
    dotnet restore $_.FullName
    dotnet build $_.FullName
}

Write-Host "Run the game"
$exePath = Resolve-Path "$PSScriptRoot/../Build/NasdaqTrader.CLI.exe";

$dataFolderPath = Resolve-Path "$PSScriptRoot/../Data";
Start-Process -FilePath $exePath -ArgumentList "-d $dataFolderPath -n 100 -t 2500 -silent -s 1000" -NoNewWindow -Wait

Set-Location -Path "$PSScriptRoot/../Build/results"

Write-Host "Push results to the repository"
git add .
git commit -m "Automated commit of build results"
git push https://$githubToken@github.com/CSHDJO/NasdaqResults.git HEAD:main
