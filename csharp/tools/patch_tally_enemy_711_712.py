#!/usr/bin/env python3
"""Retire overlapping enemy tally rows when landing chunks 711–712."""
import json
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
tally = json.loads((REPO / "docs/data/loc_tally.json").read_text())

for ch in tally["chunks"]:
    name = ch.get("name", "")
    if name == "p-enemy-resurrect-chunk471":
        ch["retired"] = True
        ch["nativeRanges"] = []
        ch["nativeLoc"] = 0
        ch["managedLoc"] = 0
        ch["note"] = (ch.get("note") or "") + " Retired: whole-function credit in chunk711."
    if name == "p-enemy-chunk1":
        kept = []
        for nr in ch.get("nativeRanges") or []:
            if nr.get("symbol") in ("CheckBossDeath", "A_BossDeath"):
                continue
            kept.append(nr)
        ch["nativeRanges"] = kept
        ch["nativeLoc"] = sum(int(r["end"]) - int(r["start"]) + 1 for r in kept)
        ch["note"] = (ch.get("note") or "") + " Trimmed CheckBossDeath/A_BossDeath after chunk712."

(REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n")
print("patched enemy chunk471 retire + p-enemy-chunk1 boss trim")
