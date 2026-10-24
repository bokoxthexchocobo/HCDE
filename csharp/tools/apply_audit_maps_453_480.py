#!/usr/bin/env python3
"""Apply audit_padding_453_480.json to chunk JSON (statement-level maps only)."""
from __future__ import annotations

import json
from pathlib import Path

TOOLS = Path(__file__).resolve().parent
AUDIT_PATH = TOOLS / "audit_padding_453_480.json"


def main() -> int:
    audits = json.loads(AUDIT_PATH.read_text(encoding="utf-8"))
    for key, table in audits.items():
        num = int(key)
        path = TOOLS / f"chunk{num}.json"
        if not path.is_file():
            continue
        data = json.loads(path.read_text(encoding="utf-8"))
        ch = data[0] if isinstance(data, list) else data
        if ch.get("retired"):
            continue
        ch["auditPaddingReview"] = table.strip() + "\n"
        path.write_text(json.dumps([ch], indent=2) + "\n", encoding="utf-8")
        print(f"updated chunk{num}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
