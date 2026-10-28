#!/bin/bash
# Prove pre-push rejects manual push even when HCDE_ALLOW_PUSH=1 (scratch / CI safe).
set -euo pipefail
REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
HOOK="${REPO_ROOT}/.git/hooks/pre-push"
TOKEN_FILE="${REPO_ROOT}/.git/hcde_land_token"
HEAD_SHA="$(git -C "$REPO_ROOT" rev-parse HEAD)"
BRANCH="$(git -C "$REPO_ROOT" branch --show-current)"
REMOTE_SHA="$(git -C "$REPO_ROOT" rev-parse "origin/${BRANCH}" 2>/dev/null || echo 0000000000000000000000000000000000000000)"

rm -f "$TOKEN_FILE"
export HCDE_ALLOW_PUSH=1
set +e
OUT="$(printf 'refs/heads/%s %s refs/heads/%s %s\n' "$BRANCH" "$HEAD_SHA" "$BRANCH" "$REMOTE_SHA" | bash "$HOOK" 2>&1)"
RC=$?
set -e
if [[ "$RC" -eq 0 ]]; then
  echo "proof_land_push_token: expected hook to fail without token" >&2
  exit 1
fi
if ! grep -q 'missing one-time land token' <<<"$OUT"; then
  echo "proof_land_push_token: unexpected output: $OUT" >&2
  exit 1
fi

printf '0000000000000000000000000000000000000001\n' > "$TOKEN_FILE"
set +e
OUT2="$(printf 'refs/heads/%s %s refs/heads/%s %s\n' "$BRANCH" "$HEAD_SHA" "$BRANCH" "$REMOTE_SHA" | bash "$HOOK" 2>&1)"
RC2=$?
set -e
rm -f "$TOKEN_FILE"
if [[ "$RC2" -eq 0 ]]; then
  echo "proof_land_push_token: expected hook to fail on bad token" >&2
  exit 1
fi
if ! grep -q 'land token HEAD mismatch\|invalid land token\|does not match land token' <<<"$OUT2"; then
  echo "proof_land_push_token: unexpected output for bad token: $OUT2" >&2
  exit 1
fi

echo "proof_land_push_token: OK (no-token and bad-token blocked with HCDE_ALLOW_PUSH=1)"
