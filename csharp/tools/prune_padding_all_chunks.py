#!/usr/bin/env python3
"""Drop padding-shaped methods unless referenced by that chunk's listed tests."""
from __future__ import annotations

import json
import re
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
TOOLS = REPO / "tools"
TESTS = REPO / "tests"
sys.path.insert(0, str(TOOLS))
from reaudit_chunks import scan_padding_methods, chunk_number, CHUNK_RULES_MIN_CHUNK  # noqa: E402
from strip_padding_hits import remove_methods  # noqa: E402

CHUNK_RE = re.compile(r"chunk(\d+)")


def tests_for_chunk(num: int) -> list[str]:
    path = TOOLS / f"chunk{num}.json"
    if not path.is_file():
        return []
    data = json.loads(path.read_text())
    if isinstance(data, list):
        data = data[0]
    if data.get("retired"):
        return []
    names: set[str] = set()
    for r in data.get("nativeRanges") or []:
        for t in r.get("tests") or []:
            names.add(t.split(".")[0])
    return sorted(names)


def methods_referenced_in_test_classes(class_names: list[str]) -> set[str]:
    refs: set[str] = set()
    if not class_names:
        return refs
    for path in TESTS.rglob("*Tests.cs"):
        text = path.read_text(errors="replace")
        if not any(f"class {c}" in text for c in class_names):
            continue
        refs.update(re.findall(r"Vm\w+\.(\w+)\(", text))
        refs.update(re.findall(r"Zcc\w+\.(\w+)\(", text))
    return refs


def allowlist_from_audit(text: str) -> set[str]:
    return set(re.findall(r"`([^`]+)`", text or ""))


def main() -> int:
    removed_total = 0
    files_touched = 0
    for path in sorted(TOOLS.glob("chunk*.json")):
        m = CHUNK_RE.search(path.name)
        if not m:
            continue
        num = int(m.group(1))
        if num < CHUNK_RULES_MIN_CHUNK:
            continue
        data = json.loads(path.read_text())
        if isinstance(data, list):
            data = data[0]
        if data.get("retired") or not data.get("nativeRanges"):
            continue
        test_classes = tests_for_chunk(num)
        keep_from_tests = methods_referenced_in_test_classes(test_classes)
        allow = allowlist_from_audit(data.get("auditPaddingReview") or "")
        for rel in data.get("managedSourceFiles") or []:
            cs = REPO.parent / rel
            if not cs.is_file():
                continue
            text = cs.read_text()
            hits = scan_padding_methods(text)
            remove = {
                h["method"]
                for h in hits
                if h["method"] not in keep_from_tests and h["method"] not in allow
            }
            if not remove:
                continue
            cs.write_text(remove_methods(text, remove))
            removed_total += len(remove)
            files_touched += 1
            print(f"chunk{num}: removed {len(remove)} from {rel}")
    print(f"done: {removed_total} methods in {files_touched} files")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
