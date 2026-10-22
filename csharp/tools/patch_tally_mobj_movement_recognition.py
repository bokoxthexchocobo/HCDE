#!/usr/bin/env python3
"""Trim p-mobj-movement-thinker-chunk3-recognition when landing movement VM chunks."""
import json
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]

DROP_BY_CHUNK: dict[int, set[tuple[str, str]]] = {
    724: {
        ("src/playsim/p_mobj.cpp", "P_XYMovement"),
        ("src/playsim/p_mobj.cpp", "missile collide explode"),
    },
    725: {("src/playsim/p_mobj.cpp", "P_ZMovement")},
    726: {("src/playsim/p_mobj.cpp", "P_ExplodeMissile")},
    728: {("src/playsim/p_mobj.cpp", "AActor::Tick")},
}


def _trim_chunk460_floor_bounce(tally: dict) -> None:
    for ch in tally["chunks"]:
        if ch.get("name") != "p-mobj-bounce-seek-chunk460":
            continue
        kept = [
            nr
            for nr in ch.get("nativeRanges") or []
            if not (nr.get("file") == "src/playsim/p_mobj.cpp" and nr.get("symbol") == "AActor::FloorBounceMissile")
        ]
        ch["nativeRanges"] = kept
        ch["nativeLoc"] = sum(int(r["end"]) - int(r["start"]) + 1 for r in kept)
        ch["note"] = (ch.get("note") or "") + " Trimmed FloorBounceMissile after chunk726."
        break


def trim_for_chunk(chunk_num: int) -> None:
    tally = json.loads((REPO / "docs/data/loc_tally.json").read_text())
    if chunk_num == 726:
        _trim_chunk460_floor_bounce(tally)
    if chunk_num not in DROP_BY_CHUNK:
        (REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n")
        if chunk_num == 726:
            print(f"patched chunk460 overlap for chunk {chunk_num}")
        return
    drop = DROP_BY_CHUNK[chunk_num]
    for ch in tally["chunks"]:
        if ch.get("name") != "p-mobj-movement-thinker-chunk3-recognition":
            continue
        kept = [
            nr
            for nr in ch.get("nativeRanges") or []
            if (nr.get("file"), nr.get("symbol")) not in drop
        ]
        ch["nativeRanges"] = kept
        ch["nativeLoc"] = sum(int(r["end"]) - int(r["start"]) + 1 for r in kept)
        ch["note"] = (
            (ch.get("note") or "") + f" Trimmed after chunk{chunk_num} VM port."
        ).strip()
        break
    (REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n")
    print(f"patched p-mobj-movement recognition for chunk {chunk_num}")


if __name__ == "__main__":
    import sys

    trim_for_chunk(int(sys.argv[1]))
