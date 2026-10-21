#!/bin/bash
# Land one chunk when full HCDE.sln is unavailable; runs Net.Core build/tests + reaudit.
set -euo pipefail
NUM="$1"
MSG="$2"
shift 2
cd /workspace
python3 -c "
import json, sys
n = int(sys.argv[1])
data = json.load(open(f'csharp/tools/chunk{n}.json'))
meta = data[0] if isinstance(data, list) else data
if not (meta.get('auditPaddingReview') or '').strip():
    raise SystemExit(f'chunk {n}: missing auditPaddingReview')
" "$NUM"
git add "$@"
git commit -m "$MSG"
CODE_SHA=$(git rev-parse HEAD)
bash csharp/tools/check_port_coverage.sh "$NUM" "$CODE_SHA"
python3 csharp/tools/fix_chunk_tally.py "$NUM"
cd csharp/tools
python3 sync_loc_tally.py
python3 generate_native_managed_matrix.py
cd /workspace/csharp
dotnet build src/HCDE.Net.Core/HCDE.Net.Core.csproj -warnaserror -v q
dotnet test tests/HCDE.Net.Core.Tests/HCDE.Net.Core.Tests.csproj -v q --filter "FullyQualifiedName~VmINet"
cd /workspace/csharp/tools
REAUDIT_OUT=$(python3 reaudit_chunks.py audit --from "$NUM" --to "$NUM")
echo "$REAUDIT_OUT"
echo "$REAUDIT_OUT" | grep -q "missing_audit_padding_review" && exit 1
if echo "$REAUDIT_OUT" | grep -q "  warn "; then exit 1; fi
python3 write_chunk_audit.py "$NUM"
cd /workspace
git add csharp/docs csharp/tools/chunk"${NUM}.json" csharp/docs/data
git commit -m "loc_tally and matrix for chunk ${NUM}"
git push -u origin cursor/native-managed-matrix-3df6
