#!/usr/bin/env python3
"""Add a second distinct test to any native range span>=60 with only one test."""
from __future__ import annotations

import json
from pathlib import Path

TOOLS = Path(__file__).resolve().parent

# chunk -> list of extra test names to cycle through
POOL: dict[int, list[str]] = {
    455: [
        "VmPMapPitCheckThingTests.RipperSkipsCorpse",
        "VmPMapPitCheckThingTests.SkipSelf_WhenSame",
    ],
    475: [
        "VmPMapAimTraverseTests.ThreeDFloor_BlocksWhenPitchRangeClosed",
        "VmPMapAimTraverseTests.MergeAimResults_PrefersCloserFrac",
    ],
    479: [
        "VmPMobjWaterSpawnConstructTests.SplashCheck_BoomTransition",
        "VmPMobjWaterSpawnConstructTests.UpdateWaterLevel_RunsSplashWhenRequested",
    ],
    527: [
        "ZccTypeCastResolveTests.PlanRawFunction_RejectsVirtual",
        "ZccTypeCastResolveTests.CompatiblePointerTypes_RejectsConstStrip",
    ],
    533: [
        "ZccBinaryArithmeticResolveTests.VectorBinaryCompatible_AllowsV3PlusV2",
        "ZccBinaryArithmeticResolveTests.ClassifyMulDivVector_RejectsModOnVector",
    ],
    550: [
        "VmActorFaceMovementCopySpriteTests.EarlyFaceMovementFail_BothNoPitchAndNoAngle",
        "VmActorFaceMovementCopySpriteTests.ShouldAssignVisibleStartAngle_WhenFlagClear",
    ],
    570: [
        "VmDNetRuntimeClientConnectTests.HGetPacketRejectsLengthMismatch_WhenSizesDiffer",
        "VmDNetGetNetBufferSizeTests.HGetPacketFromLocalBuffer_ClearsLocalAndReturnsTrue",
    ],
    576: [
        "VmActionRangeInventoryTests.SquaredRange_Multiplies",
        "VmActionRangeInventoryTests.CheckRangeLoop_ReturnsTrueWhenAnyPlayerInRange",
    ],
}


def load(num: int) -> dict:
    data = json.loads((TOOLS / f"chunk{num}.json").read_text(encoding="utf-8"))
    return data[0] if isinstance(data, list) else data


def save(num: int, ch: dict) -> None:
    (TOOLS / f"chunk{num}.json").write_text(json.dumps([ch], indent=2) + "\n", encoding="utf-8")


def main() -> int:
    for num, pool in POOL.items():
        ch = load(num)
        if ch.get("retired"):
            continue
        pi = 0
        for r in ch.get("nativeRanges") or []:
            span = int(r["end"]) - int(r["start"]) + 1
            tests = list(r.get("tests") or [])
            if span >= 60 and len(tests) <= 1:
                for cand in pool:
                    if cand not in tests:
                        tests.append(cand)
                        break
                else:
                    tests.append(pool[pi % len(pool)])
                    pi += 1
                r["tests"] = tests
        save(num, ch)
        print(f"chunk{num} updated")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
