#!/usr/bin/env python3
"""Remove every padding-shaped method not in auditPaddingReview allowlist."""
from __future__ import annotations

import json
import re
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
TOOLS = REPO / "tools"
sys.path.insert(0, str(TOOLS))
from reaudit_chunks import scan_padding_methods, CHUNK_RULES_MIN_CHUNK  # noqa: E402
from strip_padding_hits import remove_methods  # noqa: E402


def main() -> int:
    removed = 0
    for path in sorted(TOOLS.glob("chunk*.json")):
        m = re.search(r"chunk(\d+)", path.name)
        if not m or int(m.group(1)) < CHUNK_RULES_MIN_CHUNK:
            continue
        data = json.loads(path.read_text())
        if isinstance(data, list):
            data = data[0]
        allow = set(re.findall(r"`([^`]+)`", data.get("auditPaddingReview") or ""))
        for rel in data.get("managedSourceFiles") or []:
            cs = REPO.parent / rel
            if not cs.is_file():
                continue
            text = cs.read_text()
            hits = scan_padding_methods(text)
            drop = {h["method"] for h in hits if h["method"] not in allow}
            if not drop:
                continue
            cs.write_text(remove_methods(text, drop))
            removed += len(drop)
    print(f"removed {removed} padding methods")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
