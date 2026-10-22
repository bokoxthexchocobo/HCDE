#!/usr/bin/env python3
"""Retire overlapping poison credits when landing chunk732."""
import json
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
tally = json.loads((REPO / "docs/data/loc_tally.json").read_text())

for ch in tally["chunks"]:
    name = ch.get("name", "")
    if name == "p-mobj-poison-kill-sector-chunk57":
        kept = [r for r in ch.get("nativeRanges") or [] if r.get("symbol") != "P_PoisonMobj"]
        ch["nativeRanges"] = kept
        ch["nativeLoc"] = sum(int(r["end"]) - int(r["start"]) + 1 for r in kept)
        ch["note"] = (ch.get("note") or "") + " Trimmed P_PoisonMobj after chunk732."
    if name == "p-interaction-poison-switch-chunk462":
        ch["retired"] = True
        ch["nativeRanges"] = []
        ch["nativeLoc"] = 0
        ch["managedLoc"] = 0
        ch["managedSourceFiles"] = []
        ch["note"] = (ch.get("note") or "") + " Retired: poison bands moved to chunk732; OkayToSwitch pending re-chunk."
    if name == "p-interaction-damage-late-chunk474":
        kept = [r for r in ch.get("nativeRanges") or [] if not (r.get("symbol") == "DamageMobj" and r["start"] == 1715)]
        ch["nativeRanges"] = kept
        ch["nativeLoc"] = sum(int(r["end"]) - int(r["start"]) + 1 for r in kept)

(REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n")
print("patched poison overlaps for chunk732")
