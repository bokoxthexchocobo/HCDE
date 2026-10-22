#!/usr/bin/env python3
"""Trim overlapping DamageMobj bands from chunk474 when landing chunk729."""
import json
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
TRIM_LO, TRIM_HI = 1281, 1571


def trim_ranges(ranges):
    kept = []
    for nr in ranges:
        if nr.get("symbol") != "DamageMobj":
            kept.append(nr)
            continue
        if nr["end"] < TRIM_LO or nr["start"] > TRIM_HI:
            kept.append(nr)
    return kept


def patch_chunk(path: Path):
    data = json.loads(path.read_text())
    entry = data[0] if isinstance(data, list) else data
    entry["nativeRanges"] = trim_ranges(entry.get("nativeRanges") or [])
    entry["nativeLoc"] = sum(int(r["end"]) - int(r["start"]) + 1 for r in entry["nativeRanges"])
    entry["managedSourceFiles"] = [
        "csharp/src/HCDE.Playsim/VmPInteractionVirtualDamage.cs"
    ]
    entry["note"] = (entry.get("note") or "") + " Trimmed DamageMobj 1281–1571 after chunk729 whole-function port."
    entry["auditPaddingReview"] = (
        "| Managed file | Native (`p_interaction.cpp`) |\n"
        "|--------------|---------------------------|\n"
        "| `VmPInteractionVirtualDamage.cs` | `DoDamageMobj` 1573–1603, `P_DamageMobj` 1617–1657, "
        "`AActor::PoisonMobj` action 1715–1726 |\n"
    )
    path.write_text(json.dumps(data, indent=2) + "\n")


def patch_tally():
    tally = json.loads((REPO / "docs/data/loc_tally.json").read_text())
    for ch in tally["chunks"]:
        if ch.get("name") != "p-interaction-damage-late-chunk474":
            continue
        ch["nativeRanges"] = trim_ranges(ch.get("nativeRanges") or [])
        ch["nativeLoc"] = sum(int(r["end"]) - int(r["start"]) + 1 for r in ch["nativeRanges"])
        ch["managedSourceFiles"] = [
            "csharp/src/HCDE.Playsim/VmPInteractionVirtualDamage.cs"
        ]
        ch["note"] = (ch.get("note") or "") + " Trimmed DamageMobj 1281–1571 after chunk729."
        ch["auditPaddingReview"] = (
            "| Managed | Native (`p_interaction.cpp`) |\n"
            "|---------|---------------------------|\n"
            "| `EvaluateDoDamageMobjMirror` | `DoDamageMobj` 1573–1603 |\n"
            "| `EvaluatePDamageMobjEarly` | `P_DamageMobj` 1617–1657 |\n"
            "| `PoisonMobjRequiresInflictor` | `AActor::PoisonMobj` 1715–1726 |\n"
        )
    (REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n")


patch_chunk(REPO / "tools" / "chunk474.json")
patch_tally()
print("trimmed p-interaction-damage-late-chunk474 DamageMobj 1281-1571")
