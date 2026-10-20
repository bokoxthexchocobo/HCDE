#!/bin/bash
# Usage: land_chunk_fast.sh <num> "commit message" file1 file2 ...
set -euo pipefail
NUM="$1"
MSG="$2"
shift 2
cd /workspace
git add "$@"
git commit -m "$MSG"
CODE_SHA=$(git rev-parse HEAD)
bash csharp/tools/check_port_coverage.sh "$NUM" "$CODE_SHA"
python3 csharp/tools/fix_chunk_tally.py "$NUM"
cd csharp/tools
python3 sync_loc_tally.py
python3 generate_native_managed_matrix.py
cd /workspace/csharp
dotnet build HCDE.sln -warnaserror -v q
dotnet test HCDE.sln -v q --no-build
dotnet test tests/HCDE.MigrationMatrix.Tests/HCDE.MigrationMatrix.Tests.csproj -v q --no-build
cd /workspace/csharp/tools
python3 reaudit_chunks.py audit --from "$NUM" --to "$NUM"
python3 write_chunk_audit.py "$NUM"
cd /workspace
git add csharp/docs csharp/tools/chunk"${NUM}.json" csharp/docs/data
git commit -m "loc_tally and matrix for chunk ${NUM}"
git push -u origin cursor/native-managed-matrix-3df6
