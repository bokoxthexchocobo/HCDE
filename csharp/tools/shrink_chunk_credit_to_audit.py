#!/usr/bin/env python3
"""Shrink nativeRanges to lines cited in auditPaddingReview (never widen the map).

Guardrails:
- Never modifies auditPaddingReview (no auto-generated or whole-range map paste).
- Only replaces nativeRanges with the intersection of prior credit and parsed map lines.
- Skips chunks with no parseable map lines (coverage cannot be raised by padding credit).
- After shrink, nativeLoc < 150 or managed/native < 50% → merge or retire via merge_chunks.py
  (this script only flags; it never pads credit).
- Emits total removed + per-chunk before/after; optional --apply writes chunk JSON.
"""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

TOOLS = Path(__file__).resolve().parent
sys.path.insert(0, str(TOOLS))
from chunk_port_managed import managed_loc_surviving_at_head  # noqa: E402
from reaudit_chunks import (  # noqa: E402
    audit_map_coverage_ratio,
    credited_line_intervals,
    merge_intervals,
    parse_audit_map_line_ranges,
)

NATIVE_LOC_FLOOR = 150
MANAGED_RATIO_FLOOR = 0.5


def load_chunk(num: int) -> dict | None:
    path = TOOLS / f"chunk{num}.json"
    if not path.is_file():
        return None
    data = json.loads(path.read_text(encoding="utf-8"))
    return data[0] if isinstance(data, list) else data


def native_loc(ranges: list[dict]) -> int:
    return sum(e - s + 1 for s, e in merge_intervals(credited_line_intervals(ranges)))


def intersect_intervals(a: list[tuple[int, int]], b: list[tuple[int, int]]) -> list[tuple[int, int]]:
    out: list[tuple[int, int]] = []
    for as_, ae in a:
        for bs, be in b:
            s, e = max(as_, bs), min(ae, be)
            if s <= e:
                out.append((s, e))
    return merge_intervals(out)


def shrink_ranges(chunk: dict) -> tuple[list[dict], int, int, float, str]:
    ranges = chunk.get("nativeRanges") or []
    audit = chunk.get("auditPaddingReview") or ""
    before = native_loc(ranges)
    if before == 0 or chunk.get("retired"):
        return ranges, before, before, 1.0, "retired_or_empty"
    mapped = merge_intervals(parse_audit_map_line_ranges(audit))
    if not mapped:
        return ranges, before, before, 0.0, "no_map_lines"
    cred = merge_intervals(credited_line_intervals(ranges))
    aligned = intersect_intervals(cred, mapped)
    after = sum(e - s + 1 for s, e in aligned)
    if not aligned:
        return ranges, before, 0, 0.0, "empty_intersection"
    ratio = audit_map_coverage_ratio(
        [{"start": s, "end": e} for s, e in aligned],
        audit,
    )
    if after > before:
        return ranges, before, before, ratio, "would_expand"
    template = ranges[0]
    new_ranges = []
    for s, e in aligned:
        new_ranges.append(
            {
                "file": template.get("file", ""),
                "symbol": template.get("symbol", ""),
                "start": s,
                "end": e,
                "tests": template.get("tests") or [],
            }
        )
    status = "unchanged" if after == before else "shrunk"
    return new_ranges, before, after, ratio, status


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--from", dest="lo", type=int, required=True)
    ap.add_argument("--to", dest="hi", type=int, required=True)
    ap.add_argument("--apply", action="store_true")
    args = ap.parse_args()

    total_before = 0
    total_after = 0
    rows: list[dict] = []
    for num in range(args.lo, args.hi + 1):
        chunk = load_chunk(num)
        if chunk is None:
            continue
        new_ranges, before, after, ratio, status = shrink_ranges(chunk)
        total_before += before
        total_after += after
        managed = 0
        managed_ratio = None
        if not chunk.get("retired"):
            managed = managed_loc_surviving_at_head(chunk, num)
            if after > 0:
                managed_ratio = round(managed / after, 3)
        needs_merge = (
            after > 0
            and (
                after < NATIVE_LOC_FLOOR
                or (managed_ratio is not None and managed_ratio < MANAGED_RATIO_FLOOR)
            )
        )
        if before != after or status not in ("unchanged", "retired_or_empty"):
            rows.append(
                {
                    "chunk": num,
                    "name": chunk.get("name", ""),
                    "before": before,
                    "after": after,
                    "removed": before - after,
                    "managedLoc": managed,
                    "managedRatio": managed_ratio,
                    "coverage": round(ratio * 100, 1),
                    "status": status,
                    "below_native_floor": after > 0 and after < NATIVE_LOC_FLOOR,
                    "needs_merge_or_retire": needs_merge and status in ("shrunk", "unchanged"),
                }
            )
        if args.apply and status == "shrunk":
            chunk["nativeRanges"] = new_ranges
            (TOOLS / f"chunk{num}.json").write_text(
                json.dumps([chunk], indent=2) + "\n", encoding="utf-8"
            )

    print(f"native credit sum before={total_before} after={total_after} removed={total_before - total_after}")
    print(f"chunks with changes: {len(rows)}")
    for r in sorted(rows, key=lambda x: -x["removed"]):
        print(
            f"  chunk{r['chunk']} {r['before']}->{r['after']} (-{r['removed']}) "
            f"cov={r['coverage']}% {r['status']} merge={r['needs_merge_or_retire']} {r['name']}"
        )
    report_path = TOOLS.parent / "docs" / "HCDE_CSHARP_COVERAGE_SHRINK_REPORT.json"
    payload = {
        "summary": {
            "from": args.lo,
            "to": args.hi,
            "nativeCreditBefore": total_before,
            "nativeCreditAfter": total_after,
            "nativeCreditRemoved": total_before - total_after,
            "chunksReported": len(rows),
        },
        "chunks": rows,
    }
    report_path.write_text(json.dumps(payload, indent=2) + "\n", encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
