#!/usr/bin/env python3
"""Merge sub-150 nativeLoc chunks into adjacent whole-function targets (no padding)."""
from __future__ import annotations

import json
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
TOOLS = REPO / "tools"
TALLY = REPO / "docs/data/loc_tally.json"

# Post-shrink merges only; does not modify auditPaddingReview line maps.
# Idempotent: only merges not already retired on disk.
MERGE_PLAN: list[tuple[int, list[int]]] = [
    (455, [454, 457]),
    (469, [459]),
    (460, [466, 467]),
    (474, [458, 468, 473]),
    (477, [465, 480]),
    (481, [476]),
    (509, [510]),
    (591, [586]),
]


def load(num: int) -> dict:
    data = json.loads((TOOLS / f"chunk{num}.json").read_text(encoding="utf-8"))
    return data[0] if isinstance(data, list) else data


def save(num: int, chunk: dict) -> None:
    (TOOLS / f"chunk{num}.json").write_text(json.dumps([chunk], indent=2) + "\n", encoding="utf-8")


def retire(num: int, into_name: str) -> None:
    chunk = load(num)
    chunk["retired"] = True
    chunk["nativeRanges"] = []
    chunk["note"] = f"Merged into {into_name} (coverage shrink pass)."
    save(num, chunk)


def merge_into(target: int, sources: list[int]) -> None:
    if not sources:
        return
    tgt = load(target)
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
        if not src.get("retired"):
            retire(src_num, into_name)
    tgt["managedSourceFiles"] = files
    tgt["nativeRanges"] = ranges
    tgt["note"] = (tgt.get("note", "") + f" Merged: {sources}.").strip()
    save(target, tgt)


def sync_tally(tally: dict) -> None:
    by_name = {c["name"]: c for c in tally["chunks"]}
    for num in range(450, 700):
        path = TOOLS / f"chunk{num}.json"
        if not path.is_file():
            continue
        ch = load(num)
        name = ch["name"]
        if name not in by_name:
            continue
        row = by_name[name]
        if ch.get("retired"):
            row["retired"] = True
            row["nativeRanges"] = []
            row["nativeLoc"] = 0
            row["note"] = ch.get("note", row.get("note", ""))
            continue
        row.pop("retired", None)
        row["nativeRanges"] = ch.get("nativeRanges", [])
        row["managedSourceFiles"] = ch.get("managedSourceFiles", [])
        row["nativeLoc"] = sum(int(r["end"]) - int(r["start"]) + 1 for r in row["nativeRanges"])


def main() -> int:
    plan = [(t, s) for t, s in MERGE_PLAN if s]
    for target, sources in plan:
        merge_into(target, sources)
        print(f"merged {sources} -> chunk{target}")
    tally = json.loads(TALLY.read_text(encoding="utf-8"))
    sync_tally(tally)
    TALLY.write_text(json.dumps(tally, indent=2) + "\n", encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
