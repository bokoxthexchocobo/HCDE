#!/usr/bin/env python3
"""Generate native-to-managed support matrix artifacts under csharp/docs/."""

from __future__ import annotations

import json
import re
import sys
from collections import defaultdict
from dataclasses import dataclass
from datetime import date
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
CSHARP = REPO / "csharp"
SRC = REPO / "src"
DOCS = CSHARP / "docs"
DATA = DOCS / "data"

STATUS_EXECUTED = "executed"
STATUS_PARTIAL = "partial"
STATUS_DECODED = "decoded-only"
STATUS_ABSENT = "absent"


@dataclass
class LocScope:
    label: str
    native_loc: int
    managed_loc: int
    notes: str


def count_native_loc(root: Path) -> dict[str, int]:
    counts: dict[str, int] = defaultdict(int)
    for path in root.rglob("*"):
        if path.suffix not in {".cpp", ".c"}:
            continue
        rel = path.relative_to(root)
        top = rel.parts[0] if rel.parts else ""
        try:
            text = path.read_text(encoding="utf-8", errors="replace")
        except OSError:
            continue
        lines = text.count("\n") + (0 if text.endswith("\n") or not text else 1)
        counts[top] += lines
        counts["__total__"] += lines
    return counts


def count_managed_loc(root: Path) -> int:
    total = 0
    for path in root.rglob("*.cs"):
        try:
            text = path.read_text(encoding="utf-8", errors="replace")
        except OSError:
            continue
        total += text.count("\n") + (0 if text.endswith("\n") or not text else 1)
    return total


def parse_acs_pcode_enum(text: str) -> dict[str, int]:
    names: dict[str, int] = {}
    for match in re.finditer(r"^\s*([A-Za-z0-9_]+)\s*=\s*(-?\d+),?", text, re.MULTILINE):
        names[match.group(1)] = int(match.group(2))
    return names


def expand_acs_vm_cases(source: str, pcode: dict[str, int]) -> set[int]:
    executed: set[int] = set()
    for name in re.findall(r"\(int\)AcsPcode\.([A-Za-z0-9_]+)", source):
        if name in pcode:
            executed.add(pcode[name])
    for value in re.findall(r"case\s+(\d+)\s*:", source):
        executed.add(int(value))
    for low, high in re.findall(
        r"case\s+>=\s*\(int\)AcsPcode\.([A-Za-z0-9_]+)\s+and\s+<=\s*\(int\)AcsPcode\.([A-Za-z0-9_]+)",
        source,
    ):
        if low in pcode and high in pcode:
            lo, hi = pcode[low], pcode[high]
            if lo <= hi:
                executed.update(range(lo, hi + 1))
    for a, b in re.findall(
        r"case\s+\(int\)AcsPcode\.([A-Za-z0-9_]+)\s+or\s+\(int\)AcsPcode\.([A-Za-z0-9_]+)",
        source,
    ):
        for name in (a, b):
            if name in pcode:
                executed.add(pcode[name])
    for group in re.findall(
        r"case\s+(?:\(int\)AcsPcode\.\w+\s+or\s+)+\(int\)AcsPcode\.([A-Za-z0-9_]+)",
        source,
    ):
        if group in pcode:
            executed.add(pcode[group])
    # Numeric alternates (legacy wire ids)
    for chunk in re.findall(r"case\s+([\d\sor]+):", source):
        for token in chunk.split("or"):
            token = token.strip()
            if token.isdigit():
                executed.add(int(token))
    return executed


def parse_acs_decoded(walker: str, pcode: dict[str, int]) -> set[int]:
    decoded: set[int] = set()
    for name in re.findall(r"\(int\)AcsPcode\.([A-Za-z0-9_]+)", walker):
        if name in pcode:
            decoded.add(pcode[name])
    for value in re.findall(r"\b(\d{2,4})\s+or\s+\d", walker):
        decoded.add(int(value))
    for value in re.findall(r"^\s*(\d+)\s+or\s+\d", walker, re.MULTILINE):
        decoded.add(int(value))
    for value in re.findall(r"^\s*(\d+)\s*=>", walker, re.MULTILINE):
        decoded.add(int(value))
    return decoded


