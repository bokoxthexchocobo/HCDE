#!/bin/bash
# Usage: land_chunk_fast.sh <num> "commit message" file1 file2 ...
#        land_chunk_fast.sh --resume <num>
#        land_chunk_fast.sh consolidate "message" file1 file2 ...  (VM test consolidation; no tally row)
# Sole supported path to git push in this repo.
set -euo pipefail
RESUME=0
if [ "${1:-}" = "--resume" ]; then
  RESUME=1
  shift
fi
NUM="$1"
if [ "$NUM" = "consolidate" ]; then
  MSG="$2"
  shift 2
  cd /workspace
  git add "$@"
  git commit -m "$MSG"
  bash /workspace/csharp/tools/verify_full_solution_tests.sh
  HCDE_ALLOW_PUSH=1 git push -u origin "$(git branch --show-current)"
  exit 0
fi
if [ "$RESUME" = 1 ]; then
  shift
  cd /workspace
  if ! [[ "$NUM" =~ ^[0-9]+$ ]]; then
    echo "land_chunk_fast --resume: expected chunk number, got '$NUM'" >&2
    exit 1
  fi
  if [ -n "${1:-}" ]; then
    echo "land_chunk_fast --resume: no extra arguments (port must already be committed)" >&2
    exit 1
  fi
else
  MSG="$2"
  shift 2
  cd /workspace
fi
python3 -c "
import json, sys
n = int(sys.argv[1])
data = json.load(open(f'csharp/tools/chunk{n}.json'))
meta = data[0] if isinstance(data, list) else data
if not (meta.get('auditPaddingReview') or '').strip():
    raise SystemExit(f'chunk {n}: missing auditPaddingReview in chunk{n}.json')
" "$NUM"
python3 csharp/tools/uncredited_range_check.py --chunk "$NUM" || {
  echo "land_chunk_fast: uncredited_range_check failed — target lines overlap existing tally credit"
  exit 1
}
if [ "$RESUME" = 1 ]; then
  if ! git cat-file -e "HEAD:csharp/tools/chunk${NUM}.json" 2>/dev/null; then
    echo "land_chunk_fast --resume: chunk${NUM}.json not in git history" >&2
    exit 1
  fi
  CODE_SHA=$(git log -1 --format=%H -- "csharp/tools/chunk${NUM}.json")
  if [ -z "$CODE_SHA" ]; then
    echo "land_chunk_fast --resume: no commit found for csharp/tools/chunk${NUM}.json" >&2
    exit 1
  fi
  echo "land_chunk_fast --resume: using port commit ${CODE_SHA} (from chunk${NUM}.json history)"
else
  git add "$@"
  git commit -m "$MSG"
  CODE_SHA=$(git rev-parse HEAD)
fi
bash csharp/tools/check_port_coverage.sh "$NUM" "$CODE_SHA"
python3 csharp/tools/fix_chunk_tally.py "$NUM" "$CODE_SHA"
HEAD_SHA=$(git rev-parse HEAD)
while IFS= read -r peer_json; do
  [ -z "$peer_json" ] && continue
  peer_num="${peer_json#csharp/tools/chunk}"
  peer_num="${peer_num%.json}"
  if [ "$peer_num" != "$NUM" ]; then
    echo "land_chunk_fast: refresh tally row for chunk ${peer_num} (chunk JSON changed since port)"
    python3 csharp/tools/fix_chunk_tally.py "$peer_num" "$HEAD_SHA"
  fi
done < <(git diff --name-only "$CODE_SHA" "$HEAD_SHA" -- 'csharp/tools/chunk*.json' || true)
if [ "$NUM" = "814" ]; then
  echo "land_chunk_fast: refresh chunk 481 managedSourceFiles (VmPMapLineTrace exclusivity for 814)"
  python3 csharp/tools/fix_chunk_tally.py 481 "$HEAD_SHA"
fi
cd csharp/tools
python3 sync_loc_tally.py
python3 generate_native_managed_matrix.py
cd /workspace/csharp/tools
REAUDIT_OUT=$(python3 reaudit_chunks.py audit --from 450 --to "$NUM")
echo "$REAUDIT_OUT"
echo "$REAUDIT_OUT" | grep -q "missing_audit_padding_review" && {
  echo "reaudit: missing_audit_padding_review — add auditPaddingReview to chunk${NUM}.json"
  exit 1
}
if ! echo "$REAUDIT_OUT" | grep -qE 'Hard failures: 0'; then
  echo "reaudit: hard failures must be 0 before land (see output above)"
  exit 1
fi
if echo "$REAUDIT_OUT" | grep -q '  warn '; then
  echo "reaudit: soft warnings must be 0 before land (see output above)"
  exit 1
fi
python3 write_chunk_audit.py "$NUM"
cd /workspace
git add csharp/docs csharp/tools/chunk"${NUM}.json" csharp/docs/data
git commit -m "loc_tally and matrix for chunk ${NUM}"
TALLY_SHA=$(git rev-parse HEAD)
echo "land_chunk_fast: hardened verify at tally commit ${TALLY_SHA}"
bash /workspace/csharp/tools/verify_full_solution_tests.sh
HCDE_ALLOW_PUSH=1 git push -u origin "$(git branch --show-current)"
