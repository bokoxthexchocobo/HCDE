#!/usr/bin/env python3
"""Merge sub-150 nativeLoc chunks into targets without exceeding 400 native lines."""
from __future__ import annotations

import json
from pathlib import Path

TOOLS = Path(__file__).resolve().parent
TALLY = TOOLS.parent / "docs/data/loc_tally.json"
FLOOR = 150
CEILING = 400

# (target, [sources]) — sources must be <150; result target+sources in [150, 400].
PLAN: list[tuple[int, list[int]]] = [
    (601, [542]),
]


def load(num: int) -> dict:
    data = json.loads((TOOLS / f"chunk{num}.json").read_text(encoding="utf-8"))
    return data[0] if isinstance(data, list) else data


def save(num: int, chunk: dict) -> None:
    (TOOLS / f"chunk{num}.json").write_text(json.dumps([chunk], indent=2) + "\n", encoding="utf-8")


def native_loc(chunk: dict) -> int:
    return sum(int(r["end"]) - int(r["start"]) + 1 for r in chunk.get("nativeRanges") or [])


def retire(num: int, into_name: str) -> None:
    ch = load(num)
    ch["retired"] = True
    ch["nativeRanges"] = []
    ch["managedSourceFiles"] = []
    ch["note"] = f"Merged into {into_name} (sub-150 depad merge)."
    save(num, ch)


def merge_into(target: int, sources: list[int]) -> None:
    tgt = load(target)
    if tgt.get("retired"):
        raise SystemExit(f"target chunk{target} is retired")
    base = native_loc(tgt)
    src_loc = 0
    for s in sources:
        src = load(s)
        if src.get("retired"):
            continue
        src_loc += native_loc(src)
    total = base + src_loc
    if total > CEILING:
        raise SystemExit(f"chunk{target} would be {total} native lines (> {CEILING})")
    into_name = tgt.get("name", f"chunk{target}")
    files = list(tgt.get("managedSourceFiles") or [])
    ranges = list(tgt.get("nativeRanges") or [])
    for src_num in sources:
        src = load(src_num)
        if src.get("retired"):
            continue
        for f in src.get("managedSourceFiles") or []:
            if f not in files:
                files.append(f)
        ranges.extend(src.get("nativeRanges") or [])
        retire(src_num, into_name)
    tgt["managedSourceFiles"] = files
    tgt["nativeRanges"] = ranges
    tgt["note"] = (tgt.get("note", "") + f" Merged: {sources}.").strip()
    save(target, tgt)
    print(f"merged {sources} -> chunk{target} (nativeLoc~{total})")


def sync_tally() -> None:
    import merge_chunks as mc

    tally = json.loads(TALLY.read_text(encoding="utf-8"))
    mc.sync_tally(tally)
    TALLY.write_text(json.dumps(tally, indent=2) + "\n", encoding="utf-8")


def main() -> int:
    for target, sources in PLAN:
        merge_into(target, sources)
    sync_tally()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
