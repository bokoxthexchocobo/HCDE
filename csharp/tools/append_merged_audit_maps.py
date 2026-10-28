#!/usr/bin/env python3
"""Append auditPaddingReview tables from chunks merged into this one (see note Merged: [...])."""
from __future__ import annotations

import json
import re
from pathlib import Path

TOOLS = Path(__file__).resolve().parent


def load(num: int) -> dict:
    data = json.loads((TOOLS / f"chunk{num}.json").read_text(encoding="utf-8"))
    return data[0] if isinstance(data, list) else data


def merged_nums(note: str) -> list[int]:
    m = re.search(r"Merged:\s*\[([^\]]+)\]", note or "")
    if not m:
        return []
    return [int(x.strip()) for x in m.group(1).split(",") if x.strip().isdigit()]


def main() -> int:
    updated = 0
    for path in sorted(TOOLS.glob("chunk*.json")):
        m = re.fullmatch(r"chunk(\d+)\.json", path.name)
        if not m:
            continue
        num = int(m.group(1))
        if num < 450:
            continue
        ch = load(num)
        if ch.get("retired"):
            continue
        nums = merged_nums(ch.get("note", ""))
        if not nums:
            continue
        audit = (ch.get("auditPaddingReview") or "").rstrip()
        for src in nums:
            src_ch = load(src)
            extra = (src_ch.get("auditPaddingReview") or "").strip()
            if not extra or extra in audit:
                continue
            audit = audit + "\n\n" + f"<!-- merged from chunk{src} -->\n" + extra
        if audit != (ch.get("auditPaddingReview") or ""):
            ch["auditPaddingReview"] = audit
            path.write_text(json.dumps([ch], indent=2) + "\n", encoding="utf-8")
            updated += 1
            print(f"chunk{num}: appended maps from {nums}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