ACS_PARTIAL = {
    "PlayerTeam",
    "PlayerBlackSkull",
    "PlayerSilverSkull",
    "PlayerGoldSkull",
    "PlayerBlackCard",
    "PlayerSilverCard",
    "PlayerGoldCard",
    "ClearLineSpecial",
    "PolyWait",
    "PolyWaitDirect",
}
# PolyWait is executed (no polyobjects yet) but listed partial until poly motion exists.


def classify_acs(pcode: dict[str, int], executed: set[int], decoded: set[int]) -> list[dict]:
    rows = []
    for name, opcode in sorted(pcode.items(), key=lambda item: item[1]):
        if opcode in executed:
            status = STATUS_PARTIAL if name in ACS_PARTIAL else STATUS_EXECUTED
        elif opcode in decoded:
            status = STATUS_DECODED
        else:
            status = STATUS_ABSENT
        rows.append(
            {
                "id": f"acs-pcode:{opcode}",
                "name": name,
                "opcode": opcode,
                "status": status,
                "nativeRef": "src/playsim/p_acs.cpp",
            }
        )
    return rows


def parse_vmops_h(path: Path) -> list[dict]:
    text = path.read_text(encoding="utf-8", errors="replace")
    ops = []
    for index, match in enumerate(re.finditer(r"xx\(([A-Z0-9_]+),", text)):
        ops.append({"index": index, "name": match.group(1)})
    return ops


def parse_vm_exec_ops(path: Path) -> set[int]:
    text = path.read_text(encoding="utf-8", errors="replace")
    executed: set[int] = set()
    for name in re.findall(r"VmOp\.([A-Za-z0-9_]+)", text):
        executed.add(name)
    return executed


def parse_vmop_enum(path: Path) -> dict[str, int]:
    text = path.read_text(encoding="utf-8", errors="replace")
    enum_block = text.split("public enum VmOp", 1)[-1].split("}", 1)[0]
    mapping: dict[str, int] = {}
    for match in re.finditer(r"^\s*([A-Za-z0-9_]+)\s*=\s*(\d+),?", enum_block, re.MULTILINE):
        mapping[match.group(1)] = int(match.group(2))
    return mapping


def classify_vm(vmops: list[dict], vm_enum: dict[str, int], executed_names: set[str]) -> list[dict]:
    """Classify vmops.h rows by opcode index; VmOp enum values follow vmops.h order."""
    by_index = {value: name for name, value in vm_enum.items()}
    rows = []
    for op in vmops:
        index = op["index"]
        csharp = by_index.get(index)
        status = STATUS_EXECUTED if csharp and csharp in executed_names else STATUS_ABSENT
        rows.append(
            {
                "id": f"vmop:{op['index']}",
                "name": op["name"],
                "index": index,
                "status": status,
                "nativeRef": "src/common/scripting/vm/vmops.h",
            }
        )
    return rows


def parse_acs_call_functions(source: str) -> tuple[set[int], set[int]]:
    declared: set[int] = set()
    for match in re.finditer(r"public const int (\w+) = (\d+);", source):
        declared.add(int(match.group(2)))
    implemented: set[int] = set()
    for match in re.finditer(r"case (\d+):", source):
        implemented.add(int(match.group(1)))
    for a, b in re.findall(r"case (\d+):\s*\n\s*case (\d+):", source):
        implemented.update({int(a), int(b)})
    return declared, implemented


def parse_native_acs_functions(path: Path) -> list[str]:
    text = path.read_text(encoding="utf-8", errors="replace")
    block = text.split("enum EACSFunctions", 1)[-1].split("};", 1)[0]
    return re.findall(r"ACSF_([A-Za-z0-9_]+)", block)


