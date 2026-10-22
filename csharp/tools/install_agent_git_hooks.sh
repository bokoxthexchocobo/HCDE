#!/bin/bash
# Install repo git hooks for Cloud Agent land discipline.
set -euo pipefail
REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
mkdir -p "$REPO_ROOT/.git/hooks"
install -m 755 "$REPO_ROOT/csharp/tools/git-hooks/pre-push" "$REPO_ROOT/.git/hooks/pre-push"
echo "Installed pre-push hook -> $REPO_ROOT/.git/hooks/pre-push"
