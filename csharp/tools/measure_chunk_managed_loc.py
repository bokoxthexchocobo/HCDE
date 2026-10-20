#!/usr/bin/env python3
"""Set chunk managedLoc from line count of csharp/src/*.cs files added in conversionManagedCommit."""
import json
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1].parent
CSHARP = REPO / "csharp"

import sys

sys.path.insert(0, str(CSHARP / "tools"))
from chunk_port_managed import managed_loc_for_chunk  # noqa: E402


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
            ml = managed_loc_for_chunk(chunk_def, sha, num)
            if ml <= 0:
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
