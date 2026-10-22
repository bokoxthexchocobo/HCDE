#!/usr/bin/env python3
"""Zero the retired p-acs-clientside-script-chunk730 tally row when landing chunk730."""
import json
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
OLD = "p-acs-clientside-script-chunk730"

chunk_path = REPO / "tools" / "chunk730_old_clientside.json"
if not chunk_path.is_file():
    data = [
        {
            "name": OLD,
            "bucket": "conversion",
            "note": "Retired: replaced by p-acs-behavior-init-chunk730 (whole FBehavior::Init).",
            "retired": True,
            "nativeRanges": [],
            "managedSourceFiles": [],
        }
    ]
    chunk_path.write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")

tally = json.loads((REPO / "docs/data/loc_tally.json").read_text())
tally["chunks"] = [ch for ch in tally["chunks"] if ch.get("name") != OLD]
(REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n", encoding="utf-8")
print(f"retired tally row {OLD}")
