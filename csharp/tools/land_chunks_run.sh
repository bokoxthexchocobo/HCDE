#!/bin/bash
set -euo pipefail
cd /workspace
land() {
  local num="$1"
  local msg="$2"
  local audit="csharp/docs/HCDE_CSHARP_CHUNK${num}_AUDIT.md"
  printf '# Chunk %s\n\nTests per `loc_tally.json`.\n\nVerified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.\n' "$num" > "$audit"
  COMMIT=$(git rev-parse HEAD)
  cd csharp
  python3 tools/append_tally_chunk.py 0 "chunk${num}.json"
  python3 - <<PY
import json
from pathlib import Path
p = Path('docs/data/loc_tally.json')
t = json.loads(p.read_text())
ch = t['chunks'][-1]
if ch.get('bucket') == 'conversion':
    ch['conversionManagedCommit'] = '$COMMIT'
elif ch.get('bucket') == 'recognition':
    ch['recognitionManagedCommit'] = '$COMMIT'
p.write_text(json.dumps(t, indent=2) + '\n')
PY
  python3 tools/generate_native_managed_matrix.py
  cd ..
  git add csharp/docs/data/loc_tally.json csharp/docs/data/native_managed_matrix.json csharp/docs/HCDE_CSHARP_NATIVE_MANAGED_MATRIX.md "$audit" "csharp/tools/chunk${num}.json"
  git commit -m "$msg"
  git push -u origin cursor/native-managed-matrix-3df6
}
