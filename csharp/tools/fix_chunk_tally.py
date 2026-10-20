#!/usr/bin/env python3
"""Set nativeLoc, managedLoc, conversionManagedCommit on the last tally chunk from chunkN.json + HEAD."""
import json
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
num = int(sys.argv[1])
sha = subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=REPO.parent, text=True).strip()
chunk = json.loads((REPO / "tools" / f"chunk{num}.json").read_text())
if isinstance(chunk, list):
    chunk = chunk[0]
ml = 0
out = subprocess.check_output(["git", "show", sha, "--numstat", "--format="], cwd=REPO.parent, text=True)
for line in out.splitlines():
    parts = line.split("\t")
    if len(parts) != 3:
        continue
    add_s, _del, path = parts
    if path.startswith("csharp/src/") and path.endswith(".cs"):
        ml += int(add_s)
nl = sum(int(r["end"]) - int(r["start"]) + 1 for r in chunk["nativeRanges"])
tally = json.loads((REPO / "docs/data/loc_tally.json").read_text())
for ch in tally["chunks"]:
    if ch["name"] == chunk["name"]:
        ch["nativeRanges"] = chunk["nativeRanges"]
        ch["nativeLoc"] = nl
        ch["managedLoc"] = ml
        ch["conversionManagedCommit"] = sha
        for k in ("note", "auditRationale", "auditSeams"):
            if k in chunk:
                ch[k] = chunk[k]
        break
else:
    chunk["nativeLoc"] = nl
    chunk["managedLoc"] = ml
    chunk["conversionManagedCommit"] = sha
    tally["chunks"].append(chunk)
(REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n")
print(f"{chunk['name']} native={nl} managed_commit={ml} sha={sha[:8]}")
