#!/bin/bash
# Usage: check_port_coverage.sh <chunkNum> [portSha]
# Exits 1 if per-VM-file managed additions in port commit < 50% of chunk nativeLoc.
set -euo pipefail
NUM="$1"
REPO=/workspace
SHA="${2:-$(git -C "$REPO" rev-parse HEAD)}"
NL=$(python3 -c "
import json, sys
from pathlib import Path
num=sys.argv[1]
chunk=json.loads((Path('$REPO/csharp/tools')/f'chunk{num}.json').read_text())
if isinstance(chunk,list): chunk=chunk[0]
print(sum(int(r['end'])-int(r['start'])+1 for r in chunk['nativeRanges']))
" "$NUM")
ML=$(python3 -c "
import json, sys
from pathlib import Path
sys.path.insert(0, '$REPO/csharp/tools')
from chunk_port_managed import managed_loc_for_chunk
num=int(sys.argv[1])
sha=sys.argv[2]
chunk=json.loads((Path('$REPO/csharp/tools')/f'chunk{num}.json').read_text())
if isinstance(chunk,list): chunk=chunk[0]
print(managed_loc_for_chunk(chunk, sha, num))
" "$NUM" "$SHA")
MIN=$(( (NL + 1) / 2 ))
echo "chunk $NUM native=$NL managed_src=$ML need>=$MIN"
if [ "$ML" -lt "$MIN" ]; then
  echo "thin_managed_coverage: abort before push"
  exit 1
fi
