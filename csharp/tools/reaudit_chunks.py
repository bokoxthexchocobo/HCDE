#!/usr/bin/env python3
"""Test-backed loc_tally re-audit for a chunk number range.

Usage:
  python3 reaudit_chunks.py audit --from 207 --to 245
  python3 reaudit_chunks.py apply-fixes   # applies FIXES dict to loc_tally.json

Audit fails (hard) when:
  - conversion chunk nativeLoc != sum(nativeRanges)
  - conversion chunk with nativeLoc > 0 lacks conversionManagedCommit SHA
  - any range lists a test method that does not exist in *Tests.cs
  - conversion range has zero tests

Audit warns (soft, not auto-trimmed) when:
  - range span >= 60 and only one test (thin band — manual review)
  - conversion chunk: managed LOC from conversionManagedCommit src/*.cs additions
    is under 50% of credited nativeLoc (thin_managed_coverage)
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
from chunk_port_managed import managed_loc_for_chunk  # noqa: E402

# Manual trims / fixes — only rows that fail audit or are verified over-credits.
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


def measured_port_managed(ch: dict) -> int:
    sha = ch.get("conversionManagedCommit", "") or ""
    return managed_loc_for_chunk(ch, sha, chunk_number(ch.get("name", "")))


def audit_chunk(ch: dict, tests: set[str]) -> tuple[list[dict], list[dict]]:
    hard: list[dict] = []
    soft: list[dict] = []
    name = ch.get("name", "")
    bucket = ch.get("bucket") or "conversion"
    nl = int(ch.get("nativeLoc", 0))
    ranges = ch.get("nativeRanges") or []
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
        commit_ml = measured_port_managed(ch)
        tally_ml = int(ch.get("managedLoc", 0))
        if commit_ml > 0 and tally_ml != commit_ml:
            hard.append(
                {
                    "kind": "managedLoc_mismatch",
                    "managedLoc_measured": commit_ml,
                    "managedLoc_tally": tally_ml,
                }
            )
        measured = commit_ml if commit_ml > 0 else tally_ml
        if measured > 0 and measured < nl * 0.5:
            soft.append(
                {
                    "kind": "thin_managed_coverage",
                    "nativeLoc": nl,
                    "managedLoc_measured": commit_ml,
                    "managedLoc_tally": tally_ml,
                    "ratio": round(measured / nl, 3),
                }
            )

    return hard, soft


def cmd_audit(lo: int, hi: int) -> int:
    tests = load_test_methods()
    tally = json.loads(TALLY.read_text())
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
    for p in sorted(tools.glob("chunk*.json")):
        m = re.match(r"chunk(\d+)(b)?\.json", p.name)
        if not m:
            continue
        n = int(m.group(1))
        if not (lo <= n <= hi):
            continue
        data = json.loads(p.read_text())
        for ch in data:
            if not any(c.get("name") == ch.get("name") for c in chunks):
                print(f"\nNOTE: {ch.get('name')} in {p.name} but not in loc_tally.json")

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
    args = parser.parse_args()
    if args.cmd == "audit":
        raise SystemExit(cmd_audit(args.lo, args.hi))
    if args.cmd == "apply-fixes":
        cmd_apply_fixes()


if __name__ == "__main__":
    main()
