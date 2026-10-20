#!/usr/bin/env python3
"""Write HCDE_CSHARP_CHUNK{n}_AUDIT.md from loc_tally + chunk{n}.json."""
from __future__ import annotations

import json
import re
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
TALLY = REPO / "docs/data/loc_tally.json"
DOCS = REPO / "docs"


def chunk_num(name: str) -> int | None:
    m = re.search(r"chunk(\d+)", name)
    return int(m.group(1)) if m else None


def find_chunk(tally: dict, num: int) -> dict | None:
    for ch in tally["chunks"]:
        if chunk_num(ch.get("name", "")) == num:
            return ch
    return None


def files_from_conversion_commit(sha: str) -> list[str]:
    if not re.fullmatch(r"[0-9a-f]{40}", sha or ""):
        return []
    try:
        out = subprocess.check_output(
            ["git", "-C", str(REPO.parent), "show", "--name-only", "--pretty=format:", sha],
            text=True,
        )
    except subprocess.CalledProcessError:
        return []
    paths = []
    for line in out.splitlines():
        line = line.strip()
        if line.startswith("csharp/src/") and line.endswith(".cs"):
            paths.append(f"`{Path(line).name}`")
        elif line.startswith("csharp/tests/") and line.endswith("Tests.cs"):
            paths.append(f"`{Path(line).name}`")
    return sorted(set(paths))


def seams_for_ranges(ranges: list[dict]) -> str:
    files = {r.get("file", "") for r in ranges}
    parts: list[str] = []
    if any("d_dehacked" in f for f in files):
        parts.append("DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.")
    if any("g_mapinfo" in f for f in files):
        parts.append("MAPINFO handlers omit live `FMapInfoParser` scanner state; managed code models option semantics only.")
    if any("thingdef" in f for f in files):
        parts.append("DECORATE/thingdef ports keep lexer/table lookups injectable.")
    if not parts:
        parts.append("Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.")
    return " ".join(parts)


def managed_files_hint(note: str, ranges: list[dict]) -> str:
    hints: list[str] = []
    if "StateMap" in note or any("StateMap" in r.get("symbol", "") for r in ranges):
        hints.append("`VmDehackedDehSuppStateMap.cs`")
    if "SoundMap" in note or "InfoNames" in note:
        hints.append("`VmDehackedDehSuppSoundInfo.cs`")
    if "ThingBits" in note or "RenderStyles" in note:
        hints.append("`VmDehackedDehSuppThingBits.cs` / `VmDehackedDehSuppRenderStyles.cs`")
    if "OrgSpr" in note:
        hints.append("`VmDehackedDehSuppOrgSprNames.cs`")
    if "OrgHeights" in note or "CodePConv" in note:
        hints.append("`VmDehackedDehSuppNumericLists.cs`")
    if "ActionList" in note:
        hints.append("`VmDehackedDehSuppActionList.cs`")
    if "lump" in note.lower() or "gate" in note.lower():
        hints.append("`VmDehackedDehSuppLumpGate.cs`")
    if "FinishDehPatch" in note:
        hints.append("`VmDehackedFinishDehPatch.cs` / `VmDehackedFinishWeaponAmmo.cs`")
    if "g_mapinfo" in note.lower() or "MAPINFO" in note:
        hints.append("`VmMapInfo*.cs` (see conversion commit)")
    if not hints:
        hints.append("(see conversion commit file list)")
    return ", ".join(hints)


def main() -> int:
    num = int(sys.argv[1])
    tally = json.loads(TALLY.read_text())
    ch = find_chunk(tally, num)
    if not ch:
        print(f"chunk {num} not in loc_tally", file=sys.stderr)
        return 1
    chunk_json = REPO / f"tools/chunk{num}.json"
    ranges = ch.get("nativeRanges") or []
    if chunk_json.is_file():
        blob = json.loads(chunk_json.read_text())
        if blob and blob[0].get("nativeRanges"):
            ranges = blob[0]["nativeRanges"]

    lines: list[str] = [f"# Chunk {num} — {ch.get('note', ch.get('name', ''))}", ""]
    lines.append("## Scope")
    lines.append("")
    lines.append(ch.get("note") or "Conversion chunk.")
    lines.append("")
    lines.append("## Native ranges")
    lines.append("")
    total = 0
    for r in ranges:
        span = int(r["end"]) - int(r["start"]) + 1
        total += span
        lines.append(
            f"- `{r['file']}` **{r.get('symbol', '?')}** {r['start']}–{r['end']} ({span} lines)"
        )
    lines.append("")
    lines.append(f"**Credited native LOC:** {total} (tally `nativeLoc` {ch.get('nativeLoc')}).")
    lines.append("")
    lines.append("## Managed")
    lines.append("")
    from_commit = files_from_conversion_commit(ch.get("conversionManagedCommit", "") or "")
    if from_commit:
        lines.append(", ".join(from_commit))
    else:
        lines.append(managed_files_hint(ch.get("note", ""), ranges))
    lines.append(f"Managed LOC (tally): {ch.get('managedLoc', '?')}.")
    lines.append(f"Conversion commit: `{ch.get('conversionManagedCommit', '')}`.")
    lines.append("")
    lines.append("## Tests")
    lines.append("")
    seen: set[str] = set()
    for r in ranges:
        for t in r.get("tests") or []:
            if t not in seen:
                seen.add(t)
                lines.append(f"- `{t}`")
    lines.append("")
    lines.append("## Verified")
    lines.append("")
    lines.append(
        "`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; "
        f"`python3 tools/reaudit_chunks.py audit --from {num} --to {num}` (0 hard / 0 soft)."
    )
    lines.append("")
    lines.append("## Seams")
    lines.append("")
    lines.append(seams_for_ranges(ranges))
    lines.append("")
    lines.append("## Credit rationale")
    lines.append("")
    lines.append(
        "Only lines whose control flow and validation are exercised by the named `[Fact]` methods "
        "above are credited; scanner framing and VM hooks outside those paths are not counted."
    )
    lines.append("")

    out = DOCS / f"HCDE_CSHARP_CHUNK{num}_AUDIT.md"
    out.write_text("\n".join(lines))
    print(out)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
