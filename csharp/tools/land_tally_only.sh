#!/bin/bash
# Append loc_tally for a chunk whose code commit already exists.
set -euo pipefail
NUM="$1"
CODE_SHA="${2:-$(git rev-parse HEAD)}"
cd /workspace/csharp
python3 tools/append_tally_chunk.py 0 "chunk${NUM}.json"
python3 - <<PY
import json
from pathlib import Path
p = Path('docs/data/loc_tally.json')
t = json.loads(p.read_text())
ch = t['chunks'][-1]
key = 'conversionManagedCommit' if ch.get('bucket') == 'conversion' else 'recognitionManagedCommit'
ch[key] = '${CODE_SHA}'
p.write_text(json.dumps(t, indent=2) + '\n')
PY
python3 tools/sync_loc_tally.py
python3 tools/generate_native_managed_matrix.py
dotnet build HCDE.sln -warnaserror -v q
dotnet test HCDE.sln -v q --no-build
dotnet test tests/HCDE.MigrationMatrix.Tests/HCDE.MigrationMatrix.Tests.csproj -v q --no-build
printf '# Chunk %s\n\nTests per loc_tally.json.\n' "$NUM" > "docs/HCDE_CSHARP_CHUNK${NUM}_AUDIT.md"
cd ..
git add csharp/docs/ "csharp/tools/chunk${NUM}.json"
git commit -m "loc_tally and matrix for chunk ${NUM}"
git push -u origin cursor/native-managed-matrix-3df6
