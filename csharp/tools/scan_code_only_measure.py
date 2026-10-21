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
from chunk_port_managed import managed_files_for_chunk, managed_loc_surviving_at_head


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


def code_only_for_chunk(data: dict, num: int) -> int:
    """Match gate: resolve managed files via chunk JSON or conversionManagedCommit."""
    files = managed_files_for_chunk(num, data)
    return sum(code_only_loc(REPO / rel) for rel in files)


def main() -> None:
    tally = json.loads(TALLY.read_text())
    below: list[tuple] = []
    scanned = 0
    pass_gate_fail_code = 0
    pass_both = 0
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
        code = code_only_for_chunk(data, num)
        min_code = (nl + 1) // 2
        gate_ok = gate >= min_code
        code_ok = code >= min_code
        if gate_ok and code_ok:
            pass_both += 1
        elif gate_ok and not code_ok:
            pass_gate_fail_code += 1
        if not code_ok:
            below.append((num, ch.get("name"), nl, gate, code, code / nl if nl else 0))

    native_stake = sum(r[2] for r in below)
    lines = [
        "# Code-only managed LOC scan (informational)\n\n",
        "This scan does **not** change `check_port_coverage.sh` or reaudit gates.\n\n",
        "## Measurement definitions\n\n",
        "- **Gate (current):** every physical line in `managedSourceFiles` at HEAD "
        "(`chunk_port_managed.physical_loc`), including blanks and comment-only lines.\n",
        "- **Managed file resolution:** same as the gate — explicit `managedSourceFiles` in "
        "`chunkN.json`, else `csharp/src/*.cs` touched by `conversionManagedCommit` "
        "(`chunk_port_managed.managed_files_for_chunk`). Older chunks without a file list are not scored as zero.\n",
        "- **Code-only (informational):** non-empty lines excluding `//` and `///` comment lines "
        "in those resolved files at HEAD.\n",
        "- **Hypothetical floor:** `code_only >= (nativeLoc + 1) // 2` (same 50% rule, code-only numerator).\n",
        "- **Shared files:** almost all chunks own a dedicated `.cs` file; two mapinfo helpers are "
        "referenced by two chunks each — each chunk counts the full file (same as the physical gate).\n\n",
        "## Summary\n\n",
        f"- Conversion chunks in `loc_tally.json` with `chunkN.json`: **{scanned}**\n",
        f"- Pass physical gate and code-only 50%: **{pass_both}**\n",
        f"- Pass physical gate but would fail code-only 50%: **{pass_gate_fail_code}**\n",
        f"- Would fall below 50% under code-only: **{len(below)}**\n",
        f"- Native credit in code-only-fail chunks (sum; double-counts if native ranges overlap across chunks): **{native_stake}**\n\n",
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
