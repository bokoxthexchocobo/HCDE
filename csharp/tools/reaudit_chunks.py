#!/usr/bin/env python3
"""Test-backed loc_tally re-audit for a chunk number range.

Usage:
  python3 reaudit_chunks.py audit --from 207 --to 245
  python3 reaudit_chunks.py apply-fixes   # applies FIXES dict to loc_tally.json

Audit fails (hard) when:
  - any two conversion chunks credit overlapping line ranges in the same native file
  - conversion chunk nativeLoc != sum(nativeRanges)
  - conversion chunk with nativeLoc > 0 lacks conversionManagedCommit SHA
  - any range lists a test method that does not exist in *Tests.cs
  - conversion range has zero tests
  - conversion tally row with chunk number >= CHUNK_RULES_MIN (450): nativeLoc <150 or >400
  - conversion tally row with chunk number < 450: same bounds reported as soft legacy_* (non-blocking)
  - conversion chunk number >= CHUNK_RULES_MIN (450): credited managed file contains a
    padding-shaped method body not explicitly listed in auditPaddingReview (constant/identity/empty,
    or `*Plan` record structs / `Plan*Native` aggregator methods)
  - conversion chunk number >= CHUNK_RULES_MIN (450): auditPaddingReview native line map covers
    <90% of credited nativeRanges (union of map ranges vs union of credited ranges)

Audit warns (soft, not auto-trimmed) when:
  - range span >= 60 and only one test (thin band — manual review)
  - conversion chunk: managed LOC from conversionManagedCommit src/*.cs additions
    is under 50% of credited nativeLoc (thin_managed_coverage)
  - active chunk{N}.json lacks non-empty auditPaddingReview (method-to-native padding map)
  - conversion chunk: managed LOC > 150% of nativeLoc without auditManagedLocJustification paragraph
"""
from __future__ import annotations

import argparse
import json
import re
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
TALLY = REPO / "docs/data/loc_tally.json"
TOOLS = REPO / "tools"
sys.path.insert(0, str(TOOLS))
from chunk_port_managed import (  # noqa: E402
    assert_no_commit_inflation,
    managed_loc_commit_additions_sum,
    managed_loc_surviving_at_head,
)
from generate_native_managed_matrix import merge_intervals  # noqa: E402

AUDIT_MAP_LINE_RANGE_RE = re.compile(r"(\d+)\s*[\u2013\-]\s*(\d+)")
MAP_COVERAGE_MIN = 0.90

# Manual trims / fixes — only rows that fail audit or are verified over-credits.
NATIVE_LOC_FLOOR = 150
NATIVE_LOC_CEILING = 400
# First conversion chunk tranche under surviving-HEAD managed LOC (b3ec1e4e, 2026-10-20).
# Native 150–400 land gate added in 4694cbae; reaudit enforces on tally nativeLoc from this index up.
CHUNK_RULES_MIN_CHUNK = 450

FIXES: dict[str, list[dict] | str] = {
    "a-keys-check-chunk238": "fix_tests_only",
}

FIX_TEST_RENAMES: dict[str, dict[str, str]] = {
    "a-keys-check-chunk238": {
        "VmLockDefKeywords.LockKeywords_IncludesAny": "VmActorKeyCheckTests.LockKeywords_IncludesAny",
    },
}


def chunk_sort_key(name: str) -> tuple[int, int]:
    m = re.search(r"chunk(\d+)(b)?", name)
    if not m:
        return (9999, 0)
    base = int(m.group(1))
    suffix = 1 if m.group(2) else 0
    return (base, suffix)


def in_range(name: str, lo: int, hi: int) -> bool:
    m = re.search(r"chunk(\d+)(b)?", name)
    if not m:
        return False
    n = int(m.group(1))
    return lo <= n <= hi


def load_test_methods() -> set[str]:
    methods: set[str] = set()
    for path in (REPO.parent / "csharp/tests").rglob("*Tests.cs"):
        text = path.read_text(errors="replace")
        classes = re.findall(r"public class (\w+)", text)
        test_names = re.findall(r"public void (\w+)\(", text)
        for cls in classes:
            for meth in test_names:
                methods.add(f"{cls}.{meth}")
    return methods


