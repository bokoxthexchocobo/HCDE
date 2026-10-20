#!/bin/bash
set -euo pipefail
NUM="$1"
MSG="$2"
shift 2
cd /workspace
if [ "$#" -gt 0 ]; then
  git add "$@"
  git commit -m "$MSG"
else
  # Tally-only chunk: reuse current tree (empty commit) so tally gets a distinct SHA.
  git commit --allow-empty -m "$MSG"
fi
COMMIT=$(git rev-parse HEAD)
if [ "${#COMMIT}" -ne 40 ]; then
  echo "bad commit length: $COMMIT" >&2
  exit 1
fi
cd csharp
python3 tools/append_tally_chunk.py 0 "chunk${NUM}.json"
python3 - <<PY
import json
from pathlib import Path
p = Path('docs/data/loc_tally.json')
t = json.loads(p.read_text())
ch = t['chunks'][-1]
k = 'conversionManagedCommit' if ch.get('bucket') == 'conversion' else 'recognitionManagedCommit'
ch[k] = '${COMMIT}'
p.write_text(json.dumps(t, indent=2) + '\n')
PY
python3 tools/generate_native_managed_matrix.py
dotnet build HCDE.sln -warnaserror -v q
dotnet test HCDE.sln -v q --no-build
dotnet test tests/HCDE.MigrationMatrix.Tests/HCDE.MigrationMatrix.Tests.csproj -v q --no-build
printf '# Chunk %s\n\nTests per loc_tally.json.\n' "$NUM" > "docs/HCDE_CSHARP_CHUNK${NUM}_AUDIT.md"
cd ..
git add csharp/docs/ "csharp/tools/chunk${NUM}.json"
git commit -m "loc_tally and matrix for chunk ${NUM}"
git push -u origin cursor/native-managed-matrix-3df6
