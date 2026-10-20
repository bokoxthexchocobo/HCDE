#!/bin/bash
# Usage: check_port_coverage.sh <chunkNum>
# Requires HEAD to be the port commit; exits 1 if managed src additions < 50% nativeLoc.
set -euo pipefail
NUM="$1"
REPO=/workspace
NL=$(python3 -c "
import json
from pathlib import Path
chunk=json.loads((Path('$REPO/csharp/tools')/f'chunk{NUM}.json').read_text())
if isinstance(chunk,list): chunk=chunk[0]
print(sum(int(r['end'])-int(r['start'])+1 for r in chunk['nativeRanges']))
")
ML=$(git -C "$REPO" show HEAD --numstat --format= | awk '/^([0-9]+|-)\t([0-9]+|-)\tcsharp\/src\/.*\.cs$/ { if ($1 ~ /^[0-9]+$/) s+=$1 } END { print s+0 }')
MIN=$(( (NL + 1) / 2 ))
echo "chunk $NUM native=$NL managed_src=$ML need>=$MIN"
if [ "$ML" -lt "$MIN" ]; then
  echo "thin_managed_coverage: abort before push"
  exit 1
fi
