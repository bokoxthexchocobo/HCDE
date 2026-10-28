#!/usr/bin/env python3
"""One-shot backfill: legacy headline chunks -> nativeRanges + conversionManagedCommit."""

from __future__ import annotations

import json
import re
import subprocess
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
TALLY = REPO / "csharp/docs/data/loc_tally.json"
LNSPEC = REPO / "src/playsim/p_lnspec.cpp"
P_ACS = REPO / "src/playsim/p_acs.cpp"
P_MAP = REPO / "src/playsim/p_map.cpp"
P_TRACE = REPO / "src/playsim/p_trace.cpp"
P_ENEMY = REPO / "src/playsim/p_enemy.cpp"
P_ACTION = REPO / "src/playsim/p_actionfunctions.cpp"
P_MOBJ = REPO / "src/playsim/p_mobj.cpp"


def first_commit(path: str) -> str:
    out = subprocess.check_output(
        ["git", "log", "--format=%H", "--reverse", "--", path],
        cwd=REPO,
        text=True,
    ).strip()
    return out.splitlines()[0] if out else ""


def func_ranges(cpp: Path, func_names: list[str], tests: list[str]) -> list[dict]:
    text = cpp.read_text(encoding="utf-8", errors="replace")
    lines = text.splitlines()
    pat = re.compile(r"^FUNC\((\w+)\)")
    indices: dict[str, int] = {}
    for i, line in enumerate(lines, start=1):
        m = pat.match(line)
        if m:
            indices[m.group(1)] = i
    ordered = sorted(indices.items(), key=lambda x: x[1])
    next_start = {name: ordered[i + 1][1] for i, (name, _) in enumerate(ordered[:-1])}
    file_rel = str(cpp.relative_to(REPO)).replace("\\", "/")
    out: list[dict] = []
    for name in func_names:
        if name not in indices:
            raise SystemExit(f"FUNC {name} not found in {cpp}")
        start = indices[name]
        end = next_start.get(name, start) - 1
        out.append(
            {
                "file": file_rel,
                "symbol": name,
                "start": start,
                "end": end,
                "tests": tests,
            }
        )
    return out


def manual_range(file_rel: str, symbol: str, start: int, end: int, tests: list[str]) -> dict:
    return {"file": file_rel, "symbol": symbol, "start": start, "end": end, "tests": tests}


def define_action_ranges(cpp: Path, action_names: list[str], tests: list[str]) -> list[dict]:
    text = cpp.read_text(encoding="utf-8", errors="replace")
    lines = text.splitlines()
    pat = re.compile(r"^DEFINE_ACTION_FUNCTION(?:_NATIVE)?\(AActor, (\w+)\)")
    indices: dict[str, int] = {}
    for i, line in enumerate(lines, start=1):
        m = pat.match(line)
        if m:
            indices[m.group(1)] = i
    ordered = sorted(indices.items(), key=lambda x: x[1])
    next_start = {name: ordered[i + 1][1] for i, (name, _) in enumerate(ordered[:-1])}
    file_rel = str(cpp.relative_to(REPO)).replace("\\", "/")
    out: list[dict] = []
    for name in action_names:
        if name not in indices:
            raise SystemExit(f"DEFINE_ACTION_FUNCTION {name} not found in {cpp}")
        start = indices[name]
        end = next_start.get(name, start + 40) - 1
        out.append({"file": file_rel, "symbol": name, "start": start, "end": end, "tests": tests})
    return out


