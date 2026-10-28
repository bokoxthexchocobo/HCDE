#!/usr/bin/env python3
"""Remove padding-shaped methods from a managed file (see reaudit_chunks.scan_padding_methods)."""
from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

TOOLS = Path(__file__).resolve().parent
sys.path.insert(0, str(TOOLS))
from reaudit_chunks import scan_padding_methods  # noqa: E402


def remove_methods(cs_text: str, method_names: set[str]) -> str:
    out = cs_text
    for name in sorted(method_names, key=len, reverse=True):
        expr = re.compile(
            rf"(?m)^[ \t]*(?:public|private|internal|protected)[^\n]*\b{name}\s*\([^)]*\)\s*=>\s*[^;]+;\s*\n?",
        )
        out, n = expr.subn("", out)
        if n:
            continue
        block = re.compile(
            rf"(?m)^[ \t]*(?:public|private|internal|protected)[^\n]*\b{name}\s*\([^)]*\)\s*\{{[^{{}}]*\}}\s*\n?",
        )
        out, _ = block.subn("", out)
    return out


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("cs_path")
    ap.add_argument("--allow", nargs="*", default=[], help="method names to keep even if padding")
    args = ap.parse_args()
    path = Path(args.cs_path)
    text = path.read_text()
    allow = set(args.allow)
    hits = scan_padding_methods(text)
    remove = {h["method"] for h in hits if h["method"] not in allow}
    if not remove:
        print("no padding methods to remove")
        return 0
    new_text = remove_methods(text, remove)
    path.write_text(new_text)
    print(f"removed {len(remove)} methods from {path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