def native_loc(ranges: list[dict]) -> int:
    return sum(int(r["end"]) - int(r["start"]) + 1 for r in ranges)


def chunk_number(name: str) -> int | None:
    m = re.search(r"chunk(\d+)(b)?", name)
    return int(m.group(1)) if m else None


def subject_to_modern_chunk_rules(num: int | None) -> bool:
    return num is not None and num >= CHUNK_RULES_MIN_CHUNK


def chunk_def_for_tally_row(ch: dict) -> dict:
    num = chunk_number(ch.get("name", ""))
    if num is None:
        return ch
    path = TOOLS / f"chunk{num}.json"
    if not path.is_file():
        return ch
    data = json.loads(path.read_text())
    if isinstance(data, list):
        data = data[0]
    merged = {**ch, **data}
    for k in ("nativeLoc", "managedLoc", "conversionManagedCommit", "retired", "bucket"):
        if k in ch:
            merged[k] = ch[k]
    if data.get("portCommits"):
        merged["portCommits"] = data["portCommits"]
    return merged


REPO_ROOT = REPO.parent


def padding_allowlist_from_audit(audit_padding_review: str) -> set[str]:
    return set(re.findall(r"`([^`]+)`", audit_padding_review or ""))


def parse_audit_map_line_ranges(audit_padding_review: str) -> list[tuple[int, int]]:
    """Inclusive line spans cited in auditPaddingReview tables (e.g. 370–374)."""
    out: list[tuple[int, int]] = []
    for m in AUDIT_MAP_LINE_RANGE_RE.finditer(audit_padding_review or ""):
        start, end = int(m.group(1)), int(m.group(2))
        if start <= end:
            out.append((start, end))
    return out


def credited_line_intervals(ranges: list[dict]) -> list[tuple[int, int]]:
    return [(int(r["start"]), int(r["end"])) for r in ranges if "start" in r and "end" in r]


def intersection_line_count(
    credited: list[tuple[int, int]], mapped: list[tuple[int, int]]
) -> int:
    if not credited or not mapped:
        return 0
    cred_m = merge_intervals(credited)
    map_m = merge_intervals(mapped)
    total = 0
    for cs, ce in cred_m:
        for ms, me in map_m:
            s, e = max(cs, ms), min(ce, me)
            if s <= e:
                total += e - s + 1
    return total


def audit_map_coverage_ratio(ranges: list[dict], audit_padding_review: str) -> float:
    credited = credited_line_intervals(ranges)
    if not credited:
        return 1.0
    cred_lines = sum(e - s + 1 for s, e in merge_intervals(credited))
    if cred_lines <= 0:
        return 1.0
    mapped = parse_audit_map_line_ranges(audit_padding_review)
    hit = intersection_line_count(credited, mapped)
    return hit / cred_lines


def _strip_cs_comments(text: str) -> str:
    text = re.sub(r"/\*.*?\*/", "", text, flags=re.S)
    text = re.sub(r"//.*?$", "", text, flags=re.M)
    return text


def _param_names(param_list: str) -> list[str]:
    names: list[str] = []
    for p in param_list.split(","):
        p = p.strip()
        if not p:
            continue
        pname = p.split()[-1].strip()
        names.append(pname.rstrip("?"))
    return names


