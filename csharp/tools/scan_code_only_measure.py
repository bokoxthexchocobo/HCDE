#!/usr/bin/env python3
"""Informational: conversion chunks below 50% under code-only managed LOC."""
from __future__ import annotations

import json
import re
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
TOOLS = REPO / "csharp" / "tools"
TALLY = REPO / "csharp" / "docs" / "data" / "loc_tally.json"
OUT = REPO / "csharp" / "docs" / "HCDE_CSHARP_CODE_ONLY_MEASURE_SCAN.md"

import sys

sys.path.insert(0, str(TOOLS))
from chunk_port_managed import managed_loc_surviving_at_head, physical_loc


def code_only_loc(path: Path) -> int:
    if not path.is_file():
        return 0
    n = 0
    for line in path.read_text(encoding="utf-8", errors="replace").splitlines():
        s = line.strip()
        if not s or s.startswith("//") or s.startswith("///"):
            continue
        n += 1
    return n


def chunk_num_from_name(name: str) -> int | None:
    m = re.search(r"chunk(\d+)", name)
    return int(m.group(1)) if m else None


def native_loc(ranges: list[dict]) -> int:
    return sum(int(r["end"]) - int(r["start"]) + 1 for r in ranges)


def main() -> None:
    tally = json.loads(TALLY.read_text())
    below: list[tuple] = []
    scanned = 0
    for ch in tally["chunks"]:
        if ch.get("bucket") != "conversion":
            continue
        num = chunk_num_from_name(ch.get("name", ""))
        if num is None:
            continue
        path = TOOLS / f"chunk{num}.json"
        if not path.is_file():
            continue
        data = json.loads(path.read_text())
        if isinstance(data, list):
            data = data[0]
        nl = native_loc(data.get("nativeRanges") or [])
        if nl <= 0:
            continue
        scanned += 1
        gate = managed_loc_surviving_at_head(data, num)
        code = sum(code_only_loc(REPO / f) for f in data.get("managedSourceFiles") or [])
        min_code = (nl + 1) // 2
        if code < min_code:
            below.append((num, ch.get("name"), nl, gate, code, code / nl if nl else 0))

    native_stake = sum(r[2] for r in below)
    lines = [
        "# Code-only managed LOC scan (informational)\n\n",
        "This scan does **not** change `check_port_coverage.sh` or reaudit gates.\n\n",
        "## Measurement definitions\n\n",
        "- **Gate (current):** every physical line in `managedSourceFiles` at HEAD "
        "(`chunk_port_managed.physical_loc`), including blanks and comment-only lines.\n",
        "- **Code-only (informational):** non-empty lines excluding `//` and `///` comment lines.\n",
        "- **Hypothetical floor:** `code_only >= (nativeLoc + 1) // 2` (same 50% rule, code-only numerator).\n\n",
        "## Summary\n\n",
        f"- Conversion chunks in `loc_tally.json` with `chunkN.json`: **{scanned}**\n",
        f"- Would fall below 50% under code-only: **{len(below)}**\n",
        f"- Native credit at stake (sum of those chunks): **{native_stake}**\n\n",
        "## Affected chunks\n\n",
        "| Chunk | Name | Native | Gate | Code-only | Code/Native |\n",
        "|------:|------|-------:|-----:|----------:|------------:|\n",
    ]
    for num, name, nl, gate, code, ratio in sorted(below, key=lambda x: x[0]):
        lines.append(f"| {num} | {name} | {nl} | {gate} | {code} | {ratio:.1%} |\n")
    OUT.write_text("".join(lines))
    print(f"Wrote {OUT} ({len(below)} below, native_stake={native_stake})")


if __name__ == "__main__":
    main()
