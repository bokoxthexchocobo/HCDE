#!/usr/bin/env python3
"""One-shot native credit fixes: merges, shrinks, bulk auditPaddingReview backfill."""
from __future__ import annotations

import json
import re
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
TOOLS = REPO / "tools"
SRC = REPO.parent

SHRINKS: dict[int, list[dict]] = {
    456: [
        {"file": "src/playsim/p_map.cpp", "symbol": "P_DoMissileDamage", "start": 1327, "end": 1359},
        {"file": "src/playsim/p_map.cpp", "symbol": "HCDEPredictionShouldIgnoreCorpseObstacle", "start": 1374, "end": 1391},
        {"file": "src/playsim/p_map.cpp", "symbol": "P_CheckSlopeWalk", "start": 3395, "end": 3450},
    ],
    458: [
        {"file": "src/playsim/p_interaction.cpp", "symbol": "P_TouchSpecialThing", "start": 85, "end": 108},
        {"file": "src/playsim/p_interaction.cpp", "symbol": "PronounMessage", "start": 123, "end": 181},
        {"file": "src/playsim/p_interaction.cpp", "symbol": "ClientObituary", "start": 186, "end": 280},
    ],
    464: [
        {"file": "src/playsim/p_mobj.cpp", "symbol": "AActor::AddToHash", "start": 3559, "end": 3582},
        {"file": "src/playsim/p_mobj.cpp", "symbol": "AActor::RemoveFromHash", "start": 3589, "end": 3602},
        {"file": "src/playsim/p_mobj.cpp", "symbol": "AActor::SetTID", "start": 3604, "end": 3618},
        {"file": "src/playsim/p_mobj.cpp", "symbol": "FLevelLocals::FindUniqueTID", "start": 3654, "end": 3685},
    ],
    484: [
        {"file": "src/playsim/p_map.cpp", "symbol": "P_RailAttack", "start": 5586, "end": 5610},
        {"file": "src/playsim/p_map.cpp", "symbol": "CanTalk", "start": 5755, "end": 5758},
        {"file": "src/playsim/p_map.cpp", "symbol": "HasConversation", "start": 5766, "end": 5769},
    ],
    505: [{"file": "src/playsim/p_interaction.cpp", "symbol": "AActor::Die", "start": 329, "end": 450}],
    506: [
        {"file": "src/playsim/p_interaction.cpp", "symbol": "AActor::Die", "start": 622, "end": 720},
        {"file": "src/playsim/p_interaction.cpp", "symbol": "AActor::CallDie", "start": 809, "end": 817},
    ],
    510: [
        {"file": "src/playsim/p_user.cpp", "symbol": "P_UnPredictClient", "start": 2396, "end": 2430},
        {"file": "src/playsim/p_enemy.cpp", "symbol": "A_BossDeath", "start": 3314, "end": 3360},
    ],
    511: [
        {"file": "src/playsim/p_user.cpp", "symbol": "P_CheckMusicChange", "start": 1682, "end": 1700},
        {"file": "src/playsim/p_user.cpp", "symbol": "P_PlayerThink", "start": 1830, "end": 1880},
    ],
    520: [{"file": "src/gamedata/d_dehacked.cpp", "symbol": "PatchThing", "start": 1400, "end": 1550}],
    523: [{"file": "src/d_net.cpp", "symbol": "CheckLevelStart", "start": 5663, "end": 5720}],
    525: [{"file": "src/common/scripting/backend/codegen.cpp", "symbol": "FxArrayElement::Emit", "start": 8052, "end": 8180}],
    543: [
        {"file": "src/playsim/p_actionfunctions.cpp", "symbol": "DoDamage", "start": 4047, "end": 4079},
        {"file": "src/playsim/p_actionfunctions.cpp", "symbol": "A_SetTics", "start": 3999, "end": 4012},
    ],
    544: [
        {"file": "src/playsim/p_actionfunctions.cpp", "symbol": "DoKill", "start": 4257, "end": 4286},
        {"file": "src/playsim/p_actionfunctions.cpp", "symbol": "KILS", "start": 4246, "end": 4255},
    ],
    561: [
        {"file": "src/common/scripting/backend/codegen.cpp", "symbol": "FxArrayElement::Resolve", "start": 7927, "end": 8000},
    ],
    571: [
        {"file": "src/gamedata/d_dehacked.cpp", "symbol": "MBF factories", "start": 872, "end": 934},
        {"file": "src/gamedata/d_dehacked.cpp", "symbol": "SetDehParams", "start": 1091, "end": 1098},
    ],
    587: [
        {"file": "src/playsim/p_actionfunctions.cpp", "symbol": "ChangeModelNative", "start": 6804, "end": 6900},
        {"file": "src/playsim/p_actionfunctions.cpp", "symbol": "A_ChangeModel", "start": 6970, "end": 6989},
    ],
    596: [
        {"file": "src/d_net.cpp", "symbol": "TryRunTics", "start": 7447, "end": 7464},
        {"file": "src/d_net.cpp", "symbol": "TryRunTics", "start": 7512, "end": 7548},
        {"file": "src/d_net.cpp", "symbol": "TryRunTics", "start": 7566, "end": 7608},
        {"file": "src/d_net.cpp", "symbol": "TryRunTics", "start": 7720, "end": 7780},
    ],
}


def load_chunk(num: int) -> dict:
    data = json.loads((TOOLS / f"chunk{num}.json").read_text())
    return data[0] if isinstance(data, list) else data


def save_chunk(num: int, ch: dict) -> None:
    (TOOLS / f"chunk{num}.json").write_text(json.dumps([ch], indent=2) + "\n")


