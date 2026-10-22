#!/usr/bin/env python3
"""Set nativeLoc, managedLoc, conversionManagedCommit on the last tally chunk from chunkN.json + HEAD."""
import json
import re
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(REPO / "tools"))
from chunk_port_managed import managed_loc_for_chunk  # noqa: E402

num = int(sys.argv[1])
port_sha = sys.argv[2].strip() if len(sys.argv) > 2 else ""
sha = subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=REPO.parent, text=True).strip()
commit_sha = port_sha if re.fullmatch(r"[0-9a-f]{40}", port_sha) else sha
head_src = subprocess.check_output(
    ["git", "diff-tree", "--no-commit-id", "--name-only", "-r", sha],
    cwd=REPO.parent,
    text=True,
).splitlines()
head_touches_managed_src = any(
    p.startswith("csharp/src/") and p.endswith(".cs") for p in head_src
)
chunk = json.loads((REPO / "tools" / f"chunk{num}.json").read_text())
if isinstance(chunk, list):
    chunk = chunk[0]
ml = managed_loc_for_chunk(chunk, sha, num)
nl = sum(int(r["end"]) - int(r["start"]) + 1 for r in chunk["nativeRanges"])
tally = json.loads((REPO / "docs/data/loc_tally.json").read_text())
for ch in tally["chunks"]:
    if ch["name"] == chunk["name"]:
        ch["nativeRanges"] = chunk["nativeRanges"]
        ch["nativeLoc"] = nl
        ch["managedLoc"] = ml
        if head_touches_managed_src or re.fullmatch(r"[0-9a-f]{40}", port_sha):
            ch["conversionManagedCommit"] = commit_sha
        for k in (
            "note",
            "auditRationale",
            "auditSeams",
            "auditPaddingReview",
            "auditManagedLocJustification",
            "managedSourceFiles",
            "auditSingleFunctionOversizeException",
            "retired",
        ):
            if k in chunk:
                ch[k] = chunk[k]
        break
else:
    chunk["nativeLoc"] = nl
    chunk["managedLoc"] = ml
    if head_touches_managed_src or re.fullmatch(r"[0-9a-f]{40}", port_sha):
        chunk["conversionManagedCommit"] = commit_sha
    tally["chunks"].append(chunk)
(REPO / "docs/data/loc_tally.json").write_text(json.dumps(tally, indent=2) + "\n")
if num in (719, 720, 721):
    import patch_tally_chase_recognition  # noqa: F401
if num == 722:
    import patch_tally_radius_recognition  # noqa: F401
if num in (724, 725, 726, 728):
    from patch_tally_mobj_movement_recognition import trim_for_chunk

    trim_for_chunk(num)
if num == 727:
    import patch_tally_actor_die_recognition  # noqa: F401
if num == 729:
    import patch_tally_damagemobj_chunk474  # noqa: F401
if num == 732:
    import patch_tally_poison_chunk732  # noqa: F401
if num == 730:
    import patch_retire_acs_clientside_730  # noqa: F401
    import patch_tally_behavior_init_chunk730  # noqa: F401
if num == 731:
    import patch_tally_callstatechain_chunk731  # noqa: F401
if num == 733:
    import patch_tally_okayswitch_chunk733  # noqa: F401
print(f"{chunk['name']} native={nl} managed_commit={ml} sha={sha[:8]}")
