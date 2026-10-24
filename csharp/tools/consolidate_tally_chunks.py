#!/usr/bin/env python3
"""Merge tally rows by chunk numbers (ranges concatenated, nativeLoc summed, one row kept)."""
import json
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
tally_path = REPO / "docs/data/loc_tally.json"


def main() -> int:
    keep_num = int(sys.argv[1])
    merge_nums = [int(x) for x in sys.argv[2:]]
    tally = json.loads(tally_path.read_text())

    def find(n: int):
        for i, ch in enumerate(tally["chunks"]):
            if f"chunk{n}" in ch.get("name", ""):
                return i, ch
        raise SystemExit(f"chunk {n} not found")

    keep_i, keep = find(keep_num)
    merged = [keep]
    for n in merge_nums:
        _, ch = find(n)
        merged.append(ch)

    combined_ranges = []
    for ch in merged:
        combined_ranges.extend(ch.get("nativeRanges") or [])

    keep["nativeRanges"] = combined_ranges
    keep["nativeLoc"] = sum(int(r["end"]) - int(r["start"]) + 1 for r in combined_ranges)
    keep["managedLoc"] = sum(int(ch.get("managedLoc") or 0) for ch in merged)
    # Latest conversion commit among merged rows
    keep["conversionManagedCommit"] = merged[-1].get("conversionManagedCommit") or keep.get(
        "conversionManagedCommit"
    )
    keep["note"] = (
        "D_DoomMain_Internal restart loop: IWAD pass, debug listen, D_DoomLoop, "
        "SignalHandler, and GameMain (consolidated chunk445–448)."
    )
    keep["name"] = f"d-doommain-restart-loop-chunk{keep_num}"
    keep["auditSeams"] = (
        "`FIWadManager`, `D_InitGame`, `DebugServer`, `D_DoomLoop`, `DisplayBackend`, "
        "and shutdown I/O are not hosted."
    )
    keep["auditRationale"] = (
        "Credits contiguous `d_main.cpp` 4658–4880: class/type static init, IWAD/mod resolution, "
        "shareware and SteamAppId gates, debug-port policy, doom-loop restart, signal handler, "
        "and `GameMain` entry from existing Vm ports."
    )

    remove_names = {merged[i]["name"] for i in range(1, len(merged))}
    tally["chunks"] = [ch for ch in tally["chunks"] if ch.get("name") not in remove_names]
    tally_path.write_text(json.dumps(tally, indent=2) + "\n")
    print(f"consolidated into {keep['name']} nativeLoc={keep['nativeLoc']} managedLoc={keep['managedLoc']}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
