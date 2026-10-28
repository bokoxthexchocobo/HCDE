#!/usr/bin/env python3
"""Drop p_lnspec ceiling/plat bands migrated to chunk432 from legacy tally row."""
import json
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
tally = json.loads((REPO / "docs/data/loc_tally.json").read_text())
MIGRATED = [
    (836, 841),
    (872, 877),
    (884, 1066),
]
legacy_name = "line-specials-plat-generic-elevator"

def overlaps(s: int, e: int) -> bool:
    for a, b in MIGRATED:
        if s <= b and e >= a:
            return True
    return False

for ch in tally["chunks"]:
    if ch["name"] != legacy_name:
        continue
    kept = []
    for r in ch["nativeRanges"]:
        if r.get("file") == "src/playsim/p_lnspec.cpp" and overlaps(int(r["start"]), int(r["end"])):
            continue
        kept.append(r)
    old_native = ch.get("nativeLoc") or sum(
        int(r["end"]) - int(r["start"]) + 1 for r in ch.get("nativeRanges", [])
    )
    old_managed = int(ch.get("managedLoc") or 0)
    ch["nativeRanges"] = kept
    new_native = sum(int(r["end"]) - int(r["start"]) + 1 for r in kept)
    ch["nativeLoc"] = new_native
    if old_native > 0 and old_managed > 0:
        ch["managedLoc"] = max(1, round(old_managed * new_native / old_native))
    break

(REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n")
print("patched legacy plat row")
