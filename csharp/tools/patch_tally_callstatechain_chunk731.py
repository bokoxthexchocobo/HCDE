#!/usr/bin/env python3
"""Ensure chunk731 tally row matches re-cut native ranges (drops 87-790 oversize)."""
import json
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
chunk = json.loads((REPO / "tools" / "chunk731.json").read_text())
if isinstance(chunk, list):
    chunk = chunk[0]
tally = json.loads((REPO / "docs/data/loc_tally.json").read_text())
for ch in tally["chunks"]:
    if ch.get("name") == chunk["name"]:
        ch["nativeRanges"] = chunk["nativeRanges"]
        ch["nativeLoc"] = sum(int(r["end"]) - int(r["start"]) + 1 for r in chunk["nativeRanges"])
        for k in ("note", "auditRationale", "auditSeams", "auditPaddingReview", "managedSourceFiles"):
            if k in chunk:
                ch[k] = chunk[k]
        break
(REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n", encoding="utf-8")
print("synced chunk731 tally row from chunk731.json")
