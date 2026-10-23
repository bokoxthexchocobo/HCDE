#!/usr/bin/env python3
"""Read-only scan: conversion tally rows whose nativeRanges credit non-code lines.

Policy: only lines **outside function bodies** are invalid credit (headers, license,
#include, banners, blank/comment **between** functions). In-function blanks and comments
stay credited with whole-function measurement (signature through closing brace).

Does not modify loc_tally.json, chunk manifests, or reaudit gates.

Usage:
  python3 scan_tally_non_code_native_credits.py --compare
  python3 scan_tally_non_code_native_credits.py --json report.json --markdown report.md
  python3 scan_tally_non_code_native_credits.py --legacy-in-body
"""
from __future__ import annotations

import argparse
import json
import re
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
TALLY = REPO / "csharp" / "docs" / "data" / "loc_tally.json"

FULL_LINE_CPP_COMMENT = re.compile(r"^\s*//")
BANNER_MARK = re.compile(r"^\s*//\s*[=/*#\-]{3,}")
PREPROC = re.compile(r"^\s*#\s*(include|if|ifdef|ifndef|else|elif|endif|define|pragma|undef)\b")
LICENSEISH = re.compile(
    r"^\s*(//\s*)?(Copyright|License|SPDX|Permission is hereby|All rights reserved)",
    re.I,
)
SIG_SAME_LINE = re.compile(r"\)\s*\{")
SIG_CLOSE_PAREN = re.compile(r"\)\s*$")


def chunk_num_from_name(name: str) -> int | None:
    m = re.search(r"chunk(\d+)", name or "")
    return int(m.group(1)) if m else None


def _brace_delta(line: str) -> int:
    return line.count("{") - line.count("}")


def _signature_start(lines: list[str], close_paren_idx: int) -> int:
    j = close_paren_idx
    while j > 0:
        prev = lines[j - 1].strip()
        if not prev:
            break
        if prev.endswith("}") or prev.endswith("};"):
            break
        if prev.endswith(";") and "(" not in prev:
            break
        if prev.startswith("#"):
            break
        j -= 1
    return j


def function_body_line_set(lines: list[str]) -> set[int]:
    """1-based line numbers from each function signature through its closing brace.

    Only top-level (file-scope) function definitions; inner ``if (...) {`` blocks are ignored.
    """
    n = len(lines)
    bodies: set[int] = set()
    file_depth = 0
    i = 0
    while i < n:
        stripped = lines[i].strip()
        if file_depth == 0 and stripped and "(" in stripped:
            same_line = SIG_SAME_LINE.search(stripped)
            next_line_brace = (
                SIG_CLOSE_PAREN.search(stripped)
                and i + 1 < n
                and lines[i + 1].strip().startswith("{")
            )
            if same_line or next_line_brace:
                start = _signature_start(lines, i)
                func_depth = 0
                saw_brace = False
                j = start
                while j < n:
                    func_depth += _brace_delta(lines[j])
                    if "{" in lines[j]:
                        saw_brace = True
                    if saw_brace:
                        bodies.update(range(start + 1, j + 2))
                    if saw_brace and func_depth <= 0:
                        i = j + 1
                        file_depth = 0
                        break
                    j += 1
                else:
                    i = n
                continue
        file_depth += _brace_delta(stripped)
        if file_depth < 0:
            file_depth = 0
        i += 1
    return bodies


def file_scope_non_code_reason(line: str) -> str | None:
    stripped = line.strip()
    if not stripped:
        return "blank"
    if FULL_LINE_CPP_COMMENT.match(line):
        if BANNER_MARK.match(line):
            return "banner"
        if LICENSEISH.match(line):
            return "license_comment"
        return "line_comment"
    if PREPROC.match(line):
        return "preprocessor"
    if stripped.startswith("/*") or stripped.endswith("*/"):
        return "block_comment"
    return None


