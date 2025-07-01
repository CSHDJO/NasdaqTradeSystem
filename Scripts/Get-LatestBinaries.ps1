$ArtifactName = "build-artifact"
$OutputDir = "Build"
$CommitLookahead = 3 # GitHub allows up to 60 unauthenticated requests/hour per IP, so we limit to 3 commits

$CurrentCommit = git rev-parse HEAD
$CurrentBranch = git rev-parse --abbrev-ref HEAD

if ($CurrentBranch -ne "master") {
    $BaseCommit = git rev-parse origin/master
} else {
    $BaseCommit = $CurrentCommit
}
Write-Host "Starting from commit: $BaseCommit"

# Get up to $CommitLookahead commits forward from base
$Commits = git rev-list --reverse --topo-order "$BaseCommit..origin/master" | Select-Object -First $CommitLookahead
$Commits = @($BaseCommit) + $Commits

Write-Host "Querying GitHub API for workflow runs..."
$RunsUrl = "https://api.github.com/repos/$Owner/$Repo/actions/runs?per_page=100"
$RunsResponse = Invoke-WebRequest -Uri $RunsUrl -UseBasicParsing
$RunsJson = $RunsResponse.Content | ConvertFrom-Json

foreach ($Commit in $Commits) {
    Write-Host "Looking for workflow run for commit $Commit"

    $Run = $RunsJson.workflow_runs | Where-Object { $_.head_sha -eq $Commit -and $_.status -eq "completed" } | Select-Object -First 1

    if ($Run) {
        $RunId = $Run.id
        Write-Host "Found run ID $RunId for $Commit"

        $ArtifactsUrl = "https://api.github.com/repos/$Owner/$Repo/actions/runs/$RunId/artifacts"
        $ArtifactsResponse = Invoke-WebRequest -Uri $ArtifactsUrl -UseBasicParsing
        $ArtifactsJson = $ArtifactsResponse.Content | ConvertFrom-Json

        $Artifact = $ArtifactsJson.artifacts | Where-Object { $_.name -eq $ArtifactName } | Select-Object -First 1

        if ($Artifact) {
            $ArtifactId = $Artifact.id
            Write-Host "Found artifact ID $ArtifactId for $ArtifactName"

            if (-Not (Test-Path $OutputDir)) {
                New-Item -ItemType Directory -Path $OutputDir | Out-Null
            }

            $ZipUrl = "https://api.github.com/repos/$Owner/$Repo/actions/artifacts/$ArtifactId/zip"
            $ZipPath = "$OutputDir\artifact.zip"

            Invoke-WebRequest -Uri $ZipUrl -OutFile $ZipPath -UseBasicParsing -Headers @{ "Accept" = "application/vnd.github.v3+json" }

            Write-Host "Downloaded artifact to $ZipPath"
            Expand-Archive -Path $ZipPath -DestinationPath $OutputDir -Force
            Write-Host "Extracted to $OutputDir"
            exit 0
        }
    }
}

Write-Host "No artifact found for the last $CommitLookahead commits from $BaseCommit"
exit 1
