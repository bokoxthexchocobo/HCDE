#!/usr/bin/env python3
"""Index DEFINE_ACTION_FUNCTION names from p_actionfunctions.cpp."""

from __future__ import annotations

import json
import re
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
SRC = REPO / "src/playsim/p_actionfunctions.cpp"
OUT = REPO / "csharp/docs/data/action_function_index.json"


def main() -> None:
    text = SRC.read_text(encoding="utf-8", errors="replace")
    names = sorted(set(re.findall(r"DEFINE_ACTION_FUNCTION\([^,]+,\s*(\w+)", text)))
    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text(json.dumps({"source": str(SRC.relative_to(REPO)), "count": len(names), "names": names}, indent=2) + "\n")
    print(f"wrote {OUT} ({len(names)} actions)")


if __name__ == "__main__":
    main()