def scan_padding_methods(cs_text: str) -> list[dict]:
    """Detect methods whose bodies are only constants, identity, or empty."""
    text = _strip_cs_comments(cs_text)
    hits: list[dict] = []
    expr_body = re.compile(r"\b(\w+)\s*\(([^)]*)\)\s*=>\s*([^;]+);", re.M)
    for m in expr_body.finditer(text):
        name, params, expr = m.group(1), m.group(2), m.group(3).strip()
        if re.fullmatch(r"(0u?l?|true|false|null)", expr):
            hits.append({"method": name, "kind": "constant_expr", "detail": expr})
            continue
        id_match = re.fullmatch(r"(\w+)", expr)
        if id_match:
            pnames = _param_names(params)
            if len(pnames) == 1 and id_match.group(1) == pnames[0]:
                hits.append({"method": name, "kind": "identity_expr", "detail": expr})
    empty_block = re.compile(r"\b(\w+)\s*\([^)]*\)\s*\{\s*\}", re.M)
    for m in empty_block.finditer(text):
        hits.append({"method": m.group(1), "kind": "empty_block", "detail": ""})
    plan_struct = re.compile(
        r"\b(?:public\s+)?(?:readonly\s+)?record\s+struct\s+(\w*Plan)\s*\(",
        re.M,
    )
    for m in plan_struct.finditer(text):
        hits.append({"method": m.group(1), "kind": "plan_record_struct", "detail": "record struct"})
    plan_method = re.compile(r"\b\w+\s+(Plan\w*Native)\s*\(", re.M)
    for m in plan_method.finditer(text):
        hits.append({"method": m.group(1), "kind": "plan_aggregator_method", "detail": m.group(1)})
    return hits


def audit_managed_padding(ch: dict, num: int | None) -> list[dict]:
    if not subject_to_modern_chunk_rules(num):
        return []
    chunk_def = chunk_def_for_tally_row(ch)
    if chunk_def.get("retired"):
        return []
    nl = int(ch.get("nativeLoc", 0))
    if nl <= 0:
        return []
    allow = padding_allowlist_from_audit(chunk_def.get("auditPaddingReview") or "")
    files = chunk_def.get("managedSourceFiles") or []
    hard: list[dict] = []
    for rel in files:
        path = REPO_ROOT / rel
        if not path.is_file():
            hard.append({"kind": "managed_padding_missing_file", "file": rel})
            continue
        for hit in scan_padding_methods(path.read_text(errors="replace")):
            if hit["method"] not in allow:
                hard.append(
                    {
                        "kind": "managed_padding_signature",
                        "file": rel,
                        "method": hit["method"],
                        "padding_kind": hit["kind"],
                        "detail": hit.get("detail", ""),
                    }
                )
    return hard


def measured_port_managed(ch: dict) -> int:
    num = chunk_number(ch.get("name", ""))
    chunk_def = chunk_def_for_tally_row(ch)
    files = chunk_def.get("managedSourceFiles") or ch.get("managedSourceFiles") or []
    if not files:
        return int(ch.get("managedLoc", 0))
    chunk_def = {**chunk_def, "managedSourceFiles": files}
    return managed_loc_surviving_at_head(chunk_def, num)


def collect_active_conversion_ranges(tally: dict) -> list[tuple[str, int, int, str]]:
    """(file, start, end, chunk_name) for non-retired conversion rows with native ranges."""
    out: list[tuple[str, int, int, str]] = []
    for ch in tally.get("chunks") or []:
        if ch.get("retired"):
            continue
        if (ch.get("bucket") or "conversion") != "conversion":
            continue
        name = ch.get("name", "")
        for r in ch.get("nativeRanges") or []:
            f = r.get("file") or ""
            if not f or "start" not in r or "end" not in r:
                continue
            out.append((f, int(r["start"]), int(r["end"]), name))
    return out


def find_native_range_overlaps(ranges: list[tuple[str, int, int, str]]) -> list[dict]:
    by_file: dict[str, list[tuple[int, int, str]]] = {}
    for f, start, end, name in ranges:
        by_file.setdefault(f, []).append((start, end, name))
    hard: list[dict] = []
    for file_path, items in by_file.items():
        items.sort(key=lambda t: (t[0], t[1]))
        for i, (s1, e1, n1) in enumerate(items):
            for s2, e2, n2 in items[i + 1 :]:
                if s2 <= e1:
                    hard.append(
                        {
                            "kind": "native_range_overlap",
                            "file": file_path,
                            "a": {"chunk": n1, "start": s1, "end": e1},
                            "b": {"chunk": n2, "start": s2, "end": e2},
                        }
                    )
    return hard


def native_loc_from_chunk_json(num: int) -> int | None:
    path = TOOLS / f"chunk{num}.json"
    if not path.is_file():
        return None
    data = json.loads(path.read_text())
    if isinstance(data, list):
        data = data[0]
    return native_loc(data.get("nativeRanges") or [])


