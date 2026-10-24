#!/usr/bin/env python3
"""Retire native slices on p-spec-sector-damage-chunk442 when chunk808 lands (same sync)."""
import json
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
path = REPO / "docs/data/loc_tally.json"
tally = json.loads(path.read_text(encoding="utf-8"))

CHUNK808 = [
    ("src/playsim/p_spec.cpp", 509, 531),
    ("src/playsim/p_spec.cpp", 533, 585),
    ("src/playsim/p_spec.cpp", 596, 627),
    ("src/playsim/p_spec.cpp", 629, 637),
    ("src/playsim/p_spec.cpp", 645, 677),
]


def overlaps(a_start, a_end, b_start, b_end):
    return a_start <= b_end and b_start <= a_end


def subtract_interval(s, e, b_start, b_end):
    if not overlaps(s, e, b_start, b_end):
        return [(s, e)]
    out = []
    if s < b_start:
        out.append((s, b_start - 1))
    if e > b_end:
        out.append((b_end + 1, e))
    return out


def trim_range(r):
    file = r["file"]
    s, e = int(r["start"]), int(r["end"])
    intervals = [(s, e)]
    for cf, bs, be in CHUNK808:
        if file != cf:
            continue
        next_iv = []
        for a, b in intervals:
            next_iv.extend(subtract_interval(a, b, bs, be))
        intervals = next_iv
    return intervals


for ch in tally["chunks"]:
    if ch["name"] != "p-spec-sector-damage-chunk442":
        continue
    new_ranges = []
    for r in ch.get("nativeRanges", []):
        for s, e in trim_range(r):
            if s > e:
                continue
            nr = dict(r)
            nr["start"] = s
            nr["end"] = e
            new_ranges.append(nr)
    ch["nativeRanges"] = new_ranges
    ch["nativeLoc"] = sum(int(r["end"]) - int(r["start"]) + 1 for r in new_ranges)
    ch["note"] = (
        "showsecretsector cvars only; DoSectorDamage / P_SectorDamage / secrets / terrain flat retired to chunk808."
    )
    break

path.write_text(json.dumps(tally, indent=2) + "\n", encoding="utf-8")
print("patch_tally_chunk808_overlap: trimmed p-spec-sector-damage-chunk442")
