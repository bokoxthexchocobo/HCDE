#!/usr/bin/env python3
"""Merge source chunk into target: union nativeRanges + managedSourceFiles, retire source."""
import json
from pathlib import Path

TOOLS = Path(__file__).resolve().parent

MERGES: list[tuple[int, int]] = [
    (543, 542),
    (604, 602),
    (605, 604),
    (606, 604),
    (607, 604),
    (608, 604),
    (456, 457),
    (464, 458),
    (484, 483),
    (506, 505),
    (511, 510),
    (523, 522),
    (525, 526),
    (544, 543),
    (561, 560),
    (571, 570),
    (587, 586),
    (546, 545),
]


def load(n: int) -> dict:
    d = json.loads((TOOLS / f"chunk{n}.json").read_text())
    return d[0] if isinstance(d, list) else d


def save(n: int, ch: dict) -> None:
    (TOOLS / f"chunk{n}.json").write_text(json.dumps([ch], indent=2) + "\n")


def merge(target: int, source: int) -> None:
    t = load(target)
    s = load(source)
    files = list(t.get("managedSourceFiles") or [])
    for f in s.get("managedSourceFiles") or []:
        if f not in files:
            files.append(f)
    t["managedSourceFiles"] = files
    t["nativeRanges"] = (t.get("nativeRanges") or []) + (s.get("nativeRanges") or [])
    t["note"] = (t.get("note") or "") + f" (merged chunk {source})"
    save(target, t)
    s["retired"] = True
    s["nativeRanges"] = []
    s["note"] = f"Merged into chunk{target}."
    save(source, s)
    print(f"merged {source} -> {target}")


def main() -> None:
    for src, tgt in MERGES:
        if not (TOOLS / f"chunk{tgt}.json").is_file():
            print(f"skip {src}->{tgt} missing target")
            continue
        merge(tgt, src)


if __name__ == "__main__":
    main()
