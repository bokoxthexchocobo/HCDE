#!/usr/bin/env python3
"""Per-chunk counts of removed public static methods from padding-strip commits."""
from __future__ import annotations

import json
import re
import subprocess
from collections import defaultdict
from pathlib import Path

REPO = Path(__file__).resolve().parents[1].parent
CSHARP = REPO / "csharp"
BASE = "e53a221b^"  # before last padding purge landing commit on branch


def removed_methods_in_file(path: str) -> int:
    try:
        diff = subprocess.check_output(
            ["git", "diff", BASE, "HEAD", "--", path],
            cwd=REPO,
            text=True,
        )
    except subprocess.CalledProcessError:
        return 0
    removed = 0
    for line in diff.splitlines():
        if line.startswith("-") and not line.startswith("---"):
            if re.search(r"public static .+\(", line):
                removed += 1
    return removed


def main() -> None:
    tally = json.loads((CSHARP / "docs/data/loc_tally.json").read_text())
    by_chunk: dict[str, int] = {}
    by_file: dict[str, int] = {}
    for ch in tally["chunks"]:
        m = re.search(r"chunk(\d+)$", ch["name"])
        if not m or int(m.group(1)) < 450:
            continue
        if ch.get("retired"):
            continue
        total = 0
        for rel in ch.get("managedSourceFiles") or []:
            path = rel if rel.startswith("csharp/") else f"csharp/{rel}"
            n = removed_methods_in_file(path)
            by_file[path] = n
            total += n
        if total:
            by_chunk[ch["name"]] = total
    top = sorted(by_chunk.items(), key=lambda x: -x[1])[:10]
    out = {
        "base": BASE,
        "chunks_with_removals": len(by_chunk),
        "total_removed_method_lines": sum(by_chunk.values()),
        "per_chunk": dict(sorted(by_chunk.items(), key=lambda x: -x[1])),
        "top10": top,
    }
    print(json.dumps(out, indent=2))


if __name__ == "__main__":
    main()
