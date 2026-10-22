#!/usr/bin/env python3
"""Remove overlapping FBehavior::Init credits superseded by chunk730."""
import json
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
INIT_LO, INIT_HI = 2281, 2792


def trim_init_ranges(ranges):
    kept = []
    for nr in ranges or []:
        if nr.get("file", "").endswith("p_acs.cpp") and nr.get("symbol") == "FBehavior::Init":
            if nr["end"] < INIT_LO or nr["start"] > INIT_HI:
                kept.append(nr)
            continue
        kept.append(nr)
    return kept


def patch_chunk_json(num: int):
    path = REPO / "tools" / f"chunk{num}.json"
    if not path.is_file():
        return
    data = json.loads(path.read_text())
    entry = data[0] if isinstance(data, list) else data
    if entry.get("retired"):
        return
    kept = trim_init_ranges(entry.get("nativeRanges"))
    if len(kept) == len(entry.get("nativeRanges") or []):
        return
    if not kept:
        entry["retired"] = True
        entry["nativeRanges"] = []
        entry["managedSourceFiles"] = []
        entry["note"] = (entry.get("note") or "") + " Retired: FBehavior::Init moved to chunk730."
    else:
        entry["nativeRanges"] = kept
        entry["note"] = (entry.get("note") or "") + " Trimmed FBehavior::Init overlap after chunk730."
    path.write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")


def patch_tally():
    tally = json.loads((REPO / "docs/data/loc_tally.json").read_text())
    for ch in tally["chunks"]:
        name = ch.get("name", "")
        if name not in (
            "acs-behavior-var-serialize-chunk591",
            "acs-behavior-init-arrays-imports-chunk593",
        ):
            continue
        kept = trim_init_ranges(ch.get("nativeRanges"))
        ch["nativeRanges"] = kept
        ch["nativeLoc"] = sum(int(r["end"]) - int(r["start"]) + 1 for r in kept)
        if not kept:
            ch["retired"] = True
            ch["nativeLoc"] = 0
            ch["managedLoc"] = 0
            ch["managedSourceFiles"] = []
        ch["note"] = (ch.get("note") or "") + " Trimmed FBehavior::Init after chunk730."
    (REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n", encoding="utf-8")


for n in (591, 593):
    patch_chunk_json(n)
patch_tally()
print("trimmed Init overlap for chunk730")
