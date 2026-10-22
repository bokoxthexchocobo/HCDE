#!/usr/bin/env python3
"""Retire split AActor::Die chunk505 credit when landing whole Die chunk727."""
import json
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
tally = json.loads((REPO / "docs/data/loc_tally.json").read_text())

for ch in tally["chunks"]:
    if ch.get("name") == "p-interaction-actor-die-morph-chunk505":
        ch["retired"] = True
        ch["nativeRanges"] = []
        ch["nativeLoc"] = 0
        ch["managedLoc"] = 0
        ch["note"] = (ch.get("note") or "") + " Retired: whole AActor::Die absorbed by chunk727."

(REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n")
print("retired chunk505 die split credit for chunk727")
