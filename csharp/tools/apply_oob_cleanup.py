#!/usr/bin/env python3
"""Trim out-of-function-body lines from chunkN.json nativeRanges only.

Does NOT read or write loc_tally.json, native_managed_matrix, or invoke
fix_chunk_tally / sync_loc_tally / reaudit. After editing a manifest, land only
via land_chunk_fast.sh.

Usage: python3 apply_oob_cleanup.py <chunk_number>
"""
from __future__ import annotations

import json
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
TOOLS = REPO / "csharp" / "tools"
sys.path.insert(0, str(TOOLS))
from chunk_port_managed import managed_loc_surviving_at_head  # noqa: E402
from scan_tally_non_code_native_credits import function_body_line_set  # noqa: E402


def contiguous(segments: list[int]) -> list[tuple[int, int]]:
    if not segments:
        return []
    segments = sorted(segments)
    out: list[tuple[int, int]] = []
    s = e = segments[0]
    for x in segments[1:]:
        if x == e + 1:
            e = x
        else:
            out.append((s, e))
            s = e = x
    out.append((s, e))
    return out


def trim_ranges(ranges: list[dict], cache: dict) -> list[dict]:
    out: list[dict] = []
    for r in ranges:
        path = r["file"].replace("\\", "/")
        if path not in cache:
            fp = REPO / path
            lines = (
                fp.read_text(encoding="utf-8", errors="replace").splitlines()
                if fp.is_file()
                else []
            )
            cache[path] = (lines, function_body_line_set(lines))
        lines, bodies = cache[path]
        kept: list[int] = []
        for ln in range(int(r["start"]), int(r["end"]) + 1):
            if ln in bodies:
                kept.append(ln)
        for start, end in contiguous(kept):
            nr = dict(r)
            nr["start"] = start
            nr["end"] = end
            out.append(nr)
    return out


def native_loc(ranges: list[dict]) -> int:
    return sum(int(r["end"]) - int(r["start"]) + 1 for r in ranges)


def band_hull_overlap_hits(meta: dict, ranges: list[dict]) -> list[str]:
    """Same hull + overlap rules as ``uncredited_range_check.py --band`` (read-only)."""
    if not ranges:
        return []
    from uncredited_range_check import load_credited, overlaps  # noqa: E402

    band_file = ranges[0]["file"]
    if any(r["file"] != band_file for r in ranges):
        return ["(multiple native files; --band hull undefined)"]
    start = min(int(r["start"]) for r in ranges)
    end = max(int(r["end"]) for r in ranges)
    norm = band_file.replace("\\", "/")
    base = norm.split("/")[-1]
    self_name = meta.get("name")
    hits: list[str] = []
    for cf, cs, ce, name in load_credited():
        if self_name and name == self_name:
            continue
        if not (cf.endswith(base) or norm in cf):
            continue
        if overlaps(start, end, cs, ce):
            hits.append(f"{name} {cf} {cs}-{ce}")
    return hits


def _retire_oob_row(meta: dict, n: int, note_suffix: str, audit_uncredited: str) -> None:
    meta["retired"] = True
    meta["nativeRanges"] = []
    meta["managedSourceFiles"] = []
    meta.pop("conversionManagedCommit", None)
    meta["note"] = ((meta.get("note") or "") + " " + note_suffix).strip()
    meta["auditUncreditedRangeCheck"] = audit_uncredited
    meta["auditManagedLocJustification"] = "Retired row; matrix native credit **0**."


def main() -> int:
    n = int(sys.argv[1])
    path = TOOLS / f"chunk{n}.json"
    if not path.is_file():
        print(f"missing {path}")
        return 1
    data = json.loads(path.read_text(encoding="utf-8"))
    meta = data[0] if isinstance(data, list) else data
    if meta.get("retired"):
        if meta.pop("conversionManagedCommit", None) is not None:
            path.write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")
            print(f"chunk {n}: already retired — cleared conversionManagedCommit")
        else:
            print(f"chunk {n}: already retired")
        return 0
    gross = native_loc(meta.get("nativeRanges") or [])
    cache: dict = {}
    new_ranges = trim_ranges(meta.get("nativeRanges") or [], cache)
    honest = native_loc(new_ranges)
    oob = gross - honest
    print(f"chunk {n}: gross={gross} honest={honest} oob_removed={oob}")

    gross_block = (
        f"\n\n### Out-of-body uncredit ({n})\n\n"
        f"| | `nativeLoc` |\n|--|----------:|\n"
        f"| **Gross** | **{gross}** |\n"
        f"| **Out-of-body removed** | **{oob}** |\n"
        f"| **Honest** | **{honest}** |\n"
    )
    review = (meta.get("auditPaddingReview") or "").rstrip()
    if f"### Out-of-body uncredit ({n})" not in review:
        meta["auditPaddingReview"] = review + gross_block

    overlap_hits = band_hull_overlap_hits(meta, new_ranges)

    if honest < 150:
        _retire_oob_row(
            meta,
            n,
            f"Retired (2026-10-23): out-of-body credit removed; honest **{honest}** < **150**.",
            f"`uncredited_range_check.py --chunk {n} --band` → retired skip (out-of-body cleanup), exit 0.",
        )
        print(f"chunk {n}: RETIRE")
    elif overlap_hits:
        hit = overlap_hits[0]
        _retire_oob_row(
            meta,
            n,
            f"Retired (2026-10-24): `--band` hull overlaps existing credit ({hit}).",
            f"`uncredited_range_check.py --chunk {n} --band` → retired skip (OVERLAP), exit 0.",
        )
        print(f"chunk {n}: RETIRE (band overlap)")
    else:
        surviving = managed_loc_surviving_at_head(meta, n)
        need = (honest + 1) // 2
        if surviving < need:
            _retire_oob_row(
                meta,
                n,
                (
                    f"Retired (2026-10-24): thin managed at HEAD — surviving **{surviving}** "
                    f"< **{need}** (50% of honest **{honest}**)."
                ),
                (
                    f"`check_port_coverage.sh` thin managed: surviving {surviving} < need {need} "
                    f"(out-of-body cleanup), exit 0."
                ),
            )
            print(f"chunk {n}: RETIRE (managed floor)")
        else:
            meta["nativeRanges"] = new_ranges
            meta["auditUncreditedRangeCheck"] = (
                meta.get("auditUncreditedRangeCheck")
                or f"`uncredited_range_check.py --chunk {n} --band` after out-of-body trim."
            )
            print(f"chunk {n}: KEEP (needs --band land)")

    path.write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
