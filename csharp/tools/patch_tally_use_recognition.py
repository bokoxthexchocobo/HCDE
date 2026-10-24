#!/usr/bin/env python3
"""Trim overlapping use-line credits when landing chunk716/718."""
import json
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
tally = json.loads((REPO / "docs/data/loc_tally.json").read_text())

for ch in tally["chunks"]:
    name = ch.get("name", "")
    if name == "p-map-try-use-lineattack-chunk3-recognition":
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
    if name == "p-map-use-fail-noway":
        ch["retired"] = True
        ch["nativeRanges"] = []
        ch["nativeLoc"] = 0
        ch["managedLoc"] = 0
        ch["note"] = (ch.get("note") or "") + " Retired: P_NoWayTraverse/P_UseLines in chunk716/718."
    if name == "p-map-offset-talk-puzzle-chunk492":
        kept = []
        for nr in ch.get("nativeRanges") or []:
            if nr.get("file") == "src/playsim/p_map.cpp" and nr.get("symbol") == "P_NoWayTraverse":
                continue
            kept.append(nr)
        ch["nativeRanges"] = kept
        ch["nativeLoc"] = sum(int(r["end"]) - int(r["start"]) + 1 for r in kept)
        ch["note"] = (ch.get("note") or "") + " Trimmed P_NoWayTraverse slice after chunk716."
        for nr in ch.get("nativeRanges") or []:
            if nr.get("symbol") == "P_UsePuzzleItem":
                pass
        kept2 = [
            nr
            for nr in ch.get("nativeRanges") or []
            if not (nr.get("file") == "src/playsim/p_map.cpp" and nr.get("symbol") == "P_UsePuzzleItem")
        ]
        if kept2 != ch.get("nativeRanges"):
            ch["nativeRanges"] = kept2
            ch["nativeLoc"] = sum(int(r["end"]) - int(r["start"]) + 1 for r in kept2)
            ch["note"] = (ch.get("note") or "") + " Trimmed P_UsePuzzleItem after chunk718."

(REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n")
print("trimmed use-line overlaps for chunk716/718")
