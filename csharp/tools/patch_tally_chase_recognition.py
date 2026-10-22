#!/usr/bin/env python3
"""Retire overlapping recognition rows when landing chase/move VM chunks."""
import json
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
tally = json.loads((REPO / "docs/data/loc_tally.json").read_text())

for ch in tally["chunks"]:
    name = ch.get("name", "")
    if name in ("p-enemy-cantleave-chase-rollback", "p-enemy-move-blocked-chunk2"):
        ch["retired"] = True
        ch["nativeRanges"] = []
        ch["nativeLoc"] = 0
        ch["managedLoc"] = 0
        ch["note"] = (ch.get("note") or "") + " Retired: absorbed by chunk702/703 VM ports."
    if name == "p-enemy-chase-core-chunk3-recognition":
        kept = []
        for nr in ch.get("nativeRanges") or []:
            sym = nr.get("symbol")
            if nr.get("file") == "src/playsim/p_enemy.cpp" and sym in (
                "A_DoChase",
                "P_Move",
                "P_SmartMove",
                "P_TryWalk",
                "P_DoNewChaseDir",
                "P_NewChaseDir",
                "P_RandomChaseDir",
                "P_CheckMeleeRange",
                "P_CheckMissileRange",
                "P_HitFriend",
                "P_IsVisible",
                "isTargetablePlayer",
                "ValidEnemyInBlock",
                "P_LookForPlayers",
            ):
                continue
            kept.append(nr)
        ch["nativeRanges"] = kept
        ch["nativeLoc"] = sum(int(r["end"]) - int(r["start"]) + 1 for r in kept)

(REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n")
print("patched recognition overlaps for A_DoChase and P_Move")