def audit_chunk(ch: dict, tests: set[str]) -> tuple[list[dict], list[dict]]:
    hard: list[dict] = []
    soft: list[dict] = []
    name = ch.get("name", "")
    bucket = ch.get("bucket") or "conversion"
    nl = int(ch.get("nativeLoc", 0))
    ranges = ch.get("nativeRanges") or []
    num = chunk_number(name)

    def append_native_bounds(native: int, source: str, modern: bool) -> None:
        if native <= 0:
            return
        if native < NATIVE_LOC_FLOOR:
            entry = {
                "kind": "nativeLoc_below_floor",
                "nativeLoc": native,
                "floor": NATIVE_LOC_FLOOR,
                "source": source,
            }
            if modern:
                hard.append(entry)
            else:
                soft.append({**entry, "kind": "legacy_nativeLoc_below_floor"})
        elif native > NATIVE_LOC_CEILING:
            entry = {
                "kind": "nativeLoc_above_ceiling",
                "nativeLoc": native,
                "ceiling": NATIVE_LOC_CEILING,
                "source": source,
            }
            if modern:
                hard.append(entry)
            else:
                soft.append({**entry, "kind": "legacy_nativeLoc_above_ceiling"})

    modern_rules = subject_to_modern_chunk_rules(num)
    if bucket == "conversion" and nl > 0:
        append_native_bounds(nl, "loc_tally", modern_rules)

    json_nl: int | None = None
    if num is not None and bucket == "conversion":
        json_nl = native_loc_from_chunk_json(num)
        if json_nl is not None and json_nl != nl:
            hard.append(
                {
                    "kind": "chunk_json_nativeLoc_mismatch",
                    "tally_nativeLoc": nl,
                    "chunk_json_nativeLoc": json_nl,
                }
            )
    sumr = native_loc(ranges)
    if nl != sumr:
        hard.append({"kind": "nativeLoc_mismatch", "expected": sumr, "actual": nl})

    if bucket == "conversion" and nl > 0:
        sha = ch.get("conversionManagedCommit", "") or ""
        if not re.fullmatch(r"[0-9a-f]{40}", sha):
            hard.append({"kind": "bad_conversion_sha", "sha": sha})
        if int(ch.get("managedLoc", 0)) <= 0:
            hard.append({"kind": "managedLoc_zero"})

    for r in ranges:
        span = int(r["end"]) - int(r["start"]) + 1
        test_list = r.get("tests") or []
        if bucket == "conversion" and nl > 0 and not test_list:
            hard.append({"kind": "no_tests", "symbol": r.get("symbol"), "span": span})
        for t in test_list:
            if t not in tests:
                hard.append({"kind": "missing_test", "test": t, "symbol": r.get("symbol")})
        if span >= 60 and len(test_list) <= 1 and test_list:
            soft.append({"kind": "thin_test_band", "symbol": r.get("symbol"), "span": span, "tests": test_list})

    if bucket == "conversion" and nl > 0:
        num = chunk_number(name)
        chunk_def = chunk_def_for_tally_row(ch)
        sha = ch.get("conversionManagedCommit", "") or ""
        surviving_ml = measured_port_managed(ch)
        tally_ml = int(ch.get("managedLoc", 0))
        if surviving_ml > 0 and tally_ml != surviving_ml:
            hard.append(
                {
                    "kind": "managedLoc_mismatch",
                    "managedLoc_surviving_head": surviving_ml,
                    "managedLoc_tally": tally_ml,
                }
            )
        measured = surviving_ml if surviving_ml > 0 else tally_ml
        if measured > 0 and measured < nl * 0.5:
            soft.append(
                {
                    "kind": "thin_managed_coverage",
                    "nativeLoc": nl,
                    "managedLoc_surviving_head": surviving_ml,
                    "managedLoc_tally": tally_ml,
                    "ratio": round(measured / nl, 3),
                }
            )

    if bucket == "conversion" and nl > 0 and num is not None:
        path = TOOLS / f"chunk{num}.json"
        if path.is_file():
            meta = json.loads(path.read_text())
            if isinstance(meta, list):
                meta = meta[0]
            if not (meta.get("auditPaddingReview") or "").strip():
                soft.append({"kind": "missing_audit_padding_review", "chunk": num})
            elif subject_to_modern_chunk_rules(num):
                audit_text = meta.get("auditPaddingReview") or ""
                ratio = audit_map_coverage_ratio(ranges, audit_text)
                if ratio < MAP_COVERAGE_MIN:
                    hard.append(
                        {
                            "kind": "audit_map_coverage_below_floor",
                            "ratio": round(ratio, 4),
                            "floor": MAP_COVERAGE_MIN,
                            "credited_lines": sum(
                                e - s + 1 for s, e in merge_intervals(credited_line_intervals(ranges))
                            ),
                            "mapped_lines": sum(
                                e - s + 1 for s, e in merge_intervals(parse_audit_map_line_ranges(audit_text))
                            ),
                        }
                    )
            if measured > 0 and nl > 0 and measured > nl * 1.5:
                justification = (meta.get("auditManagedLocJustification") or "").strip()
                if not justification:
                    soft.append(
                        {
                            "kind": "missing_managed_loc_justification",
                            "nativeLoc": nl,
                            "managedLoc_surviving_head": surviving_ml,
                            "ratio": round(measured / nl, 3),
                        }
                    )

    for entry in audit_managed_padding(ch, num):
        hard.append(entry)

    return hard, soft


