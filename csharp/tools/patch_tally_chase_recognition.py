#!/usr/bin/env python3
"""Retire overlapping recognition rows when landing chase/move VM chunks."""
import json
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
tally = json.loads((REPO / "docs/data/loc_tally.json").read_text())

for ch in tally["chunks"]:
    name = ch.get("name", "")
    if name == "p-enemy-cantleave-chase-rollback":
        ch["retired"] = True
        ch["nativeRanges"] = []
        ch["nativeLoc"] = 0
        ch["managedLoc"] = 0
    if name == "p-enemy-chase-core-chunk3-recognition":
        kept = []
        for nr in ch.get("nativeRanges") or []:
            if nr.get("file") == "src/playsim/p_enemy.cpp" and nr.get("symbol") in ("A_DoChase", "P_Move"):
                continue
            kept.append(nr)
        ch["nativeRanges"] = kept
        ch["nativeLoc"] = sum(int(r["end"]) - int(r["start"]) + 1 for r in kept)

(REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n")
print("patched recognition overlaps for A_DoChase and P_Move")