def main() -> int:
    data = json.loads(TALLY.read_text(encoding="utf-8"))
    by_name = {c["name"]: c for c in data["chunks"]}

    managed_commit: dict[str, list[str]] = {
        "acs-random-getcvar-polywait": ["csharp/src/HCDE.Playsim/AcsVm.cs"],
        "combat-trace-ordering": ["csharp/src/HCDE.Playsim/CombatTrace.cs"],
        "acs-map-mutations": ["csharp/src/HCDE.Playsim/AcsMapMutations.cs"],
        "line-specials-plat-generic-elevator": ["csharp/src/HCDE.Playsim/LineSpecials.cs"],
        "line-specials-gap-closure": ["csharp/src/HCDE.Playsim/SectorEnvironmentLineSpecials.cs"],
        "acs-callfunc-wire-dispatch-udmf": ["csharp/src/HCDE.Playsim/AcsCallFunctions.cs"],
        "action-combat-port": ["csharp/src/HCDE.Playsim/ActorCombatActions.cs"],
        "acs-hot-callfunc-behavior": ["csharp/src/HCDE.Playsim/AcsCallFunctionsExtended.cs"],
        "sector-changeflags-gap-audit": ["csharp/src/HCDE.Playsim/LineSpecials.cs"],
        "line-special-feasible-gaps": ["csharp/src/HCDE.Playsim/LineSpecialFeasibleGaps.cs"],
        "action-dispatch-damage-wiring": ["csharp/src/HCDE.Playsim/ActorActionDispatch.cs"],
        "action-combat-dispatch-batch2": ["csharp/src/HCDE.Playsim/ActorCombatActions.cs"],
        "action-explode-radius-give": ["csharp/src/HCDE.Playsim/ActorExplodeActions.cs"],
        "thing-projectile-175-178": ["csharp/src/HCDE.Playsim/ThingProjectileLineSpecials.cs"],
        "acs-air-userarray-spawnspot": ["csharp/src/HCDE.Playsim/AcsCallFunctionsExtended.cs"],
        "p-enemy-chunk1": ["csharp/src/HCDE.Playsim/ActorLook2Actions.cs"],
        "p-map-radius-chunk1": ["csharp/src/HCDE.Playsim/MapRadiusAttack.cs"],
    }

    plat_funcs = [
        "LS_Door_WaitRaise",
        "LS_Door_WaitClose",
        "LS_Generic_Door",
        "LS_Floor_RaiseByTexture",
        "LS_Floor_LowerByTexture",
        "LS_Generic_Floor",
        "LS_Pillar_Build",
        "LS_Pillar_BuildAndCrush",
        "LS_Pillar_Open",
        "LS_Ceiling_RaiseByTexture",
        "LS_Ceiling_LowerByTexture",
        "LS_Generic_Ceiling",
        "LS_Generic_Crusher",
        "LS_Generic_Crusher2",
        "LS_Generic_CrusherDist",
        "LS_Plat_PerpetualRaise",
        "LS_Plat_PerpetualRaiseLip",
        "LS_Plat_Stop",
        "LS_Plat_DownWaitUpStay",
        "LS_Plat_DownWaitUpStayLip",
        "LS_Plat_DownByValue",
        "LS_Plat_UpByValue",
        "LS_Plat_UpWaitDownStay",
        "LS_Plat_UpNearestWaitDownStay",
        "LS_Plat_RaiseAndStayTx0",
        "LS_Plat_UpByValueStayTx",
        "LS_Plat_ToggleCeiling",
        "LS_Generic_Lift",
        "LS_Elevator_MoveToFloor",
        "LS_Elevator_RaiseToNearest",
        "LS_Elevator_LowerToNearest",
    ]

    gap_funcs = [
        "LS_FloorAndCeiling_LowerByValue",
        "LS_FloorAndCeiling_RaiseByValue",
        "LS_Sector_SetFriction",
        "LS_Sector_SetWind",
        "LS_Sector_SetCurrent",
        "LS_Sector_ChangeSound",
        "LS_Sector_ChangeFlags",
        "LS_Sector_SetTranslucent",
        "LS_TranslucentLine",
        "LS_Thing_Spawn",
        "LS_Thing_SpawnFacing",
        "LS_Thing_Projectile",
        "LS_Thing_ProjectileGravity",
        "LS_Thing_ProjectileAimed",
        "LS_Thing_ProjectileIntercept",
    ]

    feasible_funcs = [
        "LS_Floor_Waggle",
        "LS_Sector_SetWind",
        "LS_Thing_SetTranslation",
        "LS_Thing_SpawnFacing",
    ]

    backfill: dict[str, list[dict]] = {
        "acs-random-getcvar-polywait": [
            manual_range("src/playsim/p_acs.cpp", "DLevelScript::Random", 3664, 3672, ["AcsRandomAndCvarOpcodeTests"]),
            manual_range("src/playsim/p_acs.cpp", "PCD_RANDOM", 8602, 8615, ["AcsRandomAndCvarOpcodeTests"]),
            manual_range("src/playsim/p_acs.cpp", "PCD_POLYWAIT", 8654, 8664, ["AcsRandomAndCvarOpcodeTests"]),
            manual_range("src/playsim/p_acs.cpp", "PCD_GETCVAR", 10283, 10286, ["AcsRandomAndCvarOpcodeTests"]),
        ],
        "combat-trace-ordering": [
            manual_range("src/playsim/p_trace.cpp", "FTraceInfo combat intercept region", 200, 479, [
                "ActorPlaneTraversalOrderingTests",
                "TraceSectorEntryTests",
                "PlaneWallImpactOrderingTests",
            ]),
        ],
        "line-specials-plat-generic-elevator": func_ranges(
            LNSPEC, plat_funcs, ["LineSpecialBatchTests"]
        ),
        "line-specials-gap-closure": func_ranges(LNSPEC, gap_funcs, ["LineSpecialClosureTests"]),
        "line-special-feasible-gaps": func_ranges(
            LNSPEC, feasible_funcs, ["WaggleActionsTests", "PusherWindTests", "ThingProjectileLineSpecialsTests"]
        ),
        "p-map-radius-chunk1": [
            manual_range("src/playsim/p_map.cpp", "GetRadiusDamage", 6130, 6202, ["MapRadiusAttackTests"]),
            manual_range("src/playsim/p_map.cpp", "GetOldRadiusDamage", 6215, 6249, ["MapRadiusAttackTests"]),
            manual_range("src/playsim/p_map.cpp", "P_GetRadiusDamage", 6259, 6282, ["MapRadiusAttackTests"]),
            manual_range("src/playsim/p_map.cpp", "P_RadiusAttack target collection", 6291, 6389, ["MapRadiusAttackTests"]),
        ],
        "p-enemy-chunk1": [
            manual_range("src/playsim/p_enemy.cpp", "A_ClearLastHeard", 2255, 2260, ["PEnemyActionsLoadedMapTests"]),
            manual_range("src/playsim/p_enemy.cpp", "A_Look2", 2330, 2381, ["PEnemyActionsLoadedMapTests"]),
            manual_range("src/playsim/p_enemy.cpp", "A_ChaseNative", 2980, 2998, ["PEnemyActionsLoadedMapTests"]),
            manual_range("src/playsim/p_enemy.cpp", "A_MonsterRail", 3155, 3196, ["PEnemyActionsLoadedMapTests"]),
            manual_range("src/playsim/p_enemy.cpp", "A_Pain", 3217, 3269, ["PEnemyActionsLoadedMapTests"]),
            manual_range("src/playsim/p_enemy.cpp", "CheckBossDeath", 3271, 3308, ["PEnemyActionsLoadedMapTests"]),
            manual_range("src/playsim/p_enemy.cpp", "A_BossDeath", 3314, 3395, ["PEnemyActionsLoadedMapTests"]),
            manual_range("src/playsim/p_enemy.cpp", "A_ClassBossHealth", 3476, 3488, ["PEnemyActionsLoadedMapTests"]),
        ],
        "acs-map-mutations": [
            manual_range("src/playsim/p_acs.cpp", "DLevelScript::ChangeFlat", 3755, 3783, ["AcsMapMutationOpcodeTests"]),
            manual_range("src/playsim/p_acs.cpp", "DLevelScript::SetLineTexture", 3785, 3823, ["AcsMapMutationOpcodeTests"]),
            manual_range("src/playsim/p_acs.cpp", "DLevelScript::DoSpawn", 3825, 3872, ["AcsMapMutationOpcodeTests"]),
            manual_range("src/playsim/p_acs.cpp", "DLevelScript::DoSpawnSpot", 3874, 3893, ["AcsMapMutationOpcodeTests"]),
            manual_range("src/playsim/p_acs.cpp", "PCD_CHANGEFLOOR", 8666, 8674, ["AcsMapMutationOpcodeTests"]),
            manual_range("src/playsim/p_acs.cpp", "PCD_SETRESULTVALUE", 8568, 8573, ["AcsMapMutationOpcodeTests"]),
        ],
        "acs-callfunc-wire-dispatch-udmf": [
            manual_range("src/playsim/p_acs.cpp", "ACSF_GetLineUDMFInt", 5448, 5478, ["AcsCallFunctionWireDispatchTests"]),
        ],
        "acs-hot-callfunc-behavior": [
            manual_range("src/playsim/p_acs.cpp", "ACSF_SpawnForced", 5675, 5688, ["AcsHotCallFuncBehaviorTests"]),
            manual_range("src/playsim/p_acs.cpp", "ACSF_GetThingUDMFInt", 5456, 5460, ["AcsHotCallFuncBehaviorTests"]),
        ],
        "acs-air-userarray-spawnspot": [
            manual_range("src/playsim/p_acs.cpp", "ACSF_GetAirSupply", 5592, 5605, ["AcsHotCallFuncBehaviorTests"]),
            manual_range("src/playsim/p_acs.cpp", "ACSF_SetAirSupply", 5606, 5619, ["AcsHotCallFuncBehaviorTests"]),
            manual_range("src/playsim/p_acs.cpp", "ACSF_SpawnSpotForced", 5683, 5690, ["AcsHotCallFuncBehaviorTests"]),
            manual_range("src/playsim/p_acs.cpp", "ACSF_SetUserArray", 5756, 5775, ["AcsHotCallFuncBehaviorTests"]),
        ],
        "action-combat-port": define_action_ranges(
            P_ACTION,
            ["A_CustomMeleeAttack", "A_CustomComboAttack", "A_BulletAttack", "A_SpawnProjectile", "A_MonsterRefire"],
            ["ActionCombatLoadedMapTests"],
        ),
        "action-combat-dispatch-batch2": define_action_ranges(
            P_ACTION,
            ["A_SpawnProjectile", "A_BulletAttack", "A_MonsterRefire"],
            ["ActionCombatLoadedMapTests"],
        ),
        "action-dispatch-damage-wiring": [
            manual_range(
                "src/playsim/p_actionfunctions.cpp",
                "ActorActionDispatch wiring (managed)",
                1172,
                1202,
                ["ActionCombatLoadedMapTests", "ActionExplodeLoadedMapTests"],
            ),
        ],
        "action-explode-radius-give": [
            manual_range("src/playsim/p_actionfunctions.cpp", "DoRadiusGive", 3807, 3939, ["ActionExplodeLoadedMapTests"]),
            manual_range("src/playsim/p_actionfunctions.cpp", "A_RadiusGive", 3941, 3985, ["ActionExplodeLoadedMapTests"]),
            manual_range("src/playsim/p_map.cpp", "P_RadiusAttack damage loop", 6368, 6400, ["ActionExplodeLoadedMapTests"]),
        ],
        "thing-projectile-175-178": func_ranges(
            LNSPEC,
            ["LS_Thing_Projectile", "LS_Thing_ProjectileGravity", "LS_Thing_ProjectileAimed", "LS_Thing_ProjectileIntercept"],
            ["ThingProjectileLineSpecialsTests"],
        ),
        "sector-changeflags-gap-audit": func_ranges(
            LNSPEC, ["LS_Sector_ChangeFlags"], ["SectorChangeFlagsLoadedMapTests"]
        ),
    }

    conversion_paths = {
        **managed_commit,
        "action-combat-port": ["csharp/src/HCDE.Playsim/ActorCombatActions.cs"],
        "action-combat-dispatch-batch2": ["csharp/src/HCDE.Playsim/ActorCombatActions.cs"],
        "action-dispatch-damage-wiring": ["csharp/src/HCDE.Playsim/ActorActionDispatch.cs"],
        "action-explode-radius-give": ["csharp/src/HCDE.Playsim/ActorExplodeActions.cs"],
        "thing-projectile-175-178": ["csharp/src/HCDE.Playsim/ThingProjectileLineSpecials.cs"],
    }

    for name, ranges in backfill.items():
        chunk = by_name[name]
        chunk["bucket"] = "conversion"
        chunk["nativeRanges"] = ranges
        paths = conversion_paths.get(name, managed_commit.get(name, []))
        if paths:
            chunk["conversionManagedCommit"] = first_commit(paths[0])

    def bucket(chunk: dict) -> str:
        b = chunk.get("bucket")
        if b:
            return b
        return "recognition" if "recognition" in chunk.get("name", "") else "conversion"

    default_managed = {
        "p-map-radius-thrust-chunk2": "csharp/src/HCDE.Playsim/MapRadiusAttack.cs",
        "p-enemy-move-blocked-chunk2": "csharp/src/HCDE.Playsim/MonsterBlockedMoveActions.cs",
        "p-mobj-missile-spawn-live": "csharp/src/HCDE.Playsim/MissileSpawnCheck.cs",
        "g-level-headless-flow-chunk8": "csharp/src/HCDE.Server/HeadlessLevelFlow.cs",
        "g-game-headless-ticker-chunk9": "csharp/src/HCDE.Server/HeadlessGameFlow.cs",
        "hcde-edf-thingtype-chunk18": "csharp/src/HCDE.Gamedata/HcdeEdfThingParser.cs",
        "acs-wire-printbind-chunk16": "csharp/src/HCDE.Playsim/AcsVm.WireDispatch.cs",
    }
    for chunk in data["chunks"]:
        if bucket(chunk) != "conversion" or chunk.get("nativeLoc", 0) <= 0:
            continue
        if "conversionManagedCommit" in chunk:
            continue
        path = default_managed.get(chunk["name"])
        if path:
            chunk["conversionManagedCommit"] = first_commit(path)

    # Chunk 19 -> recognition (pre-existing ActorSoundActions)
    c19 = by_name["p-mobj-play-spawn-sound-chunk19"]
    c19["bucket"] = "recognition"
    c19["managedLoc"] = 0
    c19.pop("conversionManagedCommit", None)
    if "nativeRanges" not in c19:
        c19["nativeRanges"] = [
            manual_range(
                "src/playsim/p_mobj.cpp",
                "P_PlaySpawnSound",
                7880,
                7900,
                ["ProjectileSpawnSoundRoutingTests"],
            )
        ]

    TALLY.write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")
    print("Backfilled", len(backfill), "legacy chunks; fixed chunk 19 recognition")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
