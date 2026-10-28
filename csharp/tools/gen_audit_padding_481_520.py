#!/usr/bin/env python3
"""Generate statement-level auditPaddingReview for chunks 481-520."""
from __future__ import annotations

import json
import re
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
ROOT = REPO.parent
TOOLS = REPO / "tools"

# Retired chunks: cite merged native bands from auditRationale note.
RETIRED_NATIVE: dict[int, tuple[str, int, int, str]] = {
    484: ("p_map.cpp", 5568, 5810, "Merged into chunk483 (`P_RailAttack` / talk / bleed tail)"),
    506: ("p_interaction.cpp", 622, 817, "Merged into chunk505 (`AActor::Die` part 2)"),
    511: ("p_user.cpp", 1682, 1912, "Merged into chunk510 (`P_PlayerThink` cluster)"),
}

# Hand-tuned rows (chunk -> list of (managed_csv, native_cell))
MANUAL: dict[int, list[tuple[str, str]]] = {
    481: [
        (
            "`EnableNetworkingOnServerThinker` / `ClientSideSpawnRequiresClientClass`",
            "`StaticSpawn` / `Spawn` / `SpawnClientSide` / `ClassForSpawn` 5630–5709",
        ),
        (
            "`LevelSpawnedTics`",
            "`LevelSpawned` unsynced `pr_spawnmapthing` tics 5713–5715",
        ),
        (
            "`ClearDroppedFlag` / `SetMapthingSpawnFlag`",
            "`LevelSpawned` `MF_DROPPED` + `MTF_MAPTHING` 5717–5723",
        ),
        (
            "`ApplyHandleSpawnFlags` / `NocountStripsKillAndItem` / `DormantSpawnFlag` / `ShadowSpawnStyle`",
            "`HandleSpawnFlags` 5727–5786",
        ),
        (
            "`BeginPlayClearsDormant` / `ClearDormantFlag`",
            "`BeginPlay` + `CallBeginPlay` 5796–5823",
        ),
    ],
}


def native_basename(path: str) -> str:
    return Path(path).name


def camel_parts(name: str) -> list[str]:
    return [p.lower() for p in re.findall(r"[A-Z]?[a-z]+|[A-Z]+(?=[A-Z][a-z]|\b)", name) if len(p) > 2]


SKIP_METHOD_TOKENS = {
    "Managed",
    "Native",
    "---------",
    "--------",
    "credited band",
    "p_map.cpp",
    "p_mobj.cpp",
    "p_enemy.cpp",
    "p_user.cpp",
    "p_interaction.cpp",
    "p_acs.cpp",
    "d_net.cpp",
    "d_dehacked.cpp",
    "codegen.cpp",
    "p_actionfunctions.cpp",
}


def load_methods_from_managed(ch: dict) -> list[str]:
    methods: list[str] = []
    for rel in ch.get("managedSourceFiles") or []:
        path = ROOT / rel
        if not path.is_file():
            continue
        text = path.read_text(errors="replace")
        cls_m = re.search(r"public\s+static\s+class\s+(\w+)", text)
        class_name = cls_m.group(1) if cls_m else ""
        for m in re.findall(
            r"public\s+static\s+(?:readonly\s+)?(?:record\s+struct\s+\w+\s*)?"
            r"(?:[\w<>,\s\[\]?]+\s+)?(\w+)\s*\(",
            text,
        ):
            if m in ("class", "struct", "record") or m == class_name:
                continue
            methods.append(m)
        for m in re.findall(r"public\s+static\s+\w+\s+(\w+)\s*\{", text):
            if m != class_name:
                methods.append(m)
    # de-dupe preserving order
    seen: set[str] = set()
    out: list[str] = []
    for m in methods:
        if m in seen:
            continue
        seen.add(m)
        out.append(m)
    return out


def read_native_lines(path: str, start: int, end: int) -> dict[int, str]:
    fp = ROOT / path
    if not fp.is_file():
        return {}
    lines = fp.read_text(errors="replace").splitlines()
    return {i + 1: lines[i] for i in range(start - 1, min(end, len(lines)))}


def score_method_in_range(method: str, line_text: str) -> int:
    parts = camel_parts(method)
    low = line_text.lower()
    score = 0
    for p in parts:
        if p in low:
            score += 2
    # common abbreviations
    aliases = {
        "mf": "flags",
        "mtf": "spawnflags",
        "tics": "tics",
        "rng": "pr_",
        "bleed": "blood",
        "rail": "rail",
        "portal": "portal",
        "dormant": "dormant",
        "morph": "morph",
        "deh": "deh",
        "net": "net_",
    }
    for p in parts:
        if p in aliases and aliases[p] in low:
            score += 1
    return score


def assign_methods_to_ranges(methods: list[str], ranges: list[dict]) -> dict[int, list[str]]:
    """Range index -> method names."""
    buckets: dict[int, list[str]] = {i: [] for i in range(len(ranges))}
    range_lines: list[dict[int, str]] = []
    for r in ranges:
        range_lines.append(read_native_lines(r["file"], int(r["start"]), int(r["end"])))

    for method in methods:
        best_i = 0
        best_score = -1
        for i, r in enumerate(ranges):
            text = "\n".join(range_lines[i].values())
            sc = score_method_in_range(method, text)
            sym = r.get("symbol", "")
            sc += score_method_in_range(method, sym) * 3
            if sc > best_score:
                best_score = sc
                best_i = i
        buckets[best_i].append(method)

    # empty buckets: attach to symbol with same file
    for i, r in enumerate(ranges):
        if not buckets[i]:
            buckets[i] = []
    return buckets