def audit_all_conversion_chunks(tests: set[str]) -> list[tuple[dict, list[dict], list[dict]]]:
    tally = json.loads(TALLY.read_text())
    rows: list[tuple[dict, list[dict], list[dict]]] = []
    for ch in tally["chunks"]:
        if (ch.get("bucket") or "conversion") != "conversion":
            continue
        if int(ch.get("nativeLoc", 0)) <= 0:
            continue
        rows.append((ch, *audit_chunk(ch, tests)))
    return rows


def cmd_map_coverage(lo: int, hi: int) -> int:
    tests = load_test_methods()
    tally = json.loads(TALLY.read_text())
    rows: list[tuple[int, str, int, int, float, bool]] = []
    for ch in tally["chunks"]:
        if ch.get("retired") or (ch.get("bucket") or "conversion") != "conversion":
            continue
        num = chunk_number(ch.get("name", ""))
        if num is None or not (lo <= num <= hi):
            continue
        nl = int(ch.get("nativeLoc", 0))
        if nl <= 0:
            continue
        chunk_def = chunk_def_for_tally_row(ch)
        ranges = chunk_def.get("nativeRanges") or []
        audit = chunk_def.get("auditPaddingReview") or ""
        cred_lines = sum(e - s + 1 for s, e in merge_intervals(credited_line_intervals(ranges)))
        map_lines = sum(e - s + 1 for s, e in merge_intervals(parse_audit_map_line_ranges(audit)))
        ratio = audit_map_coverage_ratio(ranges, audit)
        rows.append((num, ch["name"], cred_lines, map_lines, ratio, ratio >= MAP_COVERAGE_MIN))
    rows.sort(key=lambda t: t[4])
    print(f"auditPaddingReview map coverage (chunks {lo}-{hi}, floor {MAP_COVERAGE_MIN:.0%}):")
    fails = 0
    for num, name, cred, mapped, ratio, ok in rows:
        status = "ok" if ok else "FAIL"
        if not ok:
            fails += 1
        print(f"  {status} chunk{num} {ratio * 100:5.1f}%  credited={cred} mapped={mapped}  {name}")
    print(f"\n{len(rows)} chunks, {fails} below floor")
    return 1 if fails else 0