def build_matrix(loc_tally: dict) -> dict:
    pcode_text = (CSHARP / "src/HCDE.MapLoader/AcsPcode.cs").read_text(encoding="utf-8")
    pcode = parse_acs_pcode_enum(pcode_text)
    acs_vm = (CSHARP / "src/HCDE.Playsim/AcsVm.cs").read_text(encoding="utf-8")
    walker = (CSHARP / "src/HCDE.MapLoader/MapBehaviorBytecodeWalker.cs").read_text(encoding="utf-8")
    executed_acs = expand_acs_vm_cases(acs_vm, pcode)
    decoded_acs = parse_acs_decoded(walker, pcode)
    acs_rows = classify_acs(pcode, executed_acs, decoded_acs)

    vmops = parse_vmops_h(SRC / "common/scripting/vm/vmops.h")
    vm_enum = parse_vmop_enum(CSHARP / "src/HCDE.Scripting/VmOp.cs")
    vm_exec_names = parse_vm_exec_ops(CSHARP / "src/HCDE.Scripting/VmExec.cs")
    vm_rows = classify_vm(vmops, vm_enum, vm_exec_names)

    call_src = (CSHARP / "src/HCDE.Playsim/AcsCallFunctions.cs").read_text(encoding="utf-8")
    declared_calls, implemented_calls = parse_acs_call_functions(call_src)
    native_calls = parse_native_acs_functions(REPO / "src/playsim/p_acs.cpp")
    call_rows = []
    for index, native_name in enumerate(native_calls):
        # wire id is 1-based in native enum order for inventory (not exact wire); use name only
        status = STATUS_ABSENT
        if index + 1 in implemented_calls:
            status = STATUS_EXECUTED
        elif native_name in call_src:
            status = STATUS_PARTIAL
        call_rows.append(
            {
                "id": f"acs-call:{native_name}",
                "name": native_name,
                "status": status,
                "nativeRef": "src/playsim/p_acs.cpp EACSFunctions",
            }
        )

    native_counts = count_native_loc(SRC)
    managed_loc = count_managed_loc(CSHARP / "src")

    def summarize(rows: list[dict]) -> dict[str, int]:
        summary: dict[str, int] = defaultdict(int)
        for row in rows:
            summary[row["status"]] += 1
        return dict(summary)

    subsystem_rows = []
    for top, loc in sorted(native_counts.items(), key=lambda item: -item[1]):
        if top == "__total__":
            continue
        subsystem_rows.append(
            {
                "directory": f"src/{top}/",
                "nativeLoc": loc,
                "managedEquivalent": f"csharp/src/HCDE.* (partial)",
                "notes": "No full directory port; behavior landed incrementally in managed playsim/scripting.",
            }
        )

    return {
        "generatedOn": date.today().isoformat(),
        "locScope": {
            "nativeEngineRoot": "src/",
            "nativeExtensions": [".cpp", ".c"],
            "excluded": [
                "libraries/ (vendored deps)",
                "tools/ (build utilities)",
                "protocol/ (shared constants; mirrored in HCDE.Protocol)",
            ],
            "nativeEngineLoc": native_counts["__total__"],
            "managedCSharpLoc": managed_loc,
        },
        "locTally": loc_tally,
        "summaries": {
            "acsPcode": summarize(acs_rows),
            "zscriptVmOp": summarize(vm_rows),
            "acsCallFunc": summarize(call_rows),
        },
        "acsPcode": acs_rows,
        "zscriptVmOp": vm_rows,
        "acsCallFunc": call_rows,
        "nativeSubsystems": subsystem_rows,
    }


