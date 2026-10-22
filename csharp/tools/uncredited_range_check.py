#!/usr/bin/env python3
"""Report whether native line ranges are already credited in loc_tally.json.

Usage:
  python3 uncredited_range_check.py --file src/playsim/p_tags.cpp --start 36 --end 464
  python3 uncredited_range_check.py --file src/playsim/p_tags.cpp --symbol FTagManager::AddSectorTag
  python3 uncredited_range_check.py --chunk 809   # reads nativeRanges from chunk809.json

Exit 0 when every requested line is uncredited; exit 1 when any line overlaps a live conversion row.
"""
from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
TALLY = REPO / "docs/data/loc_tally.json"


def load_credited() -> list[tuple[str, int, int, str]]:
    tally = json.loads(TALLY.read_text(encoding="utf-8"))
    out: list[tuple[str, int, int, str]] = []
    for ch in tally["chunks"]:
        if ch.get("bucket") != "conversion" or ch.get("retired"):
            continue
        if ch.get("nativeLoc", 0) == 0 and not ch.get("nativeRanges"):
            continue
        for r in ch.get("nativeRanges") or []:
            out.append(
                (
                    r["file"].replace("\\", "/"),
                    int(r["start"]),
                    int(r["end"]),
                    ch["name"],
                )
            )
    return out


def overlaps(s1: int, e1: int, s2: int, e2: int) -> bool:
    return s1 <= e2 and s2 <= e1


def find_symbol_range(cpp: Path, symbol: str) -> tuple[int, int] | None:
    text = cpp.read_text(encoding="utf-8", errors="replace").splitlines()
    pat = re.compile(rf"^\s*(?:[\w:*&<>\s]+)?\s*{re.escape(symbol.split('::')[-1])}\s*\(")
    if "::" in symbol:
        cls, meth = symbol.split("::", 1)
        pat = re.compile(rf"{re.escape(cls)}::{re.escape(meth)}\s*\(")
    for i, line in enumerate(text, 1):
        if not pat.search(line):
            continue
        j = i
        while j <= len(text) and "{" not in text[j - 1]:
            j += 1
        if j > len(text):
            return None
        depth = 0
        for k in range(j, len(text) + 1):
            depth += text[k - 1].count("{") - text[k - 1].count("}")
            if depth == 0 and "{" in "".join(text[j - 1 : k]):
                return i, k
    return None


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--file", type=str, help="Native file path (e.g. src/playsim/p_tags.cpp)")
    ap.add_argument("--start", type=int)
    ap.add_argument("--end", type=int)
    ap.add_argument("--symbol", action="append", default=[])
    ap.add_argument("--chunk", type=int, help="Use nativeRanges from csharp/tools/chunkN.json")
    args = ap.parse_args()

    ranges: list[tuple[str, int, int, str]] = []
    if args.chunk:
        meta = json.loads((REPO / "tools" / f"chunk{args.chunk}.json").read_text())[0]
        for r in meta["nativeRanges"]:
            ranges.append((r["file"], int(r["start"]), int(r["end"]), r.get("symbol", "?")))
    elif args.file and args.start and args.end:
        ranges.append((args.file, args.start, args.end, "cli"))
    elif args.file and args.symbol:
        cpp = REPO.parent / args.file if not args.file.startswith("/") else Path(args.file)
        if not cpp.is_absolute():
            cpp = REPO.parent / args.file
        for sym in args.symbol:
            span = find_symbol_range(cpp, sym)
            if not span:
                print(f"symbol not found: {sym}", file=sys.stderr)
                return 2
            ranges.append((args.file, span[0], span[1], sym))
    else:
        ap.error("provide --chunk N or --file with --start/--end or --symbol")

    credited = load_credited()
    self_chunk_name: str | None = None
    if args.chunk:
        meta = json.loads((REPO / "tools" / f"chunk{args.chunk}.json").read_text())[0]
        self_chunk_name = meta.get("name")
    any_hit = False
    print("uncredited_range_check")
    for file, start, end, label in ranges:
        loc = end - start + 1
        hits: list[str] = []
        norm = file.replace("\\", "/")
        base = norm.split("/")[-1]
        for cf, cs, ce, name in credited:
            if self_chunk_name and name == self_chunk_name:
                continue
            if not (cf.endswith(base) or norm in cf):
                continue
            if overlaps(start, end, cs, ce):
                hits.append(f"{name} {cf} {cs}-{ce}")
        status = "UNCredited" if not hits else "OVERLAP"
        if hits:
            any_hit = True
        print(f"  [{status}] {label} {file} {start}-{end} ({loc} lines)")
        for h in hits:
            print(f"    credited by: {h}")
    return 1 if any_hit else 0


if __name__ == "__main__":
    raise SystemExit(main())
