#!/usr/bin/env bash
set -e

OWNER="xunafay"
REPO="NasdaqTradeSystem"
ARTIFACT_NAME="build-artifact"
OUTPUT_DIR="./Build"
COMMIT_LOOKAHEAD=3 # GitHub allows up to 60 unauthenticated requests/hour per IP, so we limit to 3 commits

CURRENT_COMMIT=$(git rev-parse HEAD)
CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)

if [ "$CURRENT_BRANCH" != "master" ]; then
  BASE_COMMIT=$(git rev-parse origin/master)
else
  BASE_COMMIT=$CURRENT_COMMIT
fi

echo "Starting from commit: $BASE_COMMIT"

COMMITS=$(git rev-list --reverse --topo-order "$BASE_COMMIT"..origin/master | head -n "$COMMIT_LOOKAHEAD")
COMMITS="$BASE_COMMIT"$'\n'"$COMMITS"

echo "Querying GitHub API for workflow runs"
RUNS_JSON=$(curl -s "https://api.github.com/repos/$OWNER/$REPO/actions/runs?per_page=100")

for COMMIT in $COMMITS; do
  echo "Looking for workflow run for commit $COMMIT"

  RUN_ID=$(echo "$RUNS_JSON" | grep -B10 "\"head_sha\": \"$COMMIT\"" | grep '"id":' | head -n1 | sed 's/[^0-9]*\([0-9]*\).*/\1/')

  if [ -n "$RUN_ID" ]; then
    echo "Found run ID $RUN_ID for $COMMIT"
    
    ARTIFACTS=$(curl -s "https://api.github.com/repos/$OWNER/$REPO/actions/runs/$RUN_ID/artifacts")
    ARTIFACT_ID=$(echo "$ARTIFACTS_JSON" | grep -A3 "\"name\": \"$ARTIFACT_NAME\"" | grep '"id":' | head -n1 | sed 's/[^0-9]*\([0-9]*\).*/\1/')

	if [ -n "$ARTIFACT_ID" ]; then
      echo "Found artifact ID $ARTIFACT_ID for $ARTIFACT_NAME"
      
      mkdir -p "$OUTPUT_DIR"
      curl -L "https://api.github.com/repos/$OWNER/$REPO/actions/artifacts/$ARTIFACT_ID/zip" \
        -H "Accept: application/vnd.github.v3+json" \
        -o "$OUTPUT_DIR/artifact.zip"

      echo "Downloaded artifact to $OUTPUT_DIR/artifact.zip"
      unzip -o "$OUTPUT_DIR/artifact.zip" -d "$OUTPUT_DIR"
      echo "Extracted to $OUTPUT_DIR"
      exit 0
    fi
  fi
done

echo "No artifact found for the last $COMMIT_LOOKAHEAD commits from $BASE_COMMIT"
exit 1

