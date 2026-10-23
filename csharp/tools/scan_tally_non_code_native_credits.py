#!/usr/bin/env python3
"""Read-only scan: conversion tally rows whose nativeRanges credit non-code lines.

Non-code (per migration policy): blank lines; full-line // or block comments; section
banners; license / include guards; #include and other preprocessor at file scope (brace
depth 0 outside any function body).

Does not modify loc_tally.json, chunk manifests, or reaudit gates. Report only.

Usage:
  python3 scan_tally_non_code_native_credits.py
  python3 scan_tally_non_code_native_credits.py --json report.json
  python3 scan_tally_non_code_native_credits.py --min-chunk 450
"""
from __future__ import annotations

import argparse
import json
import re
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
TALLY = REPO / "csharp" / "docs" / "data" / "loc_tally.json"
TOOLS = REPO / "csharp" / "tools"

BLOCK_COMMENT_START = re.compile(r"/\*")
BLOCK_COMMENT_END = re.compile(r"\*/")
FULL_LINE_CPP_COMMENT = re.compile(r"^\s*//")
BANNER_MARK = re.compile(r"^\s*//\s*[=/*#\-]{3,}")
PREPROC = re.compile(r"^\s*#\s*(include|if|ifdef|ifndef|else|elif|endif|define|pragma|undef)\b")
LICENSEISH = re.compile(
    r"^\s*(//\s*)?(Copyright|License|SPDX|Permission is hereby|All rights reserved)",
    re.I,
)


def chunk_num_from_name(name: str) -> int | None:
    m = re.search(r"chunk(\d+)", name or "")
    return int(m.group(1)) if m else None


def classify_file_lines(lines: list[str]) -> dict[int, str]:
    """Map 1-based line number -> reason tag for non-code lines in the file."""
    non_code: dict[int, str] = {}
    depth = 0
    in_block = False
    for i, raw in enumerate(lines, start=1):
        line = raw.rstrip("\n")
        stripped = line.strip()
        if not stripped:
            non_code[i] = "blank"
            continue
        if in_block:
            non_code[i] = "block_comment"
            if BLOCK_COMMENT_END.search(stripped):
                in_block = False
            continue
        if BLOCK_COMMENT_START.search(stripped) and not stripped.endswith("*/"):
            non_code[i] = "block_comment"
            if "*/" not in stripped[stripped.find("/*") + 2 :]:
                in_block = True
            continue
        if FULL_LINE_CPP_COMMENT.match(line):
            if BANNER_MARK.match(line):
                non_code[i] = "banner"
            elif LICENSEISH.match(line):
                non_code[i] = "license_comment"
            else:
                non_code[i] = "line_comment"
            continue
        if depth == 0 and PREPROC.match(line):
            non_code[i] = "preprocessor"
            continue
        # brace depth (crude; skips strings)
        for ch in stripped:
            if ch == "{":
                depth += 1
            elif ch == "}":
                depth = max(0, depth - 1)
    return non_code


def symbol_suggests_non_code(symbol: str) -> bool:
    s = (symbol or "").lower()
    keys = (
        "separator",
        "header",
        "comment",
        "banner",
        "license",
        "include guard",
        "section gap",
        "gross",
    )
    return any(k in s for k in keys)


