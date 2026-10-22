#!/usr/bin/env python3
"""Trim overlapping DamageMobj bands from chunk474 when landing chunk729."""
import json
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(REPO / "tools"))
from chunk_port_managed import managed_loc_for_chunk  # noqa: E402
TRIM_LO, TRIM_HI = 1281, 1571


def trim_ranges(ranges):
    kept = []
    for nr in ranges:
        if nr.get("symbol") != "DamageMobj":
            kept.append(nr)
            continue
        if nr["end"] < TRIM_LO or nr["start"] > TRIM_HI:
            kept.append(nr)
    return kept


def patch_chunk(path: Path):
    data = json.loads(path.read_text())
    entry = data[0] if isinstance(data, list) else data
    entry["retired"] = True
    entry["nativeRanges"] = []
    entry["nativeLoc"] = 0
    entry["managedSourceFiles"] = []
    entry["note"] = (entry.get("note") or "") + " Retired: credit moved to chunk729."
    path.write_text(json.dumps(data, indent=2) + "\n")


def patch_tally():
    tally = json.loads((REPO / "docs/data/loc_tally.json").read_text())
    for ch in tally["chunks"]:
        if ch.get("name") != "p-interaction-damage-late-chunk474":
            continue
        ch["retired"] = True
        ch["nativeRanges"] = []
        ch["nativeLoc"] = 0
        ch["managedLoc"] = 0
        ch["managedSourceFiles"] = []
        ch["note"] = (ch.get("note") or "") + " Retired: DamageMobj + wrapper bands moved to chunk729."
    (REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n")


patch_chunk(REPO / "tools" / "chunk474.json")
patch_tally()
print("trimmed p-interaction-damage-late-chunk474 DamageMobj 1281-1571")
