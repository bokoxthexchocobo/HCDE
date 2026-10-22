#!/usr/bin/env python3
"""Trim P_UseTraverse recognition after chunk716 conversion."""
import json
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
tally = json.loads((REPO / "docs/data/loc_tally.json").read_text())

for ch in tally["chunks"]:
    if ch.get("name") == "p-map-try-use-lineattack-chunk3-recognition":
        kept = [
            nr
            for nr in ch.get("nativeRanges") or []
            if not (
                nr.get("file") == "src/playsim/p_map.cpp"
                and nr.get("symbol") == "P_UseTraverse"
            )
        ]
        ch["nativeRanges"] = kept
        ch["nativeLoc"] = sum(int(r["end"]) - int(r["start"]) + 1 for r in kept)
        ch["note"] = (ch.get("note") or "") + " Trimmed P_UseTraverse after chunk716."

(REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n")
print("trimmed P_UseTraverse recognition overlap")
