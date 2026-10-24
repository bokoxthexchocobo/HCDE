#!/bin/bash
# Issue one-time .git/hcde_land_token and push (called only from land_chunk_fast.sh).
set -euo pipefail
REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
BRANCH="${1:?branch required}"
HOOKS_DIR="$(cd "$(dirname "$0")/../../.git/hooks" && pwd)"
HEAD_SHA="$(git -C "$REPO_ROOT" rev-parse HEAD)"
TOKEN_FILE="${REPO_ROOT}/.git/hcde_land_token"
printf '%s\n' "$HEAD_SHA" > "$TOKEN_FILE"
chmod 600 "$TOKEN_FILE"
git -C "$REPO_ROOT" -c core.hooksPath="$HOOKS_DIR" push -u origin "$BRANCH"