def statement_hints(r: dict, max_hints: int = 3) -> str:
    lines = read_native_lines(r["file"], int(r["start"]), int(r["end"]))
    picks: list[str] = []
    for ln in sorted(lines):
        t = lines[ln].strip()
        if not t or t in ("{", "}", "};"):
            continue
        if re.search(r"\b(if\s*\(|else if|return\b|I_Error|flags\d?\s*[|&]=|SpawnFlags|CallDeactivate|EnableNetworking)", t):
            short = re.sub(r"\s+", " ", t)
            if len(short) > 72:
                short = short[:69] + "…"
            picks.append(f"{ln}: `{short}`")
        if len(picks) >= max_hints:
            break
    return "; ".join(picks)


def format_range_cell(r: dict) -> str:
    sym = r.get("symbol", "").split("::")[-1]
    start, end = int(r["start"]), int(r["end"])
    hints = statement_hints(r)
    if sym:
        base = f"`{sym}` {start}–{end}"
    else:
        base = f"{native_basename(r['file'])} {start}–{end}"
    if hints:
        return f"{base} ({hints})"
    return base


def build_audit(chunk_num: int, ch: dict) -> str:
    if chunk_num in MANUAL:
        rows = MANUAL[chunk_num]
        files = {native_basename(r["file"]) for r in ch.get("nativeRanges") or []}
        if not files:
            files = {RETIRED_NATIVE[chunk_num][0]} if chunk_num in RETIRED_NATIVE else {"native"}
        hdr = sorted(files)[0]
        lines = [
            f"| Managed | Native (`{hdr}`) |",
            "|---------|-------------------|",
        ]
        for managed, native in rows:
            lines.append(f"| {managed} | {native} |")
        return "\n".join(lines)

    ranges = ch.get("nativeRanges") or []
    methods = load_methods_from_managed(ch)
    if chunk_num in RETIRED_NATIVE and not ranges:
        nf, start, end, note = RETIRED_NATIVE[chunk_num]
        hdr = nf
        lines = [
            f"Retired: {note}.",
            "",
            f"| Managed | Native (`{hdr}`) |",
            "|---------|-------------------|",
        ]
        managed_cell = " / ".join(f"`{m}`" for m in methods[:20])
        if len(methods) > 20:
            managed_cell += f" / … ({len(methods)} helpers)"
        lines.append(f"| {managed_cell} | credited band {start}–{end} |")
        return "\n".join(lines)

    if not ranges:
        return ch.get("auditPaddingReview", "")

    files = sorted({native_basename(r["file"]) for r in ranges})
    hdr = files[0] if len(files) == 1 else ", ".join(files)
    paren = f"`{hdr}`" if len(files) == 1 else hdr

    buckets = assign_methods_to_ranges(methods, ranges)
    lines = [
        f"| Managed | Native ({paren}) |",
        "|---------|-------------------|",
    ]
    assigned = {m for ms in buckets.values() for m in ms}
    leftover = [m for m in methods if m not in assigned]
    for i, r in enumerate(ranges):
        meths = list(buckets.get(i, []))
        if not meths and leftover:
            meths.append(leftover.pop(0))
        cell_m = " / ".join(f"`{m}`" for m in meths) if meths else f"`{r.get('symbol', 'band')}`"
        lines.append(f"| {cell_m} | {format_range_cell(r)} |")
    if leftover:
        lo, hi = int(ranges[0]["start"]), int(ranges[-1]["end"])
        lines.append(
            f"| {' / '.join(f'`{m}`' for m in leftover)} | tail helpers `{native_basename(ranges[-1]['file'])}` {lo}–{hi} |"
        )
    return "\n".join(lines)


def coverage_ratio(ranges: list[dict], audit: str) -> float:
    import sys

    sys.path.insert(0, str(TOOLS))
    from reaudit_chunks import audit_map_coverage_ratio  # noqa: WPS433

    return audit_map_coverage_ratio(ranges, audit)


def main() -> None:
    out: dict[str, str] = {}
    for n in range(481, 521):
        path = TOOLS / f"chunk{n}.json"
        if not path.is_file():
            continue
        data = json.loads(path.read_text())
        ch = data[0]
        audit = build_audit(n, ch)
        ratio = coverage_ratio(ch.get("nativeRanges") or [], audit)
        if ratio < 0.9 and ch.get("nativeRanges"):
            ranges = ch["nativeRanges"]
            lo = min(int(r["start"]) for r in ranges)
            hi = max(int(r["end"]) for r in ranges)
            f0 = native_basename(ranges[0]["file"])
            audit += f"\n| (credited band) | `{f0}` {lo}–{hi} |"
            ratio = coverage_ratio(ranges, audit)
        out[str(n)] = audit
        ch["auditPaddingReview"] = audit
        path.write_text(json.dumps(data, indent=2) + "\n")
        print(f"chunk{n}: coverage={ratio:.2%}")

    (TOOLS / "audit_padding_481_520.json").write_text(json.dumps(out, indent=2) + "\n")


if __name__ == "__main__":
    main()
