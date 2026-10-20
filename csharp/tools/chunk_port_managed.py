#!/usr/bin/env python3
"""Per-chunk managed LOC from conversionManagedCommit, scoped to managedSourceFiles."""
from __future__ import annotations

import json
import re
import subprocess
from pathlib import Path

REPO = Path(__file__).resolve().parents[1].parent


def commit_src_additions(sha: str, source_files: list[str] | None = None) -> int:
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


def managed_loc_for_chunk(chunk_def: dict, sha: str, num: int | None = None) -> int:
    files = managed_files_for_chunk(num, chunk_def)
    commits = port_commits(chunk_def, sha)
    total = 0
    for commit in commits:
        total += commit_src_additions(commit, files if files else None)
    return total
