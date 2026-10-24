#!/bin/bash
set -euo pipefail
CHUNK_JSON="$1"
MSG_CODE="$2"
MSG_TALLY="$3"
AUDIT_FILE="$4"
# Stage code paths from caller before invoking
COMMIT=$(git rev-parse HEAD)
cd /workspace/csharp
python3 tools/append_tally_chunk.py 0 "$(basename "$CHUNK_JSON")"
python3 - <<PY
import json
from pathlib import Path
p = Path('docs/data/loc_tally.json')
t = json.loads(p.read_text())
t['chunks'][-1]['conversionManagedCommit'] = '$COMMIT'
p.write_text(json.dumps(t, indent=2) + '\n')
PY
python3 tools/generate_native_managed_matrix.py
dotnet build HCDE.sln -warnaserror -v q
dotnet test HCDE.sln -v q --no-build
dotnet test tests/HCDE.MigrationMatrix.Tests/HCDE.MigrationMatrix.Tests.csproj -v q --no-build
CHUNK_NUM="$(basename "$CHUNK_JSON" .json | sed 's/^chunk//')"
REAUDIT_OUT="$(python3 tools/reaudit_chunks.py audit --from 450 --to "$CHUNK_NUM")"
echo "$REAUDIT_OUT"
if ! echo "$REAUDIT_OUT" | grep -qE 'Hard failures: 0'; then
  echo "reaudit: hard failures must be 0 before land (see output above)"
  exit 1
fi
if echo "$REAUDIT_OUT" | grep -q '  warn '; then
  echo "reaudit: soft warnings must be 0 before land (see output above)"
  exit 1
fi
git add docs/data/loc_tally.json docs/data/native_managed_matrix.json docs/HCDE_CSHARP_NATIVE_MANAGED_MATRIX.md "$AUDIT_FILE"
git commit -m "$MSG_TALLY"
