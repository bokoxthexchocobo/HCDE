#!/usr/bin/env python3
"""Retire chunk531 ACS string pool credits superseded by chunks 789-790."""
import json
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
for path in [REPO / "tools/chunk531.json", REPO / "docs/data/loc_tally.json"]:
    data = json.loads(path.read_text())
    if path.name == "chunk531.json":
        entry = data[0] if isinstance(data, list) else data
        entry["retired"] = True
        entry["nativeRanges"] = []
        entry["managedSourceFiles"] = []
        entry["note"] = (entry.get("note") or "") + " Retired: string pool bands moved to chunks 789-790."
        path.write_text(json.dumps(data, indent=2) + "\n")
        continue

    for ch in data["chunks"]:
        if ch.get("name") == "p-acs-string-pool-chunk531":
            ch["retired"] = True
            ch["nativeRanges"] = []
            ch["nativeLoc"] = 0
            ch["managedLoc"] = 0
            ch["managedSourceFiles"] = []
            ch["note"] = (ch.get("note") or "") + " Retired after chunks 789-790."
    path.write_text(json.dumps(data, indent=2) + "\n")

print("retired chunk531 string pool overlap")
