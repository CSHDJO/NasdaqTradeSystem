$ErrorActionPreference = "Stop"

$OWNER = "xunafay"
$REPO = "NasdaqTradeSystem"
$ARTIFACT_NAME = "build-artifact"
$OUTPUT_DIR = "./Build"
$COMMIT_LOOKAHEAD = 3

if (-not (Get-Command "gh" -ErrorAction SilentlyContinue)) {
    echo "GitHub CLI is not installed"
    exit 1
}

$CURRENT_COMMIT = git rev-parse HEAD
$CURRENT_BRANCH = git rev-parse --abbrev-ref HEAD

if ($CURRENT_BRANCH -ne "master") {
    $BASE_COMMIT = git rev-parse origin/master
} else {
    $BASE_COMMIT = $CURRENT_COMMIT
}

echo "Starting from commit: $BASE_COMMIT"

$COMMITS = git rev-list --reverse --topo-order "$BASE_COMMIT"..origin/master | Select-Object -First $COMMIT_LOOKAHEAD
$COMMITS = @($BASE_COMMIT) + $COMMITS

echo "Querying GitHub API for workflow runs"

foreach ($COMMIT in $COMMITS) {
    echo "Looking for workflow run for commit $COMMIT"

    $RUN_ID = gh run list `
        --repo "$OWNER/$REPO" `
        --limit 100 `
        --json databaseId,headSha,name,status `
        --jq ".[] | select(.headSha == `"$COMMIT`" and .name == `"Build main solution`" and .status == `"completed`") | .databaseId" |
        Select-Object -First 1

    if ($RUN_ID) {
        echo "Found run ID $RUN_ID for $COMMIT"

        $ARTIFACTS_JSON = gh api repos/$OWNER/$REPO/actions/runs/$RUN_ID/artifacts | ConvertFrom-Json
        $ARTIFACT_ID = $ARTIFACTS_JSON.artifacts |
            Where-Object { $_.name -eq $ARTIFACT_NAME } |
            Select-Object -ExpandProperty id -First 1

        if ($ARTIFACT_ID) {
            echo "Found artifact ID $ARTIFACT_ID for $ARTIFACT_NAME"

            if (-not (Test-Path $OUTPUT_DIR)) {
                New-Item -ItemType Directory -Path $OUTPUT_DIR | Out-Null
            }

            Get-ChildItem -Path $OUTPUT_DIR -Force |
                Where-Object { $_.Name -ne "Results" } |
                Remove-Item -Recurse -Force

            gh run download $RUN_ID `
                --repo "$OWNER/$REPO" `
                --name "$ARTIFACT_NAME" `
                --dir $OUTPUT_DIR

            echo "Downloaded artifact to $OUTPUT_DIR"
            echo "Extracted to $OUTPUT_DIR"
            exit 0
        }
    }
}

echo "No artifact found for the last $COMMIT_LOOKAHEAD commits from $BASE_COMMIT"
exit 1

