#!/usr/bin/env bash
set -e

OWNER="xunafay"
REPO="NasdaqTradeSystem"
BRANCH="master"
ARTIFACT_NAME="build-artifact"
OUTPUT_DIR="./Build"
COMMIT_LOOKAHEAD=3 # GitHub allows up to 60 unauthenticated requests/hour per IP, so we limit to 3 commits

if ! command -v gh >/dev/null 2>&1; then
  echo "GitHub CLI is not installed"
  exit 1
fi

CURRENT_COMMIT=$(git rev-parse HEAD)
CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)

if [ "$CURRENT_BRANCH" != "$BRANCH" ]; then
  BASE_COMMIT=$(git rev-parse origin/$BRANCH)
else
  BASE_COMMIT=$CURRENT_COMMIT
fi

echo "Starting from commit: $BASE_COMMIT"

COMMITS=$(git rev-list --reverse --topo-order "$BASE_COMMIT"..origin/$BRANCH | head -n "$COMMIT_LOOKAHEAD")
COMMITS="$BASE_COMMIT"$'\n'"$COMMITS"

echo "Querying GitHub API for workflow runs"
RUNS_JSON=$(curl -s "https://api.github.com/repos/$OWNER/$REPO/actions/runs?per_page=100")

for COMMIT in $COMMITS; do
  echo "Looking for workflow run for commit $COMMIT"

  RUN_ID=$(gh run list \
    --repo "$OWNER/$REPO" \
    --limit 100 \
    --json databaseId,headSha,name,status \
    --jq ".[] | select(.headSha == \"$COMMIT\" and .name == \"Build main solution\" and .status == \"completed\") | .databaseId" | head -n 1)


  if [ -n "$RUN_ID" ]; then
    echo "Found run ID $RUN_ID for $COMMIT"

    ARTIFACTS_JSON=$(curl -s "https://api.github.com/repos/$OWNER/$REPO/actions/runs/$RUN_ID/artifacts")
    ARTIFACT_ID=$(echo "$ARTIFACTS_JSON" | jq -r \
      --arg NAME "$ARTIFACT_NAME" \
      '.artifacts[] | select(.name == $NAME) | .id' | head -n 1)

    if [ -n "$ARTIFACT_ID" ]; then
      echo "Found artifact ID $ARTIFACT_ID for $ARTIFACT_NAME"

      mkdir -p "$OUTPUT_DIR"

	  find "$OUTPUT_DIR" -mindepth 1 -not -name "Results" -exec rm -rf {} +

      gh run download "$RUN_ID" \
        --repo "$OWNER/$REPO" \
        --name "$ARTIFACT_NAME" \
        --dir "$OUTPUT_DIR"

      echo "Downloaded artifact to $OUTPUT_DIR"
      echo "Extracted to $OUTPUT_DIR"
      exit 0
    fi
  fi
done

echo "No artifact found for the last $COMMIT_LOOKAHEAD commits from $BASE_COMMIT"
exit 1

