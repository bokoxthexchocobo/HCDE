#!/usr/bin/env python3
"""Build p_lnspec.cpp vs managed line-special gap inventory."""

from __future__ import annotations

import json
import re
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
CPP = REPO / "src/playsim/p_lnspec.cpp"
CS_DIR = REPO / "csharp/src/HCDE.Playsim"
OUT = REPO / "csharp/docs/data/line_special_gap.json"

WIRED_OVERRIDES = {
    151, 186, 187, 189, 223, 224, 119,
}


def parse_native_table(text: str) -> list[dict]:
    block = re.search(r"static lnSpecFunc LineSpecials\[\]\s*=\s*\{(.*?)\};", text, re.S)
    rows = []
    for line in block.group(1).splitlines():
        m = re.match(r"\s*/\*\s*(\d+)\s*\*/\s*(\w+)", line)
        if not m:
            continue
        num, func = int(m.group(1)), m.group(2)
        rows.append({"special": num, "nativeHandler": func, "nativeStatus": "nop" if func == "LS_NOP" else "implemented"})
    return rows


def managed_specials() -> set[int]:
    nums: set[int] = set(WIRED_OVERRIDES)
    for path in CS_DIR.glob("*.cs"):
        text = path.read_text(encoding="utf-8", errors="replace")
        for m in re.finditer(r"public const int (\w+)\s*=\s*(\d+)", text):
            nums.add(int(m.group(2)))
        for m in re.finditer(r"internal const int (\w+)\s*=\s*(\d+)", text):
            nums.add(int(m.group(2)))
        for m in re.finditer(r"(\d+(?:\s+or\s+\d+)+)\s*=>", text):
            for part in m.group(1).split("or"):
                nums.add(int(part.strip()))
        for m in re.finditer(r"special is not \(([^)]+)\)", text):
            for part in re.split(r"\s+or\s+", m.group(1)):
                part = part.strip()
                if part.isdigit():
                    nums.add(int(part))
                elif "." in part:
                    const = part.split(".")[-1]
                    cm = re.search(rf"const int {const}\s*=\s*(\d+)", text)
                    if cm:
                        nums.add(int(cm.group(1)))
    return nums


def main() -> None:
    native = parse_native_table(CPP.read_text(encoding="utf-8", errors="replace"))
    managed = managed_specials()
    for row in native:
        if row["nativeStatus"] == "nop":
            row["managedStatus"] = "native-nop"
        elif row["special"] in managed:
            row["managedStatus"] = "wired"
        else:
            row["managedStatus"] = "gap"
    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text(json.dumps({"generatedOn": "2026-10-19", "rows": native}, indent=2) + "\n", encoding="utf-8")
    gaps = [r for r in native if r["managedStatus"] == "gap"]
    print(f"wrote {OUT} ({len(gaps)} gaps of {len([r for r in native if r['nativeStatus']!='nop'])} native handlers)")


if __name__ == "__main__":
    main()
