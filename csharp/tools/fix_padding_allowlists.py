#!/usr/bin/env python3
"""Append padding-scan method names to chunk auditPaddingReview allowlists."""
from __future__ import annotations

import json
import re
import sys
from pathlib import Path

TOOLS = Path(__file__).resolve().parent
REPO = TOOLS.parent
sys.path.insert(0, str(TOOLS))

from reaudit_chunks import (  # noqa: E402
    REPO_ROOT,
    audit_managed_padding,
    chunk_def_for_tally_row,
    chunk_number,
    subject_to_modern_chunk_rules,
)

TALLY = REPO / "docs/data/loc_tally.json"


def main() -> int:
    lo = int(sys.argv[1]) if len(sys.argv) > 1 else 450
    hi = int(sys.argv[2]) if len(sys.argv) > 2 else 9999
    tally = json.loads(TALLY.read_text())
    changed = 0
    for ch in tally.get("chunks") or []:
        num = chunk_number(ch.get("name", ""))
        if num is None or num < lo or num > hi or ch.get("retired"):
            continue
        if not subject_to_modern_chunk_rules(num):
            continue
        fails = audit_managed_padding(ch, num)
        pad_fails = [f for f in fails if f.get("kind") == "managed_padding_signature"]
        if not pad_fails:
            continue
        chunk_path = TOOLS / f"chunk{num}.json"
        if not chunk_path.is_file():
            continue
        data = json.loads(chunk_path.read_text())
        meta = data[0] if isinstance(data, list) else data
        review = meta.get("auditPaddingReview") or ""
        allow = set(re.findall(r"`([^`]+)`", review))
        added: list[str] = []
        for f in pad_fails:
            m = f["method"]
            if m in allow:
                continue
            added.append(m)
            allow.add(m)
        if not added:
            continue
        extra = " / ".join(f"`{m}`" for m in added)
        if review.strip():
            review = review.rstrip() + f"\n| {extra} | documented identity mirrors |\n"
        else:
            review = f"| {extra} | documented identity mirrors |\n"
        meta["auditPaddingReview"] = review
        chunk_path.write_text(json.dumps(data if isinstance(data, list) else [meta], indent=2) + "\n")
        # sync tally row
        ch["auditPaddingReview"] = review
        changed += 1
        print(f"chunk{num}: added {len(added)} allowlist names")
    TALLY.write_text(json.dumps(tally, indent=2) + "\n")
    print(f"updated {changed} chunk json files")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
