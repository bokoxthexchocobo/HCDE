#!/usr/bin/env python3
"""Retire chunk462 OkayToSwitch credit and trim chunk473 overlap for chunk733."""
import json
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
TRIM_SYMBOLS = {
    ("DoDamageMobj", 1573, 1603),
    ("P_DamageMobj", 1617, 1657),
    ("AActor::OkayToSwitchTarget", 1734, 1801),
    ("CallOkayToSwitchTarget", 1810, 1821),
}


def trim_ranges(ranges):
    kept = []
    for nr in ranges or []:
        key = (nr.get("symbol"), int(nr["start"]), int(nr["end"]))
        if key in TRIM_SYMBOLS:
            continue
        if nr.get("file", "").endswith("p_interaction.cpp") and nr.get("symbol") == "AActor::OkayToSwitchTarget":
            continue
        kept.append(nr)
    return kept


tally = json.loads((REPO / "docs/data/loc_tally.json").read_text())
for ch in tally["chunks"]:
    name = ch.get("name", "")
    if name == "p-interaction-poison-switch-chunk462":
        ch["retired"] = True
        ch["nativeRanges"] = []
        ch["nativeLoc"] = 0
        ch["managedLoc"] = 0
        ch["managedSourceFiles"] = []
        ch["note"] = (ch.get("note") or "") + " Retired: OkayToSwitch moved to chunk733."
    if name == "p-interaction-damage-reflection-chunk473":
        kept = trim_ranges(ch.get("nativeRanges"))
        ch["nativeRanges"] = kept
        ch["nativeLoc"] = sum(int(r["end"]) - int(r["start"]) + 1 for r in kept)
        ch["note"] = (ch.get("note") or "") + " Trimmed damage dispatch overlap after chunk733."
(REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n", encoding="utf-8")
print("patched chunk462/473 for chunk733")
