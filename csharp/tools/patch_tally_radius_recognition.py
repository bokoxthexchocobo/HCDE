#!/usr/bin/env python3
"""Retire split radius conversion rows when landing whole P_RadiusAttack chunk722."""
import json
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
tally = json.loads((REPO / "docs/data/loc_tally.json").read_text())

for ch in tally["chunks"]:
    name = ch.get("name", "")
    if name in ("p-map-radius-chunk1", "p-map-radius-thrust-chunk2"):
        ch["retired"] = True
        ch["nativeRanges"] = []
        ch["nativeLoc"] = 0
        ch["managedLoc"] = 0
        ch["note"] = (ch.get("note") or "") + " Retired: absorbed by chunk722 VmPMapRadiusDamage."
    if name == "action-explode-radius-give":
        kept = [
            nr
            for nr in ch.get("nativeRanges") or []
            if not (nr.get("file") == "src/playsim/p_map.cpp" and nr.get("symbol") == "P_RadiusAttack damage loop")
        ]
        ch["nativeRanges"] = kept
        ch["nativeLoc"] = sum(int(r["end"]) - int(r["start"]) + 1 for r in kept)

(REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n")
print("patched radius recognition/conversion overlaps for chunk722")