def render_markdown(matrix: dict) -> str:
    tally = matrix["locTally"]
    scope = matrix["locScope"]
    acs = matrix["summaries"]["acsPcode"]
    vm = matrix["summaries"]["zscriptVmOp"]
    calls = matrix["summaries"]["acsCallFunc"]

    lines = [
        "# HCDE native-to-managed support matrix",
        "",
        f"Generated: {matrix['generatedOn']}. Machine-readable: [`data/native_managed_matrix.json`](data/native_managed_matrix.json).",
        "",
        "## LOC scope and tally",
        "",
        f"- Native engine scope: `{scope['nativeEngineRoot']}` `*.cpp` / `*.c` only → **{scope['nativeEngineLoc']:,}** lines.",
        f"- Excluded from engine scope: {', '.join(scope['excluded'])}.",
        f"- Managed implementation: `csharp/src/**/*.cs` → **{scope['managedCSharpLoc']:,}** lines.",
        "",
        "### Run tally",
        "",
        f"- Converted this run (native behavior, tested): **{tally['convertedThisRunNative']:,}** native lines.",
        f"- Recognition this run (already ported, tested): **{tally.get('recognitionThisRunNative', 0):,}** native lines.",
        f"- Added this run (managed): **{tally['convertedThisRunManaged']:,}** C# lines.",
        f"- Cumulative conversion (native): **{tally.get('convertedBehaviorNative', tally.get('convertedThisRunNative', 0)):,}**; recognition: **{tally.get('recognitionBehaviorNative', 0):,}**; combined: **{tally.get('combinedBehaviorNative', 0):,}**.",
        f"- Native engine remaining (scope): **{tally['remainingNative']:,}** lines.",
        "",
        "Native lines count toward conversion only when matching behavior is implemented and covered by regression tests.",
        "",
        "## Headline coverage",
        "",
        "| Area | Executed | Partial | Decoded-only | Absent |",
        "| --- | ---: | ---: | ---: | ---: |",
        f"| ACS word opcodes (`AcsPcode`) | {acs.get('executed', 0)} | {acs.get('partial', 0)} | {acs.get('decoded-only', 0)} | {acs.get('absent', 0)} |",
        f"| ZScript VM opcodes (`vmops.h` rows) | {vm.get('executed', 0)} | {vm.get('partial', 0)} | {vm.get('decoded-only', 0)} | {vm.get('absent', 0)} |",
        f"| ACS `CallFunc` (`EACSFunctions` names) | {calls.get('executed', 0)} | {calls.get('partial', 0)} | {calls.get('decoded-only', 0)} | {calls.get('absent', 0)} |",
        "",
        "## Native `src/` LOC by top-level directory",
        "",
        "| Directory | Native LOC | Managed notes |",
        "| --- | ---: | --- |",
    ]
    for row in matrix["nativeSubsystems"][:15]:
        lines.append(f"| `{row['directory']}` | {row['nativeLoc']:,} | {row['notes']} |")
    lines.extend(
        [
            "",
            "## Method",
            "",
            "- ACS opcode execution is inferred from `HCDE.Playsim/AcsVm.cs` switch cases.",
            "- ACS decode coverage is inferred from `HCDE.MapLoader/MapBehaviorBytecodeWalker.cs` operand tables.",
            "- VM opcode names come from `src/common/scripting/vm/vmops.h`; execution from `HCDE.Scripting/VmExec.cs`.",
            "- Partial ACS rows are listed in `ACS_PARTIAL` inside `csharp/tools/generate_native_managed_matrix.py`.",
            "",
        ]
    )
    return "\n".join(lines)


def main() -> int:
    tally_path = DATA / "loc_tally.json"
    if tally_path.exists():
        loc_tally = json.loads(tally_path.read_text(encoding="utf-8"))
    else:
        native_total = count_native_loc(SRC)["__total__"]
        loc_tally = {
            "convertedThisRunNative": 0,
            "convertedThisRunManaged": 0,
            "remainingNative": native_total,
        }
    matrix = build_matrix(loc_tally)
    DATA.mkdir(parents=True, exist_ok=True)
    (DATA / "native_managed_matrix.json").write_text(json.dumps(matrix, indent=2) + "\n", encoding="utf-8")
    (DOCS / "HCDE_CSHARP_NATIVE_MANAGED_MATRIX.md").write_text(render_markdown(matrix), encoding="utf-8")
    print(f"Wrote matrix with {len(matrix['acsPcode'])} ACS opcodes")
    return 0


if __name__ == "__main__":
    sys.exit(main())
