#!/usr/bin/env python3
"""Set chunk managedLoc from line count of csharp/src/*.cs files added in conversionManagedCommit."""
import json
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1].parent
CSHARP = REPO / "csharp"


def src_lines_in_commit(sha: str) -> int:
    out = subprocess.check_output(
        ["git", "show", sha, "--numstat", "--format="],
        cwd=REPO,
        text=True,
    )
    total = 0
    for line in out.splitlines():
        parts = line.split("\t")
        if len(parts) != 3:
            continue
        add_s, _del, path = parts
        if path.startswith("csharp/src/") and path.endswith(".cs") and add_s.isdigit():
            total += int(add_s)
    return total


def file_lines_at_commit(sha: str, path: str) -> int:
    try:
        text = subprocess.check_output(["git", "show", f"{sha}:{path}"], cwd=REPO, text=True)
    except subprocess.CalledProcessError:
        return 0
    return len(text.splitlines())


def lines_at_head(path: str) -> int:
    p = REPO / path
    if not p.is_file():
        return 0
    return len(p.read_text().splitlines())


def main() -> int:
    nums = [int(x) for x in sys.argv[1:]]
    tally = json.loads((CSHARP / "docs/data/loc_tally.json").read_text())
    for num in nums:
        chunk_def = json.loads((CSHARP / f"tools/chunk{num}.json").read_text())
        if isinstance(chunk_def, list):
            chunk_def = chunk_def[0]
        name = chunk_def["name"]
        for ch in tally["chunks"]:
            if ch["name"] != name:
                continue
            sha = ch.get("conversionManagedCommit")
            if not sha:
                print(f"chunk {num}: no conversionManagedCommit", file=sys.stderr)
                break
            listed = chunk_def.get("managedSourceFiles") or []
            if listed:
                ml = sum(lines_at_head(p) for p in listed)
            else:
                # Prefer total lines in primary new file at commit (whole VM), else sum additions.
                show = subprocess.check_output(
                    ["git", "show", sha, "--name-only", "--format="],
                    cwd=REPO,
                    text=True,
                )
                src_files = [
                    p
                    for p in show.splitlines()
                    if p.startswith("csharp/src/") and p.endswith(".cs")
                ]
                if len(src_files) == 1:
                    ml = file_lines_at_commit(sha, src_files[0])
                elif len(src_files) > 1:
                    ml = sum(file_lines_at_commit(sha, p) for p in src_files)
                else:
                    ml = src_lines_in_commit(sha)
            ch["managedLoc"] = ml
            print(f"{name} managedLoc={ml} sha={sha[:8]}")
            break
        else:
            print(f"chunk {num}: not in tally", file=sys.stderr)
    (CSHARP / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