def scan_tally(min_chunk: int | None) -> dict:
    tally = json.loads(TALLY.read_text(encoding="utf-8"))
    file_cache: dict[str, dict[int, str]] = {}
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
        range_meta: list[dict] = []
        for r in ranges:
            path = r.get("file", "").replace("\\", "/")
            start, end = int(r["start"]), int(r["end"])
            sym = r.get("symbol", "?")
            for ln in range(start, end + 1):
                credited_lines.add((path, ln))
            range_meta.append(
                {
                    "file": path,
                    "symbol": sym,
                    "start": start,
                    "end": end,
                    "span": end - start + 1,
                    "symbol_non_code": symbol_suggests_non_code(sym),
                }
            )

        native_before = sum(end - start + 1 for start, end in [(int(r["start"]), int(r["end"])) for r in ranges])

        non_code_credited: list[dict] = []
        for path, ln in sorted(credited_lines):
            if path not in file_cache:
                fp = REPO / path
                if not fp.is_file():
                    file_cache[path] = {}
                else:
                    file_cache[path] = classify_file_lines(
                        fp.read_text(encoding="utf-8", errors="replace").splitlines()
                    )
            reason = file_cache[path].get(ln)
            if reason:
                non_code_credited.append({"file": path, "line": ln, "reason": reason})

        # whole ranges flagged by symbol name (entire span non-code credit)
        for rm in range_meta:
            if rm["symbol_non_code"]:
                for ln in range(rm["start"], rm["end"] + 1):
                    key = (rm["file"], ln)
                    if key in credited_lines and not any(
                        x["file"] == rm["file"] and x["line"] == ln for x in non_code_credited
                    ):
                        non_code_credited.append(
                            {"file": rm["file"], "line": ln, "reason": "symbol_range"}
                        )

        non_code_keys = {(x["file"], x["line"]) for x in non_code_credited}
        native_after = native_before - len(non_code_keys & credited_lines)

        if not non_code_credited:
            continue

        rows.append(
            {
                "chunk": num,
                "name": ch.get("name"),
                "nativeLoc_before": native_before,
                "nativeLoc_after_honest": native_after,
                "non_code_line_count": len(non_code_keys & credited_lines),
                "below_150_after": native_after < 150,
                "ranges": range_meta,
                "non_code_lines": sorted(non_code_credited, key=lambda x: (x["file"], x["line"])),
            }
        )

    rows.sort(key=lambda r: (r["chunk"] or 0, r["name"] or ""))
    return {
        "tally_path": str(TALLY.relative_to(REPO)),
        "row_count": len(rows),
        "rows": rows,
        "summary": {
            "total_non_code_lines": sum(r["non_code_line_count"] for r in rows),
            "rows_below_150_after": sum(1 for r in rows if r["below_150_after"]),
        },
    }


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("--json", type=Path, help="Write full JSON report to this path")
    ap.add_argument("--min-chunk", type=int, default=None, help="Only chunk numbers >= N")
    ap.add_argument("--markdown", type=Path, help="Write human summary markdown")
    args = ap.parse_args()

    report = scan_tally(args.min_chunk)

    if args.json:
        args.json.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
        print(f"wrote {args.json}")

    if args.markdown:
        lines = [
            "# Non-code native credit scan (read-only)",
            "",
            f"Rows with credited non-code lines: **{report['row_count']}**.",
            f"Total credited non-code lines (union per row): **{report['summary']['total_non_code_lines']}**.",
            f"Rows with honest `nativeLoc` &lt; 150: **{report['summary']['rows_below_150_after']}**.",
            "",
        ]
        for r in report["rows"]:
            lines.append(f"## Chunk {r['chunk']} — `{r['name']}`")
            lines.append("")
            lines.append(
                f"| `nativeLoc` before | after (drop non-code) | non-code lines | retire if cleaned? |"
            )
            lines.append(
                f"|-------------------:|----------------------:|---------------:|:------------------:|"
            )
            lines.append(
                f"| {r['nativeLoc_before']} | {r['nativeLoc_after_honest']} | {r['non_code_line_count']} | "
                f"{'**yes**' if r['below_150_after'] else 'no'} |"
            )
            lines.append("")
            by_reason: dict[str, int] = {}
            for x in r["non_code_lines"]:
                by_reason[x["reason"]] = by_reason.get(x["reason"], 0) + 1
            lines.append("Reasons: " + ", ".join(f"{k}={v}" for k, v in sorted(by_reason.items())))
            lines.append("")
        args.markdown.write_text("\n".join(lines) + "\n", encoding="utf-8")
        print(f"wrote {args.markdown}")

    if not args.json and not args.markdown:
        print(json.dumps(report["summary"], indent=2))
        print(f"rows: {report['row_count']} (use --json for detail)")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
