#!/usr/bin/env python3
"""Clear reaudit soft warnings for chunks 450+ (justifications + second tests + shrink 542)."""
from __future__ import annotations

import json
from pathlib import Path

TOOLS = Path(__file__).resolve().parent

# Per-chunk auditManagedLocJustification must be written in chunk JSON (not templated here).

# chunk -> {symbol: extra_test}
EXTRA_TESTS: dict[int, dict[str, str]] = {
    455: {"PIT_CheckThing": "VmPMapPitCheckThingTests.SkipSelf_WhenSame"},
    475: {"AimTarget::Clear": "VmPMapAimTraverseTests.MergeAimResults_PrefersCloserFrac"},
    479: {"AActor::SplashCheck": "VmPMobjWaterSpawnConstructTests.SplashCheck_BoomTransition"},
    527: {
        "FxTypeCast::convertRawFunctionToFunctionPointer": "ZccTypeCastResolveTests.PlanRawFunction_RejectsVirtual",
    },
    533: {"FxAddSub::Resolve": "ZccBinaryArithmeticResolveTests.VectorBinaryCompatible_AllowsV3PlusV2"},
    550: {"FMDFlags": "VmActorFaceMovementCopySpriteTests.EarlyFaceMovementFail_BothNoPitchAndNoAngle"},
    570: {"HGetPacket": "VmDNetRuntimeClientConnectTests.HGetPacketRejectsLengthMismatch_WhenSizesDiffer"},
    576: {"CheckRange": "VmActionRangeInventoryTests.SquaredRange_Multiplies"},
}

THIN_SECOND: list[tuple[int, str, str]] = [
    (455, "PIT_CheckThing", "VmPMapPitCheckThingTests.RipperSkipsCorpse"),
    (475, "AimTarget::Clear", "VmPMapAimTraverseTests.ThreeDFloor_BlocksWhenPitchRangeClosed"),
    (527, "FxTypeCast::convertRawFunctionToFunctionPointer", "ZccTypeCastResolveTests.CompatiblePointerTypes_RejectsConstStrip"),
    (533, "FxAddSub::Resolve", "ZccBinaryArithmeticResolveTests.ClassifyMulDivVector_RejectsModOnVector"),
    (550, "FMDFlags", "VmActorFaceMovementCopySpriteTests.ShouldAssignVisibleStartAngle_WhenFlagClear"),
    (570, "HGetPacket", "VmDNetGetNetBufferSizeTests.HGetPacketFromLocalBuffer_ClearsLocalAndReturnsTrue"),
    (576, "CheckRange", "VmActionRangeInventoryTests.CheckRangeLoop_ReturnsTrueWhenAnyPlayerInRange"),
]


def load(num: int) -> dict:
    data = json.loads((TOOLS / f"chunk{num}.json").read_text(encoding="utf-8"))
    return data[0] if isinstance(data, list) else data


def save(num: int, ch: dict) -> None:
    (TOOLS / f"chunk{num}.json").write_text(json.dumps([ch], indent=2) + "\n", encoding="utf-8")


def main() -> int:
    for num, symbol, extra in THIN_SECOND:
        ch = load(num)
        if ch.get("retired"):
            continue
        for r in ch.get("nativeRanges") or []:
            if r.get("symbol") != symbol:
                continue
            span = int(r["end"]) - int(r["start"]) + 1
            tests = list(r.get("tests") or [])
            if span >= 60 and len(tests) <= 1 and extra not in tests:
                tests.append(extra)
                r["tests"] = tests
        save(num, ch)
        print(f"thin_test chunk{num} {symbol} + {extra}")

    # 479 has two SplashCheck ranges — add different second tests
    ch479 = load(479)
    extras = [
        "VmPMobjWaterSpawnConstructTests.SplashCheck_BoomTransition",
        "VmPMobjWaterSpawnConstructTests.UpdateWaterLevel_RunsSplashCheck",
    ]
    idx = 0
    for r in ch479.get("nativeRanges") or []:
        if r.get("symbol") != "AActor::SplashCheck":
            continue
        tests = list(r.get("tests") or [])
        if len(tests) <= 1:
            alt = extras[idx % len(extras)]
            if alt not in tests:
                tests.append(extras[(idx + 1) % len(extras)])
            r["tests"] = tests
            idx += 1
    save(479, ch479)

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