def cmd_breakdown() -> int:
    tests = load_test_methods()
    rows = audit_all_conversion_chunks(tests)
    from collections import defaultdict

    def bucket():
        return {"chunks": 0, "events": 0, "nativeLoc": 0}

    pre_hard: dict[str, dict] = defaultdict(bucket)
    pre_soft: dict[str, dict] = defaultdict(bucket)
    post_hard: dict[str, dict] = defaultdict(bucket)
    post_soft: dict[str, dict] = defaultdict(bucket)
    pre_chunk_ids: set[str] = set()
    post_chunk_ids: set[str] = set()

    for ch, hard, soft in rows:
        num = chunk_number(ch.get("name", ""))
        modern = subject_to_modern_chunk_rules(num)
        nl = int(ch.get("nativeLoc", 0))
        name = ch.get("name", "")
        if modern:
            post_chunk_ids.add(name)
        else:
            pre_chunk_ids.add(name)
        for h in hard:
            dest = post_hard if modern else pre_hard
            dest[h["kind"]]["events"] += 1
            dest[h["kind"]]["nativeLoc"] += nl
        for s in soft:
            dest = post_soft if modern else pre_soft
            dest[s["kind"]]["events"] += 1
            dest[s["kind"]]["nativeLoc"] += nl

    def sum_events(d: dict) -> int:
        return sum(v["events"] for v in d.values())

    def sum_native(d: dict) -> int:
        return sum(v["nativeLoc"] for v in d.values())

    out = {
        "chunk_rules_min_chunk": CHUNK_RULES_MIN_CHUNK,
        "conversion_chunks_with_nativeLoc": len(rows),
        "pre_rules": {
            "chunks": len(pre_chunk_ids),
            "hard_events": sum_events(pre_hard),
            "soft_events": sum_events(pre_soft),
            "hard_nativeLoc_sum": sum_native(pre_hard),
            "soft_nativeLoc_sum": sum_native(pre_soft),
            "hard_by_kind": dict(sorted(pre_hard.items())),
            "soft_by_kind": dict(sorted(pre_soft.items())),
        },
        "post_rules": {
            "chunks": len(post_chunk_ids),
            "hard_events": sum_events(post_hard),
            "soft_events": sum_events(post_soft),
            "hard_nativeLoc_sum": sum_native(post_hard),
            "soft_nativeLoc_sum": sum_native(post_soft),
            "hard_by_kind": dict(sorted(post_hard.items())),
            "soft_by_kind": dict(sorted(post_soft.items())),
        },
    }
    print(json.dumps(out, indent=2))
    return 0


def cmd_padding_scan(lo: int, hi: int) -> int:
    tally = json.loads(TALLY.read_text())
    total = 0
    for ch in sorted(tally["chunks"], key=lambda c: chunk_sort_key(c.get("name", ""))):
        num = chunk_number(ch.get("name", ""))
        if num is None or not (lo <= num <= hi):
            continue
        if (ch.get("bucket") or "conversion") != "conversion":
            continue
        if int(ch.get("nativeLoc", 0)) <= 0:
            continue
        for entry in audit_managed_padding(ch, num):
            print(f"{ch['name']}: {entry}")
            total += 1
    print(f"\nPadding signature hits (unallowlisted): {total}")
    return 1 if total else 0


