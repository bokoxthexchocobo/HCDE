#!/bin/bash
# Usage: land_one_chunk.sh <num> "code message" file1 file2 ...
set -euo pipefail
NUM="$1"
MSG="$2"
shift 2
cd /workspace
if [ "$#" -gt 0 ]; then
  git add "$@"
  git commit -m "$MSG"
fi
CODE_SHA=$(git rev-parse HEAD)
SRC_FILES=$(git show HEAD --name-only --format= | grep '^csharp/src/.*\.cs$' || true)
SRC_COUNT=$(echo "$SRC_FILES" | grep -c . || echo 0)
if [ "$SRC_COUNT" -gt 1 ]; then
  echo "warn: port commit touches $SRC_COUNT csharp/src files; prefer one Vm per chunk"
fi
cd csharp
python3 tools/reaudit_chunks.py audit --from "$NUM" --to "$NUM" 2>/dev/null || true
python3 tools/append_tally_chunk.py 0 "chunk${NUM}.json"
python3 - <<PY
import json
from pathlib import Path
p = Path('docs/data/loc_tally.json')
t = json.loads(p.read_text())
ch = t['chunks'][-1]
ch['conversionManagedCommit'] = '${CODE_SHA}'  # full 40-char from git rev-parse HEAD
# managedLoc from source file if single Vm*.cs in commit
p.write_text(json.dumps(t, indent=2) + '\n')
PY
python3 tools/sync_loc_tally.py
python3 tools/generate_native_managed_matrix.py
dotnet build HCDE.sln -warnaserror -v q
dotnet test HCDE.sln -v q --no-build
dotnet test tests/HCDE.MigrationMatrix.Tests/HCDE.MigrationMatrix.Tests.csproj -v q --no-build
python3 tools/reaudit_chunks.py audit --from "$NUM" --to "$NUM"
python3 tools/write_chunk_audit.py "$NUM"
cd ..
git add csharp/docs/ "csharp/tools/chunk${NUM}.json"
git commit -m "loc_tally and matrix for chunk ${NUM}"
git push -u origin cursor/native-managed-matrix-3df6
