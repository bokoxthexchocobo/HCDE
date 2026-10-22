#!/usr/bin/env python3
"""Retire overlapping recognition rows when landing p_map / look VM chunks."""
import json
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
tally = json.loads((REPO / "docs/data/loc_tally.json").read_text())

for ch in tally["chunks"]:
    name = ch.get("name", "")
    if name == "p-map-try-use-lineattack-chunk3-recognition":
        kept = []
        for nr in ch.get("nativeRanges") or []:
            if nr.get("file") == "src/playsim/p_map.cpp" and nr.get("symbol") in (
                "P_CheckPosition",
                "FSlide::SlideMove",
            ):
                continue
            kept.append(nr)
        ch["nativeRanges"] = kept
        ch["nativeLoc"] = sum(int(r["end"]) - int(r["start"]) + 1 for r in kept)
        ch["note"] = (ch.get("note") or "") + " Trimmed P_CheckPosition/SlideMove after chunk705/706."
    if name == "p-enemy-look-targets-chunk503":
        ch["retired"] = True
        ch["nativeRanges"] = []
        ch["nativeLoc"] = 0
        ch["managedLoc"] = 0
        ch["note"] = (ch.get("note") or "") + " Retired: whole-function credit in chunk707."

(REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n")
print("patched map recognition + retired chunk503 tally row")