def classify_file_lines_legacy(lines: list[str]) -> dict[int, str]:
    """Legacy scan: blank/comment anywhere in file (includes in-function)."""
    non_code: dict[int, str] = {}
    in_block = False
    for i, raw in enumerate(lines, start=1):
        line = raw.rstrip("\n")
        stripped = line.strip()
        if not stripped:
            non_code[i] = "blank"
            continue
        if in_block:
            non_code[i] = "block_comment"
            if "*/" in stripped:
                in_block = False
            continue
        if "/*" in stripped and "*/" not in stripped.split("/*", 1)[-1]:
            non_code[i] = "block_comment"
            in_block = True
            continue
        if FULL_LINE_CPP_COMMENT.match(line):
            non_code[i] = "banner" if BANNER_MARK.match(line) else "line_comment"
            continue
        if PREPROC.match(line):
            non_code[i] = "preprocessor"
    return non_code


def symbol_suggests_non_code(symbol: str) -> bool:
    s = (symbol or "").lower()
    return any(
        k in s
        for k in (
            "separator",
            "header",
            "comment",
            "banner",
            "license",
            "include guard",
            "section gap",
            "gross",
        )
    )


def scan_tally(min_chunk: int | None, count_in_body: bool) -> dict:
    tally = json.loads(TALLY.read_text(encoding="utf-8"))
    file_lines: dict[str, list[str]] = {}
    file_bodies: dict[str, set[int]] = {}
    file_legacy: dict[str, dict[int, str]] = {}

    def load_file(path: str) -> list[str]:
        if path not in file_lines:
            fp = REPO / path
            file_lines[path] = (
                fp.read_text(encoding="utf-8", errors="replace").splitlines()
                if fp.is_file()
                else []
            )
        return file_lines[path]

    rows: list[dict] = []

    for ch in tally.get("chunks", []):
        if ch.get("bucket") != "conversion" or ch.get("retired"):
            continue
        ranges = ch.get("nativeRanges") or []
        if not ranges:
            continue
        num = chunk_num_from_name(ch.get("name", ""))
        if min_chunk is not None and (num is None or num < min_chunk):
            continue

        credited_lines: set[tuple[str, int]] = set()
        symbol_only_lines: set[tuple[str, int]] = set()
        range_meta: list[dict] = []
        for r in ranges:
            path = r.get("file", "").replace("\\", "/")
            start, end = int(r["start"]), int(r["end"])
            sym = r.get("symbol", "?")
            for ln in range(start, end + 1):
                credited_lines.add((path, ln))
            if symbol_suggests_non_code(sym):
                for ln in range(start, end + 1):
                    symbol_only_lines.add((path, ln))
            range_meta.append(
                {
                    "file": path,
                    "symbol": sym,
                    "start": start,
                    "end": end,
                    "symbol_non_code": symbol_suggests_non_code(sym),
                }
            )

        native_before = sum(int(r["end"]) - int(r["start"]) + 1 for r in ranges)
        non_code_credited: list[dict] = []

        for path, ln in sorted(credited_lines):
            lines = load_file(path)
            if count_in_body:
                if path not in file_legacy:
                    file_legacy[path] = classify_file_lines_legacy(lines)
                reason = file_legacy[path].get(ln)
            else:
                if path not in file_bodies:
                    file_bodies[path] = function_body_line_set(lines)
                if (path, ln) in symbol_only_lines:
                    reason = "symbol_range"
                elif ln in file_bodies[path]:
                    reason = None
                elif ln <= len(lines):
                    reason = file_scope_non_code_reason(lines[ln - 1])
                else:
                    reason = None
            if reason:
                non_code_credited.append({"file": path, "line": ln, "reason": reason})

        non_code_keys = {(x["file"], x["line"]) for x in non_code_credited}
        native_after = native_before - len(non_code_keys)
        if not non_code_credited:
            continue

        rows.append(
            {
                "chunk": num,
                "name": ch.get("name"),
                "nativeLoc_before": native_before,
                "nativeLoc_after_honest": native_after,
                "non_code_line_count": len(non_code_keys),
                "below_150_after": native_after < 150,
            }
        )

    rows.sort(key=lambda r: (r["chunk"] or 0, r["name"] or ""))
    return {
        "mode": "legacy_in_body" if count_in_body else "out_of_function_body_only",
        "row_count": len(rows),
        "summary": {
            "total_non_code_lines": sum(r["non_code_line_count"] for r in rows),
            "rows_below_150_after": sum(1 for r in rows if r["below_150_after"]),
        },
        "rows": rows,
    }


