#!/usr/bin/env python3
import json
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
chunk_file = sys.argv[2] if len(sys.argv) > 2 else "append_chunk.json"
chunks = json.loads((REPO / "tools" / chunk_file).read_text())
idx = int(sys.argv[1])
chunk = chunks[idx]
chunk["nativeLoc"] = sum(r["end"] - r["start"] + 1 for r in chunk["nativeRanges"])
path = REPO / "docs/data/loc_tally.json"
tally = json.loads(path.read_text())
if any(c.get("name") == chunk["name"] for c in tally["chunks"]):
    raise SystemExit(f"duplicate {chunk['name']}")
tally["chunks"].append(chunk)
path.write_text(json.dumps(tally, indent=2) + "\n")
