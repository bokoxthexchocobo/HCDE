#!/usr/bin/env python3
"""Managed LOC for migration chunks: surviving csharp/src lines at HEAD only."""
from __future__ import annotations

import json
import re
import subprocess
from pathlib import Path

REPO = Path(__file__).resolve().parents[1].parent

MANAGED_SRC_PREFIX = "csharp/src/"


def is_countable_managed_csharp_line(line: str) -> bool:
    """All physical lines count (see ``managed_loc_surviving_at_head`` note in honesty audit)."""
    return True


def validate_managed_source_files(files: list[str]) -> None:
    for rel in files:
        if not rel.startswith(MANAGED_SRC_PREFIX) or not rel.endswith(".cs"):
            raise ValueError(f"managedSourceFiles must be csharp/src/*.cs, got {rel}")
        if ".Tests/" in rel or "/tests/" in rel.lower():
            raise ValueError(f"managedSourceFiles must not include tests: {rel}")


def countable_loc_in_file(path: Path) -> int:
    """Physical line count at HEAD (parity with historical ``git numstat`` land credits)."""
    return physical_loc(path)


def physical_loc(path: Path) -> int:
    if not path.is_file():
        return 0
    return len(path.read_text(encoding="utf-8", errors="replace").splitlines())


def commit_src_additions(sha: str, source_files: list[str] | None = None) -> int:
    """Sum of added lines in one commit for csharp/src/*.cs (tests excluded)."""
    if not re.fullmatch(r"[0-9a-f]{40}", sha or ""):
        return 0
    try:
        out = subprocess.check_output(
            ["git", "show", sha, "--numstat", "--format="],
            cwd=REPO,
            text=True,
        )
    except subprocess.CalledProcessError:
        return 0
    want = set(source_files or [])
    total = 0
    for line in out.splitlines():
        parts = line.split("\t")
        if len(parts) != 3:
            continue
        add_s, _del, path = parts
        if not path.startswith(MANAGED_SRC_PREFIX) or not path.endswith(".cs"):
            continue
        if ".Tests/" in path:
            continue
        if want and path not in want:
            continue
        if add_s.isdigit():
            total += int(add_s)
    return total


def chunk_def_path(num: int) -> Path:
    return REPO / "csharp" / "tools" / f"chunk{num}.json"


def managed_files_for_chunk(num: int | None, chunk_def: dict) -> list[str]:
    listed = chunk_def.get("managedSourceFiles") or []
    if listed:
        validate_managed_source_files(listed)
        return list(listed)
    if num is None:
        return []
    sha = chunk_def.get("conversionManagedCommit", "")
    if not re.fullmatch(r"[0-9a-f]{40}", sha or ""):
        return []
    try:
        out = subprocess.check_output(
            ["git", "show", sha, "--name-only", "--format="],
            cwd=REPO,
            text=True,
        )
    except subprocess.CalledProcessError:
        return []
    files = [
        p
        for p in out.splitlines()
        if p.startswith(MANAGED_SRC_PREFIX) and p.endswith(".cs")
    ]
    validate_managed_source_files(files)
    return files


def latest_commit_touching_files(files: list[str]) -> str:
    if not files:
        return ""
    try:
        out = subprocess.check_output(
            ["git", "log", "-1", "--format=%H", "--", *files],
            cwd=REPO,
            text=True,
        ).strip()
    except subprocess.CalledProcessError:
        return ""
    return out if re.fullmatch(r"[0-9a-f]{40}", out) else ""


def managed_loc_surviving_at_head(chunk_def: dict, num: int | None = None) -> int:
    """Countable csharp/src lines in managedSourceFiles at repo HEAD (tests never included)."""
    files = managed_files_for_chunk(num, chunk_def)
    total = 0
    for rel in files:
        total += countable_loc_in_file(REPO / rel)
    return total


def managed_loc_commit_additions_sum(chunk_def: dict, sha: str, num: int | None = None) -> int:
    files = managed_files_for_chunk(num, chunk_def)
    if not re.fullmatch(r"[0-9a-f]{40}", sha or ""):
        return 0
    return commit_src_additions(sha, files if files else None)


def managed_loc_for_chunk(chunk_def: dict, sha: str, num: int | None = None) -> int:
    return managed_loc_surviving_at_head(chunk_def, num)


def assert_no_commit_inflation(chunk_def: dict, sha: str, num: int | None = None) -> None:
    surviving = managed_loc_surviving_at_head(chunk_def, num)
    legacy = managed_loc_commit_additions_sum(chunk_def, sha, num)
    if legacy > surviving:
        raise ValueError(
            f"managed_loc_commit_inflation: commit-addition sum {legacy} > surviving HEAD {surviving}"
        )


def collect_managed_source_files_from_repo() -> dict[str, list[str]]:
    """Map csharp/src path -> chunk json names that claim it."""
    owners: dict[str, list[str]] = {}
    for path in sorted((REPO / "csharp" / "tools").glob("chunk*.json")):
        try:
            data = json.loads(path.read_text())
        except json.JSONDecodeError:
            continue
        if isinstance(data, list):
            data = data[0]
        name = data.get("name", path.name)
        for rel in data.get("managedSourceFiles") or []:
            owners.setdefault(rel, []).append(name)
    return owners
