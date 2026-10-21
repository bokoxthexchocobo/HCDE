#!/usr/bin/env python3
"""Set nativeLoc, managedLoc, conversionManagedCommit on the last tally chunk from chunkN.json + HEAD."""
import json
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(REPO / "tools"))
from chunk_port_managed import managed_loc_for_chunk  # noqa: E402

num = int(sys.argv[1])
sha = subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=REPO.parent, text=True).strip()
head_src = subprocess.check_output(
    ["git", "diff-tree", "--no-commit-id", "--name-only", "-r", sha],
    cwd=REPO.parent,
    text=True,
).splitlines()
head_touches_managed_src = any(
    p.startswith("csharp/src/") and p.endswith(".cs") for p in head_src
)
chunk = json.loads((REPO / "tools" / f"chunk{num}.json").read_text())
if isinstance(chunk, list):
    chunk = chunk[0]
ml = managed_loc_for_chunk(chunk, sha, num)
nl = sum(int(r["end"]) - int(r["start"]) + 1 for r in chunk["nativeRanges"])
tally = json.loads((REPO / "docs/data/loc_tally.json").read_text())
for ch in tally["chunks"]:
    if ch["name"] == chunk["name"]:
        ch["nativeRanges"] = chunk["nativeRanges"]
        ch["nativeLoc"] = nl
        ch["managedLoc"] = ml
        if head_touches_managed_src:
            ch["conversionManagedCommit"] = sha
        for k in ("note", "auditRationale", "auditSeams", "auditPaddingReview", "managedSourceFiles"):
            if k in chunk:
                ch[k] = chunk[k]
        break
else:
    chunk["nativeLoc"] = nl
    chunk["managedLoc"] = ml
    if head_touches_managed_src:
        chunk["conversionManagedCommit"] = sha
    tally["chunks"].append(chunk)
(REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n")
print(f"{chunk['name']} native={nl} managed_commit={ml} sha={sha[:8]}")