def analyze_manifest_ranges(ranges: list[dict], count_in_body: bool) -> dict:
    """Gross / whole-function / out-of-body for a single manifest (e.g. pre-retire git)."""
    sep = [r for r in ranges if symbol_suggests_non_code(r.get("symbol", ""))]
    fn = [r for r in ranges if r not in sep]
    gross = sum(int(r["end"]) - int(r["start"]) + 1 for r in ranges)
    whole_fn = sum(int(r["end"]) - int(r["start"]) + 1 for r in fn)
    credited: set[tuple[str, int]] = set()
    sym_lines: set[tuple[str, int]] = set()
    for r in ranges:
        path = r["file"].replace("\\", "/")
        for ln in range(int(r["start"]), int(r["end"]) + 1):
            credited.add((path, ln))
        if symbol_suggests_non_code(r.get("symbol", "")):
            for ln in range(int(r["start"]), int(r["end"]) + 1):
                sym_lines.add((path, ln))
    file_lines: dict[str, list[str]] = {}
    file_bodies: dict[str, set[int]] = {}
    out_body = 0
    for path, ln in credited:
        if path not in file_lines:
            fp = REPO / path
            file_lines[path] = (
                fp.read_text(encoding="utf-8", errors="replace").splitlines()
                if fp.is_file()
                else []
            )
        if path not in file_bodies:
            file_bodies[path] = function_body_line_set(file_lines[path])
        if (path, ln) in sym_lines:
            out_body += 1
            continue
        if ln not in file_bodies[path] and ln <= len(file_lines[path]):
            if file_scope_non_code_reason(file_lines[path][ln - 1]):
                out_body += 1
    return {
        "gross_nativeLoc": gross,
        "whole_function_nativeLoc": whole_fn,
        "out_of_body_non_code_lines": out_body,
        "separator_only_lines": gross - whole_fn,
    }


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("--json", type=Path)
    ap.add_argument("--markdown", type=Path)
    ap.add_argument("--min-chunk", type=int, default=None)
    ap.add_argument("--legacy-in-body", action="store_true")
    ap.add_argument("--compare", action="store_true")
    args = ap.parse_args()

    if args.compare:
        legacy = scan_tally(args.min_chunk, True)
        corrected = scan_tally(args.min_chunk, False)
        print(
            json.dumps(
                {
                    "legacy_in_body": {
                        "rows_affected": legacy["row_count"],
                        **legacy["summary"],
                    },
                    "corrected_out_of_body_only": {
                        "rows_affected": corrected["row_count"],
                        **corrected["summary"],
                    },
                },
                indent=2,
            )
        )
        return 0

    report = scan_tally(args.min_chunk, args.legacy_in_body)
    if args.json:
        args.json.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
        print(f"wrote {args.json}")
    if args.markdown:
        s = report["summary"]
        md = [
            f"# Scan ({report['mode']})",
            "",
            f"Rows affected: **{report['row_count']}**",
            f"Lines: **{s['total_non_code_lines']}**",
            f"Rows &lt; 150 after drop: **{s['rows_below_150_after']}**",
            "",
        ]
        args.markdown.write_text("\n".join(md) + "\n", encoding="utf-8")
        print(f"wrote {args.markdown}")
    if not args.json and not args.markdown and not args.compare:
        print(json.dumps(report["summary"], indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
