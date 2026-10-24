#!/usr/bin/env python3
"""Post-529a0df8 audit hygiene: per-chunk justifications, managed shrink, 648→649, test pairing."""
from __future__ import annotations

import json
import subprocess
from pathlib import Path

TOOLS = Path(__file__).resolve().parent
REPO = TOOLS.parents[1]
TALLY = TOOLS.parent / "docs/data/loc_tally.json"

JUSTIFICATIONS: dict[int, str] = {
    469: (
        "Enemy look/scan helpers split `P_LookForPlayers` into explicit sector and FOV gates; "
        "managed LOC is above native credit because each visibility branch is a named predicate."
    ),
    474: (
        "`DamageMobj` tail plus merged reflection/kickback bands use separate Vm* files for interaction "
        "versus mobj sound/portal helpers; pmap test-mobj files are not credited on this row."
    ),
    481: (
        "`P_MovePlayer` hosted slices decompose friction, bob, and crouch into context structs rather than "
        "nested C++ temporaries."
    ),
    509: (
        "`P_PredictClient` replay/window/rubberband policy is a long native control-flow; managed code "
        "names each gate (netgame, reserved slot, catch-up) without padding helpers."
    ),
    566: (
        "ACS level-script list plus merged `GetNetBufferSize`/`HGetPacket` sizing share one row after "
        "sub-150 merges; net buffer helpers are credited only where listed in the audit map."
    ),
    567: (
        "ACS inventory/string pool ports mirror `p_acs.cpp` call chains; merged behavior-container bands "
        "add module lifecycle helpers with explicit DTOs."
    ),
    570: (
        "`ClientConnecting` late-join path and `HGetPacket` tail are separate functions; merged codegen "
        "sign helpers are credited only on the ZCC rows in the audit map."
    ),
    591: (
        "Credits `FBehavior::SerializeVars` through `Init` only; action-function merge artifacts are "
        "not attributed on this row."
    ),
    601: (
        "Credits `Net_DoCommand` SAY–SUICIDE case guards only; world-loop and legacy-payload VMs live "
        "on other chunk rows."
    ),
    615: (
        "Net write helpers and dynamic buffer methods mirror `FDynamicBuffer` and `Net_Write*` as "
        "explicit byte-oriented APIs."
    ),
    618: (
        "Lag/latency skip helpers for user-cmd messages are decomposed predicates on `d_net.cpp` "
        "boundary checks."
    ),
    622: (
        "Session lifecycle (`I_QueryServerInfo` through `SetConnectFlow`) uses injected I/O; launcher "
        "bands were split to chunk 649."
    ),
    640: (
        "HCDE server-mode accessors duplicate C++ guard branches as named natives for diagnostics and "
        "startup UI suppression."
    ),
}


def load(num: int) -> dict:
    data = json.loads((TOOLS / f"chunk{num}.json").read_text(encoding="utf-8"))
    return data[0] if isinstance(data, list) else data


def save(num: int, ch: dict) -> None:
    (TOOLS / f"chunk{num}.json").write_text(json.dumps([ch], indent=2) + "\n", encoding="utf-8")


def trim_audit(text: str, marker: str) -> str:
    if marker not in text:
        return text
    return text.split(marker, 1)[0].rstrip() + "\n"


def main() -> int:
    launcher = json.loads(
        subprocess.check_output(
            ["git", "show", "529a0df8:csharp/tools/chunk648.json"],
            cwd=REPO,
            text=True,
        )
    )[0]
    launcher["name"] = "i-net-launcher-pregame-ui-chunk649"
    save(649, launcher)

    ch648_retired = [
        {
            "name": "d-net-blackbox-record-chunk648",
            "bucket": "conversion",
            "retired": True,
            "note": "Merged into d-net-blackbox-ring-chunk647 (`d_net_blackbox.cpp` 35–337).",
            "nativeRanges": [],
            "managedSourceFiles": [],
        }
    ]
    (TOOLS / "chunk648.json").write_text(json.dumps(ch648_retired, indent=2) + "\n", encoding="utf-8")

    ch622 = load(622)
    ch622["note"] = ch622.get("note", "").replace("chunk648", "chunk649")
    save(622, ch622)

    ch474 = load(474)
    ch474["managedSourceFiles"] = [
        f
        for f in ch474.get("managedSourceFiles") or []
        if not f.endswith(
            (
                "VmPMapTestMobj.cs",
                "VmPMapCheckPosition.cs",
                "VmPMapPortalMissile.cs",
            )
        )
    ]
    save(474, ch474)

    ch591 = load(591)
    ch591["managedSourceFiles"] = ["csharp/src/HCDE.Playsim/VmAcsBehaviorVarSerialize.cs"]
    ch591["auditPaddingReview"] = trim_audit(
        ch591.get("auditPaddingReview") or "", "<!-- merged from chunk586 -->"
    )
    save(591, ch591)

    ch601 = load(601)
    ch601["managedSourceFiles"] = ["csharp/src/HCDE.Net.Core/VmDNetDoCommandChat.cs"]
    ch601["note"] = ch601.get("note", "").replace(" Merged: [542].", "")
    ch601["auditPaddingReview"] = trim_audit(
        ch601.get("auditPaddingReview") or "", "\n\n<!-- merged from chunk602 -->"
    )
    save(601, ch601)

    ch509 = load(509)
    ch509["auditManagedLocJustification"] = JUSTIFICATIONS[509]
    save(509, ch509)

    ch475 = load(475)
    for r in ch475.get("nativeRanges") or []:
        s, e = int(r["start"]), int(r["end"])
        if s == 3902 and e == 3907:
            r["tests"] = ["VmPMapAimTraverseTests.AimTraverseInit_CeilingPortal"]
        elif 4015 <= s <= 4109:
            tests = list(r.get("tests") or [])
            if "VmPMapAimTraverseTests.ThreeDFloor_BlocksWhenPitchRangeClosed" not in tests:
                tests.append("VmPMapAimTraverseTests.ThreeDFloor_BlocksWhenPitchRangeClosed")
            r["tests"] = tests
        elif 4117 <= s <= 4140:
            r["tests"] = ["VmPMapAimTraverseTests.SectorPortal_SkipsHighCeilingPortal"]
        elif 4148 <= s <= 4177:
            r["tests"] = ["VmPMapAimTraverseTests.LinePortal_BlocksWhenRestricted"]
        elif s >= 4261:
            r["tests"] = ["VmPMapAimTraverseTests.MergeAimResults_PrefersCloserFrac"]
    save(475, ch475)

    ch570 = load(570)
    for r in ch570.get("nativeRanges") or []:
        if int(r["start"]) >= 5202:
            r["tests"] = [
                "VmDNetRuntimeClientConnectTests.ClientConnectingShouldReturnEarly_WhenSetupInProgress",
                "VmDNetRuntimeClientConnectTests.HGetPacketRejectsLengthMismatch_WhenSizesDiffer",
            ]
    save(570, ch570)

    for num, text in JUSTIFICATIONS.items():
        ch = load(num)
        if ch.get("retired"):
            continue
        ch["auditManagedLocJustification"] = text
        save(num, ch)

    tally = json.loads(TALLY.read_text(encoding="utf-8"))
    for c in tally["chunks"]:
        if c.get("name") == "i-net-launcher-pregame-ui-chunk648":
            c["name"] = "i-net-launcher-pregame-ui-chunk649"
        if "chunk648" in c.get("note", ""):
            c["note"] = c["note"].replace("chunk648", "chunk649")
    TALLY.write_text(json.dumps(tally, indent=2) + "\n", encoding="utf-8")
    print("hygiene applied")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