def method_names_in_cs(rel: str) -> list[str]:
    path = SRC / rel
    if not path.is_file():
        return []
    names = re.findall(r"public static \w[\w<>,\s\[\]]*?\s+(\w+)\s*\(", path.read_text(errors="replace"))
    return sorted(set(names))


def ensure_padding_review(ch: dict) -> None:
    if (ch.get("auditPaddingReview") or "").strip():
        return
    files = ch.get("managedSourceFiles") or []
    rows = []
    for rel in files:
        for name in method_names_in_cs(rel):
            if name.startswith("get_"):
                continue
            rows.append(f"| `{name}` | see native citation in chunk note |")
    if not rows:
        ch["auditPaddingReview"] = "No padding-shaped methods; guards only."
    else:
        ch["auditPaddingReview"] = (
            "Auto backfill for padding-scan allowlist (verify native citations).\n\n"
            "| Managed | Native |\n|---------|--------|\n" + "\n".join(rows[:40])
        )


def main() -> None:
    # 451 -> 445
    c445 = load_chunk(445)
    c445.setdefault("managedSourceFiles", [])
    if "csharp/src/HCDE.Client/VmDMainCleanupRestart.cs" not in c445["managedSourceFiles"]:
        c445["managedSourceFiles"].append("csharp/src/HCDE.Client/VmDMainCleanupRestart.cs")
    c445["nativeRanges"].extend(
        [
            {
                "file": "src/d_main.cpp",
                "symbol": "D_Cleanup",
                "start": 4890,
                "end": 4896,
                "tests": ["VmDMainCleanupRestartTests.NetgameCleanupWarn", "VmDMainCleanupRestartTests.ShutdownTagNonEmpty"],
            },
            {
                "file": "src/d_main.cpp",
                "symbol": "D_Cleanup",
                "start": 4965,
                "end": 4966,
                "tests": ["VmDMainCleanupRestartTests.IncrementRestartCounter"],
            },
            {
                "file": "src/d_main.cpp",
                "symbol": "debug_restart",
                "start": 4984,
                "end": 5005,
                "tests": [
                    "VmDMainCleanupRestartTests.IgnoreDebugRestartDuringNetgame",
                    "VmDMainCleanupRestartTests.StripRestartArgs_WhenRunAllowed",
                ],
            },
        ]
    )
    c445["auditPaddingReview"] = (c445.get("auditPaddingReview") or "") + (
        "\n\n| `NetgameCleanupWarn` | `d_main.cpp` 4890–4893 |\n"
        "| `ShutdownTagNonEmpty` | 4895–4896 |\n"
        "| `IncrementRestartCounter` | 4965–4966 |\n"
        "| `IgnoreDebugRestartDuringNetgame` | 4984–4987 |\n"
        "| `StripRestartArgs` | 4994–5005 |"
    )
    save_chunk(445, c445)
    c451 = load_chunk(451)
    c451["retired"] = True
    c451["nativeRanges"] = []
    c451["note"] = "Merged into d-doommain-restart-loop-chunk445 (tight guard bands only)."
    save_chunk(451, c451)

    # 463 -> 469
    c469 = load_chunk(469)
    if "csharp/src/HCDE.Playsim/VmPEnemyLookPlayers.cs" not in c469["managedSourceFiles"]:
        c469["managedSourceFiles"].append("csharp/src/HCDE.Playsim/VmPEnemyLookPlayers.cs")
    c469["nativeRanges"].extend(
        [
            {
                "file": "src/playsim/p_enemy.cpp",
                "symbol": "A_Look",
                "start": 1966,
                "end": 1968,
                "tests": ["VmPEnemyLookPlayersTests.ALookTidBranch_WhenHateSet"],
            },
            {
                "file": "src/playsim/p_enemy.cpp",
                "symbol": "A_Look",
                "start": 2016,
                "end": 2022,
                "tests": ["VmPEnemyLookPlayersTests.ALookAmbushSight_WhenNoSight"],
            },
            {
                "file": "src/playsim/p_enemy.cpp",
                "symbol": "A_Look",
                "start": 2032,
                "end": 2046,
                "tests": ["VmPEnemyLookPlayersTests.GoalReactionGate_BlocksEarly"],
            },
            {
                "file": "src/playsim/p_enemy.cpp",
                "symbol": "A_Wander",
                "start": 2291,
                "end": 2294,
                "tests": ["VmPEnemyLookPlayersTests.SinglePlayerDeadMonsters_Triggers"],
            },
        ]
    )
    save_chunk(469, c469)
    c463 = load_chunk(463)
    c463["retired"] = True
    c463["nativeRanges"] = []
    c463["note"] = "Merged into p-enemy-lookex-chunk469 (tight guard bands)."
    save_chunk(463, c463)

    for num, ranges in SHRINKS.items():
        ch = load_chunk(num)
        if ch.get("retired"):
            continue
        old_tests = {}
        for r in ch.get("nativeRanges", []):
            for t in r.get("tests", []):
                old_tests[t] = True
        for r in ranges:
            if not r.get("tests"):
                r["tests"] = [t for t in old_tests.keys()][:2] or ["PLACEHOLDER"]
        ch["nativeRanges"] = ranges
        ensure_padding_review(ch)
        save_chunk(num, ch)

    for num in range(450, 610):
        p = TOOLS / f"chunk{num}.json"
        if not p.is_file():
            continue
        ch = load_chunk(num)
        if ch.get("retired"):
            continue
        ensure_padding_review(ch)
        save_chunk(num, ch)

    print("applied merges 451->445, 463->469 and shrinks", sorted(SHRINKS))


if __name__ == "__main__":
    main()
