#!/bin/bash
# Install repo git hooks for Cloud Agent land discipline.
# Honors `core.hooksPath` when set (Cloud Agent VMs use ~/.cursor/agent-hooks/...).
set -euo pipefail
REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
SRC="$REPO_ROOT/csharp/tools/git-hooks/pre-push"
mkdir -p "$REPO_ROOT/.git/hooks"
install -m 755 "$SRC" "$REPO_ROOT/.git/hooks/pre-push"
echo "Installed pre-push hook -> $REPO_ROOT/.git/hooks/pre-push"
HOOKS_PATH="$(git -C "$REPO_ROOT" config --get core.hooksPath || true)"
if [ -n "$HOOKS_PATH" ]; then
  case "$HOOKS_PATH" in
    /*) TARGET_DIR="$HOOKS_PATH" ;;
    *) TARGET_DIR="$REPO_ROOT/$HOOKS_PATH" ;;
  esac
  mkdir -p "$TARGET_DIR"
  install -m 755 "$SRC" "$TARGET_DIR/pre-push"
  echo "Installed pre-push hook -> $TARGET_DIR/pre-push (core.hooksPath)"
fi
