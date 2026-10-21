#!/usr/bin/env python3
"""Refresh chunk managedLoc from surviving csharp/src lines at HEAD (see chunk_port_managed.py)."""
import json
import re
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1].parent
CSHARP = REPO / "csharp"

sys.path.insert(0, str(CSHARP / "tools"))
from chunk_port_managed import (  # noqa: E402
    latest_commit_touching_files,
    managed_loc_commit_additions_sum,
    managed_loc_surviving_at_head,
    chunk_def_path,
)


def chunk_number(name: str) -> int | None:
    m = re.search(r"chunk(\d+)(b)?", name)
    return int(m.group(1)) if m else None


def load_chunk_def(num: int | None, ch: dict) -> dict | None:
    if num is None:
        return None
    path = chunk_def_path(num)
    if not path.is_file():
        return None
    data = json.loads(path.read_text())
    if isinstance(data, list):
        data = data[0]
    merged = {**data, **ch}
    if data.get("portCommits"):
        merged["portCommits"] = data["portCommits"]
    if data.get("managedSourceFiles"):
        merged["managedSourceFiles"] = data["managedSourceFiles"]
    return merged


def remeasure_chunk(ch: dict) -> tuple[int, bool]:
    num = chunk_number(ch.get("name", ""))
    chunk_def = load_chunk_def(num, ch)
    if chunk_def is None:
        chunk_def = {}
    files = list(chunk_def.get("managedSourceFiles") or ch.get("managedSourceFiles") or [])
    if not files:
        return int(ch.get("managedLoc", 0)), False
    chunk_def = {**chunk_def, "managedSourceFiles": files}
    return managed_loc_surviving_at_head(chunk_def, num), True


def main() -> int:
    tally_path = CSHARP / "docs/data/loc_tally.json"
    tally = json.loads(tally_path.read_text())
    nums = [int(x) for x in sys.argv[1:]] if len(sys.argv) > 1 else []

    updated = 0
    for ch in tally["chunks"]:
        if ch.get("bucket", "conversion") != "conversion":
            continue
        if int(ch.get("nativeLoc", 0)) <= 0:
            continue
        num = chunk_number(ch.get("name", ""))
        if nums and (num is None or num not in nums):
            continue
        chunk_def = load_chunk_def(num, ch) or {}
        files = list(chunk_def.get("managedSourceFiles") or ch.get("managedSourceFiles") or [])
        if not files:
            continue
        chunk_def = {**chunk_def, "managedSourceFiles": files}
        ml = managed_loc_surviving_at_head(chunk_def, num)
        old = int(ch.get("managedLoc", 0))
        ch["managedLoc"] = ml
        fresh = latest_commit_touching_files(files)
        if fresh:
            ch["conversionManagedCommit"] = fresh
        if ml != old:
            print(f"{ch['name']}: {old} -> {ml}")
            updated += 1

    if not nums or updated > 0 or nums:
        tally_path.write_text(json.dumps(tally, indent=2) + "\n")
    print(f"updated {updated} chunk managedLoc values")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
