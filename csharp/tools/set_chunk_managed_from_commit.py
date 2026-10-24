#!/usr/bin/env python3
"""Set managedLoc on a tally chunk from csharp/src additions in a specific port commit (optionally one file)."""
import json
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
chunk_name = sys.argv[1]
sha = sys.argv[2]
src_filter = sys.argv[3] if len(sys.argv) > 3 else None
out = subprocess.check_output(["git", "show", sha, "--numstat", "--format="], cwd=REPO.parent, text=True)
ml = 0
for line in out.splitlines():
    parts = line.split("\t")
    if len(parts) != 3:
        continue
    add_s, _del, path = parts
    if not path.startswith("csharp/src/") or not path.endswith(".cs"):
        continue
    if src_filter and not path.endswith(src_filter):
        continue
    ml += int(add_s)
tally = json.loads((REPO / "docs/data/loc_tally.json").read_text())
for ch in tally["chunks"]:
    if ch.get("name") == chunk_name:
        ch["managedLoc"] = ml
        ch["conversionManagedCommit"] = sha
        break
else:
    raise SystemExit(f"chunk not found: {chunk_name}")
(REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n")
print(f"{chunk_name} managedLoc={ml} commit={sha[:8]}")
