#!/usr/bin/env python3
"""Recompute loc_tally.json headline fields from chunk entries (nativeRanges sums + bucket totals)."""
import json
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(REPO / "tools"))
from generate_native_managed_matrix import sync_loc_tally  # noqa: E402

def main() -> int:
    path = REPO / "docs/data/loc_tally.json"
    tally = json.loads(path.read_text(encoding="utf-8"))
    tally = sync_loc_tally(tally)
    path.write_text(json.dumps(tally, indent=2) + "\n", encoding="utf-8")
    print(
        f"synced convertedBehaviorNative={tally['convertedBehaviorNative']} "
        f"recognitionBehaviorNative={tally['recognitionBehaviorNative']} "
        f"remainingNative={tally['remainingNative']}"
    )
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