def cmd_audit(lo: int, hi: int) -> int:
    tests = load_test_methods()
    tally = json.loads(TALLY.read_text())
    all_ranges = collect_active_conversion_ranges(tally)
    overlap_hits = find_native_range_overlaps(all_ranges)
    # Fail on overlaps among post-legacy conversion chunks (>=450); older rows may double-credit by design.
    overlap_hits = [
        h
        for h in overlap_hits
        if (n_a := chunk_number(h["a"]["chunk"])) is not None
        and (n_b := chunk_number(h["b"]["chunk"])) is not None
        and n_a >= 450
        and n_b >= 450
    ]
    if overlap_hits:
        print("Native range overlaps (conversion chunks >= 450):")
        for hit in overlap_hits:
            print(f"  FAIL {hit}")
    chunks = [c for c in tally["chunks"] if in_range(c.get("name", ""), lo, hi)]
    chunks.sort(key=lambda c: chunk_sort_key(c.get("name", "")))

    total_native = sum(int(c.get("nativeLoc", 0)) for c in chunks)
    hard_total = 0
    soft_total = 0
    print(f"Chunks {lo}-{hi}: {len(chunks)} rows, nativeLoc sum={total_native}")
    for ch in chunks:
        hard, soft = audit_chunk(ch, tests)
        if hard or soft:
            print(f"\n{ch['name']} (nativeLoc={ch.get('nativeLoc')}, bucket={ch.get('bucket')})")
            for h in hard:
                print(f"  FAIL {h}")
                hard_total += 1
            for s in soft:
                print(f"  warn {s}")
                soft_total += 1

    tools = REPO / "tools"
    tally_names = {c.get("name") for c in chunks}
    for p in sorted(tools.glob("chunk*.json")):
        m = re.match(r"chunk(\d+)(b)?\.json", p.name)
        if not m:
            continue
        n = int(m.group(1))
        if not (lo <= n <= hi):
            continue
        data = json.loads(p.read_text())
        if isinstance(data, list):
            data = data[0]
        def_name = data.get("name", "")
        json_nl = native_loc(data.get("nativeRanges") or [])
        if (data.get("bucket") or "conversion") == "conversion" and json_nl > 0:
            if def_name not in tally_names:
                print(f"\n{def_name} in {p.name} but not in loc_tally.json")
                hard_total += 1
                print(
                    f"  FAIL {{'kind': 'chunk_json_not_in_tally', 'chunk': {n}, "
                    f"'name': '{def_name}', 'nativeLoc': {json_nl}}}"
                )
                if json_nl < NATIVE_LOC_FLOOR or json_nl > NATIVE_LOC_CEILING:
                    kind = (
                        "nativeLoc_below_floor"
                        if json_nl < NATIVE_LOC_FLOOR
                        else "nativeLoc_above_ceiling"
                    )
                    print(
                        f"  FAIL {{'kind': '{kind}', 'nativeLoc': {json_nl}, "
                        f"'source': 'chunk{n}.json'}}"
                    )
                    hard_total += 1

    hard_total += len(overlap_hits)
    print(f"\nHard failures: {hard_total}, soft warnings: {soft_total}")
    return 1 if hard_total else 0


def apply_test_renames(ch: dict, renames: dict[str, str]) -> bool:
    changed = False
    for r in ch.get("nativeRanges") or []:
        tests = r.get("tests") or []
        r["tests"] = [renames.get(t, t) for t in tests]
        if r["tests"] != tests:
            changed = True
    return changed


def cmd_apply_fixes() -> None:
    tally = json.loads(TALLY.read_text())
    for ch in tally["chunks"]:
        name = ch.get("name", "")
        if name in FIX_TEST_RENAMES:
            apply_test_renames(ch, FIX_TEST_RENAMES[name])
        if FIXES.get(name) == "fix_tests_only":
            ch["note"] = (ch.get("note", "") + " (re-audit 207-245: test name fix)").strip()
    TALLY.write_text(json.dumps(tally, indent=2) + "\n")
    print("Applied FIXES to loc_tally.json")


def main() -> None:
    parser = argparse.ArgumentParser()
    sub = parser.add_subparsers(dest="cmd", required=True)
    a = sub.add_parser("audit")
    a.add_argument("--from", dest="lo", type=int, required=True)
    a.add_argument("--to", dest="hi", type=int, required=True)
    sub.add_parser("apply-fixes")
    sub.add_parser("breakdown")
    p = sub.add_parser("padding-scan")
    p.add_argument("--from", dest="lo", type=int, default=CHUNK_RULES_MIN_CHUNK)
    p.add_argument("--to", dest="hi", type=int, default=9999)
    m = sub.add_parser("map-coverage")
    m.add_argument("--from", dest="lo", type=int, default=CHUNK_RULES_MIN_CHUNK)
    m.add_argument("--to", dest="hi", type=int, default=9999)
    args = parser.parse_args()
    if args.cmd == "map-coverage":
        raise SystemExit(cmd_map_coverage(args.lo, args.hi))
    if args.cmd == "padding-scan":
        raise SystemExit(cmd_padding_scan(args.lo, args.hi))
    if args.cmd == "audit":
        raise SystemExit(cmd_audit(args.lo, args.hi))
    if args.cmd == "apply-fixes":
        cmd_apply_fixes()
    if args.cmd == "breakdown":
        raise SystemExit(cmd_breakdown())


if __name__ == "__main__":
    main()
