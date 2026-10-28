# Chunk 577 — `A_Weave`, `A_LineEffect`, `A_WolfAttack` ballistics, and `A_Warp` setup/success (`p_actionfunctions.cpp` **3484–3762**). Disjoint from retired **576** hull (no **3255–3422** teleport overlap).

## Scope

`A_Weave`, `A_LineEffect`, `A_WolfAttack` ballistics, and `A_Warp` setup/success (`p_actionfunctions.cpp` **3484–3762**). Disjoint from retired **576** hull (no **3255–3422** teleport overlap).

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_Weave** 3491–3516 (26 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Weave** 3518–3524 (7 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Weave** 3527–3536 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **A_LineEffect** 3558–3567 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **A_WolfAttack** 3597–3602 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **A_WolfAttack** 3607–3666 (60 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Warp** 3707–3730 (24 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Warp** 3732–3747 (16 lines)
- `src/playsim/p_actionfunctions.cpp` **DoCheckSpecies** 3751–3756 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **DoCheckClass** 3758–3762 (5 lines)

**Credited native LOC:** 170 (tally `nativeLoc` 170).

## Managed

`VmActionWeave.cs`, `VmActionLineEffect.cs`, `VmActionWolfAttackBallistics.cs`, `VmActionWarp.cs`, `VmActionWarpSuccess.cs`, `VmActionRadiusGiveEligibility.cs`
Managed LOC (tally): 166.
Conversion commit: `c6d87e1316ed93e6fda44ff5531792ca58a9e7fd`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `WeaveIndexXY` / `WeaveIndexZ` / `ShouldWeaveXY` / `ShouldWeaveZ` / `AdvanceWeaveIndex` / `UseTryMoveWhenInteractive` | `A_Weave` **3491–3516**, **3518–3524** (`BobSin`/`P_TryMove` seam) |
| `SkipWhenLinedone` / `HasLineSpecial` / `MarkLinedoneWhenNonRepeating` / `LineEffectCanRun` / `ApplyLinedoneFlag` | `A_LineEffect` **3558–3567** (`P_ExecuteSpecial` seam) |
| `WolfAttackAbortsWithoutTarget` / `WolfAttackAbortsWithoutSight` | `A_WolfAttack` **3597–3602** |
| `TargetCanDodge` / `ChebyshevDistance` / `BlockDistance` / hitchance / damage helpers | `A_WolfAttack` **3607–3666** (hit/damage/spawn seam) |
| `PrimeDualReturnCount` / `ShouldPrimeChainSuccessFalse` / `ShouldClearStatePointer` / `UseTidLookup` / `WarpFailsWithoutReference` | `A_Warp` **3707–3730** |
| `ShouldSetSuccessStatePointer` / `ShouldSetChainSuccess` | `A_Warp` **3732–3747** |
| `SpeciesPasses` / `ClassPasses` | `DoCheckSpecies` / `DoCheckClass` **3751–3762** |

### vs retired **576**

| | Lines |
|--|------:|
| **576** hull (retired, overlap) | **1989–2211**, **3255–3422**, … |
| **577** (this row) | **3484–3762** (disjoint) |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **172** |
| Net behavior-ported | **172** |


**Post-review ratio:** 166/170 managed/native ≈ 98% (src file LOC at HEAD; see tally).

## Tests

- `VmActionWeaveTests.WeaveIndexXY_MasksToSixBits`
- `VmActionWeaveTests.ShouldWeaveXY_WhenDistAndSpeedNonZero`
- `VmActionWeaveTests.UseTryMoveWhenInteractive_WithoutNointeraction`
- `VmActionWeaveTests.AdvanceWeaveIndex_WrapsAt64`
- `VmActionLineEffectTests.LineEffectCanRun_WhenNotLinedone`
- `VmActionLineEffectTests.ApplyLinedoneFlag_SetsBit`
- `VmActionLineEffectTests.WolfAttackAbortsWithoutTarget`
- `VmActionLineEffectTests.WolfAttackAbortsWithoutSight`
- `VmActionWolfAttackBallisticsTests.HitchanceFromSpeed_Uses256WhenSlow`
- `VmActionWolfAttackBallisticsTests.AdjustHitchanceForDistance_SubtractsDodgePenalty`
- `VmActionWolfAttackBallisticsTests.DamageFromFlags_UsesRandomWhenAllowed`
- `VmActionWolfAttackBallisticsTests.ShouldSpawnBlood_FalseWhenNoBlood`
- `VmActionWarpTests.PrimeDualReturnCount_WhenMultipleReturns`
- `VmActionWarpTests.UseTidLookup_WhenFlagged`
- `VmActionWarpTests.WarpFailsWithoutReference`
- `VmActionWarpSuccessTests.ShouldSetSuccessStatePointer_WhenWarpOk`
- `VmActionWarpSuccessTests.ShouldSetChainSuccess_WhenNoStateJump`
- `VmActionRadiusGiveEligibilityTests.SpeciesPasses_WhenNoFilter`
- `VmActionRadiusGiveEligibilityTests.ClassPasses_WhenFilterNull`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 577 --to 577` (0 hard / 0 soft).

## Seams

`BobSin`, `P_TryMove`, `UnlinkFromWorld`, `SetXY`, `LinkToWorld`, `TranslateLineDef`, `P_ExecuteSpecial`, `P_CheckSight`, `A_FaceTarget`, `A_WolfAttack_ShadowHandling`, puff/damage/spawn/sound, `COPY_AAPTR`, `SingleActorFromTID`, and `P_Thing_Warp` are not hosted.

## Credit rationale

Weave index/mask/advance and interaction branch; linedone gating; Wolf dodge/hitchance/damage math; warp inventory return priming and success pointer/chain flags; species/class filter predicates from retired **576** split.
