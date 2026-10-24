#!/usr/bin/env python3
"""Remove duplicate native line credits; keep each line on the earliest chunk only."""

from __future__ import annotations

import json
import sys
from collections import defaultdict
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
TALLY_PATH = REPO / "docs/data/loc_tally.json"


def chunk_bucket(chunk: dict) -> str:
    bucket = chunk.get("bucket")
    if bucket in {"conversion", "recognition"}:
        return bucket
    if "recognition" in chunk.get("name", ""):
        return "recognition"
    return "conversion"


def lines_in_range(start: int, end: int) -> list[int]:
    return list(range(start, end + 1))


def compress_line_sets(line_nums: list[int]) -> list[tuple[int, int]]:
    if not line_nums:
        return []
    line_nums = sorted(line_nums)
    spans: list[tuple[int, int]] = []
    span_start = span_end = line_nums[0]
    for ln in line_nums[1:]:
        if ln == span_end + 1:
            span_end = ln
        else:
            spans.append((span_start, span_end))
            span_start = span_end = ln
    spans.append((span_start, span_end))
    return spans


def find_overlaps(chunks: list[dict]) -> dict[tuple[str, int], list[tuple[int, str]]]:
    hits: dict[tuple[str, int], list[tuple[int, str]]] = defaultdict(list)
    for ci, chunk in enumerate(chunks):
        for r in chunk.get("nativeRanges") or []:
            file = r["file"]
            for ln in range(int(r["start"]), int(r["end"]) + 1):
                hits[(file, ln)].append((ci, chunk["name"]))
    return {k: v for k, v in hits.items() if len(v) > 1}


def dedupe_chunks(chunks: list[dict]) -> tuple[list[dict], list[dict]]:
    """Return (updated chunks, audit rows)."""
    credited: set[tuple[str, int]] = set()
    audit: list[dict] = []

    for ci, chunk in enumerate(chunks):
        ranges = chunk.get("nativeRanges")
        if not ranges:
            continue

        new_ranges: list[dict] = []
        for r in ranges:
            file = r["file"]
            start, end = int(r["start"]), int(r["end"])
            kept_lines = [ln for ln in range(start, end + 1) if (file, ln) not in credited]
            dropped = (end - start + 1) - len(kept_lines)
            if dropped:
                audit.append(
                    {
                        "chunkIndex": ci,
                        "chunk": chunk["name"],
                        "symbol": r.get("symbol", ""),
                        "file": file,
                        "original": [start, end],
                        "droppedLines": dropped,
                    }
                )

            for span_start, span_end in compress_line_sets(kept_lines):
                nr = dict(r)
                nr["start"] = span_start
                nr["end"] = span_end
                new_ranges.append(nr)
                for ln in range(span_start, span_end + 1):
                    credited.add((file, ln))

        chunk["nativeRanges"] = new_ranges

    return chunks, audit


def main() -> int:
    tally = json.loads(TALLY_PATH.read_text(encoding="utf-8"))
    chunks = tally.get("chunks", [])
    before_overlaps = find_overlaps(chunks)
    before_extra = sum(len(v) - 1 for v in before_overlaps.values())

    chunks, audit = dedupe_chunks(chunks)
    tally["chunks"] = chunks

    after_overlaps = find_overlaps(chunks)
    TALLY_PATH.write_text(json.dumps(tally, indent=2) + "\n", encoding="utf-8")

    print(f"Overlapping lines before: {len(before_overlaps)} ({before_extra} extra credits)")
    print(f"Overlapping lines after:  {len(after_overlaps)}")
    print(f"Audit rows (ranges trimmed): {len(audit)}")
    dropped_by_chunk: dict[str, int] = defaultdict(int)
    for row in audit:
        dropped_by_chunk[row["chunk"]] += row["droppedLines"]
    top = sorted(dropped_by_chunk.items(), key=lambda x: -x[1])[:20]
    for name, n in top:
        print(f"  -{n:4d}  {name}")
    return 0 if not after_overlaps else 1


if __name__ == "__main__":
    sys.exit(main())
