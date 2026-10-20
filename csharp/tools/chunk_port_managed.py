#!/usr/bin/env python3
"""Managed LOC for migration chunks: surviving source lines at HEAD only."""
from __future__ import annotations

import json
import re
import subprocess
from pathlib import Path

REPO = Path(__file__).resolve().parents[1].parent


def physical_loc(path: Path) -> int:
    if not path.is_file():
        return 0
    return len(path.read_text(encoding="utf-8", errors="replace").splitlines())


def commit_src_additions(sha: str, source_files: list[str] | None = None) -> int:
    """Legacy: sum of added lines in one commit (used only for inflation detection)."""
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
        if not path.startswith("csharp/src/") or not path.endswith(".cs"):
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
    return [
        p
        for p in out.splitlines()
        if p.startswith("csharp/src/") and p.endswith(".cs")
    ]


def port_commits(chunk_def: dict, primary_sha: str) -> list[str]:
    extra = chunk_def.get("portCommits") or []
    commits: list[str] = []
    for sha in [primary_sha, *extra]:
        if re.fullmatch(r"[0-9a-f]{40}", sha or "") and sha not in commits:
            commits.append(sha)
    return commits


def managed_loc_surviving_at_head(chunk_def: dict, num: int | None = None) -> int:
    """Non-empty physical lines in managedSourceFiles (or commit file list) at repo HEAD."""
    files = managed_files_for_chunk(num, chunk_def)
    total = 0
    for rel in files:
        total += physical_loc(REPO / rel)
    return total


def managed_loc_commit_additions_sum(chunk_def: dict, sha: str, num: int | None = None) -> int:
    """Line additions in conversionManagedCommit only (must not exceed surviving LOC at HEAD)."""
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
