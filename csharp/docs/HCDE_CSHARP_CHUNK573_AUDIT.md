# Chunk 573 — Print/log/recoil through fade/spawn/sight/particle (consolidated chunk573–575; p_actionfunctions.cpp 1381–1930)

## Scope

Print/log/recoil through fade/spawn/sight/particle (consolidated chunk573–575; p_actionfunctions.cpp 1381–1930)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_Recoil** 1381–1388 (8 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Print** 1405–1419 (15 lines)
- `src/playsim/p_actionfunctions.cpp` **A_PrintBold** 1443–1451 (9 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Log** 1470–1473 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **A_LogInt** 1490–1491 (2 lines)
- `src/playsim/p_actionfunctions.cpp` **A_LogFloat** 1507–1508 (2 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetTranslucent** 1525–1529 (5 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetRenderStyle** 1544–1545 (2 lines)
- `src/playsim/p_actionfunctions.cpp` **A_FadeIn** 1570–1583 (14 lines)
- `src/playsim/p_actionfunctions.cpp` **A_FadeOut** 1604–1615 (12 lines)
- `src/playsim/p_actionfunctions.cpp` **A_FadeTo** 1639–1662 (24 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SpawnDebris** 1685–1690 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SpawnDebris** 1694–1695 (2 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SpawnParticle** 1743–1746 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SpawnParticle** 1755–1761 (7 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SpawnParticleEx** 1804–1807 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SpawnParticleEx** 1817–1822 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SpawnParticleEx** 1838–1841 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **DoCheckSightOrRange** 1887–1914 (28 lines)
- `src/playsim/p_actionfunctions.cpp` **CheckSightOrRange** 1930–1930 (1 lines)

**Credited native LOC:** 159 (tally `nativeLoc` 159).

## Managed

`VmActionPrintLogRecoil.cs`, `VmActionFadeSpawn.cs`, `VmActionSightParticle.cs`
Managed LOC (tally): 205.
Conversion commit: `9f3c4c1828c7d3dfc7239aa0a34770c64bb3bbca`.

## Padding review (2026-10-21)

**Superseded chunk numbers:** 574, 575 (retired; credit lives only in chunk573.json / tally row `action-print-through-sight-chunk573`).

**Fidelity review (2026-10-21):** Native credit is whole functions where every credited line has a managed counterpart; seam-only lines (`1420–1423`, `1452–1454`, `1474`, `1491–1492`, `1508–1509`, `1585–1586`) are excluded.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `RecoilThrustAngleDegrees` | `A_Recoil` 1386–1387 |
| `ResolveDollarString` / `ShouldPrintToLocalView` / `SelectMidPrintDuration` / `WithTemporaryMidTime` | `A_Print` 1405–1419 |
| `ResolveDollarString` / `SelectMidPrintDuration` | `A_PrintBold` 1443–1451 |
| `ShouldSkipLocalLog` / `ResolveDollarString` | `A_Log` 1470–1473 |
| `ShouldSkipLocalLog` / `FormatLogInt` | `A_LogInt` 1490–1491 |
| `ShouldSkipLocalLog` / `FormatLogFloat` | `A_LogFloat` 1507–1508 |
| `MapTranslucentMode` / `ShouldClearAlpha1Flag` / `ClampAlpha` | `A_SetTranslucent` 1525–1529 |
| `ApplySetRenderStyle` | `A_SetRenderStyle` 1544–1545 |
| `FadeReduceDefault` / `ClearAlpha1RenderStyleFlag` / `AddFadeInReduce` / `StepFadeIn` | `A_FadeIn` 1570–1583 |

**Fidelity review (2026-10-21):** Excluded `P_RemoveThing` (`1618–1619`, `1664–1666`), debris `Spawn`/`SetState` (`1697–1707`), and `P_SpawnParticle` (`1772`).

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `FadeReduceDefault` / `StepFadeOut` | `A_FadeOut` 1604–1615 |
| `StepFadeToward` / `FinishFadeTo` | `A_FadeTo` 1639–1662 |
| `NormalizeDebrisMultipliers` / `DebrisOffsetFromPr` | `A_SpawnDebris` 1685–1690, 1694–1695 |
| `ClampParticleStartAlpha` / `ClampPositiveFadeStep` / `ShouldSpawnParticle` / `RotateRelPos` | `A_SpawnParticle` 1743–1746, 1755–1761 |

**Fidelity review (2026-10-21):** No credit for `CheckIfSeen` (`1854–1878`, all `P_CheckSight`). `CheckSightOrRange` credits only `range *= range` (`1930`); player loop is seam. `A_SpawnParticleEx` excludes `P_SpawnParticle` (`1843`).

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `ClampParticleStartAlpha` / `ClampFadeStep` / `AbsParticleSize` / `ShouldSpawnParticleEx` / `NormalizeParticleStyle` | `A_SpawnParticleEx` 1804–1807, 1838–1841 |
| `VerticalSightDelta` / `SquaredDistance2d` / `WithinRange` / `DoCheckSightOrRange` | `DoCheckSightOrRange` 1889–1914 |
| `SquaredRange` | `CheckSightOrRange` 1930 |

**Post-review ratio:** 205/159 managed/native ≈ 129% (src file LOC at HEAD; see tally).

## Tests

- `VmActionPrintLogRecoilTests.RecoilThrustAngleDegrees_AddsHalfTurn`
- `VmActionPrintLogRecoilTests.ResolveDollarString_LooksUpWhenPrefixed`
- `VmActionPrintLogRecoilTests.ShouldPrintToLocalView_WhenSelfOrTargetLocal`
- `VmActionPrintLogRecoilTests.WithTemporaryMidTime_SwapsDuration`
- `VmActionPrintLogRecoilTests.SelectMidPrintDuration_UsesDefaultWhenNonPositive`
- `VmActionPrintLogRecoilTests.ShouldSkipLocalLog_WhenLocalAndNotSelfView`
- `VmActionPrintLogRecoilTests.FormatLogInt_MatchesPrintf`
- `VmActionPrintLogRecoilTests.FormatLogFloat_MatchesPrintf`
- `VmActionPrintLogRecoilTests.MapTranslucentMode_MapsFuzzyMode`
- `VmActionPrintLogRecoilTests.ClampAlpha_BoundsToUnitInterval`
- `VmActionPrintLogRecoilTests.ApplySetRenderStyle_ClampsAlpha`
- `VmActionPrintLogRecoilTests.ClearAlpha1RenderStyleFlag_ClearsBit`
- `VmActionPrintLogRecoilTests.AddFadeInReduce_UsesDefaultReduce`
- `VmActionPrintLogRecoilTests.StepFadeIn_ClampsAndMarksRemove`
- `VmActionFadeSpawnTests.StepFadeOut_ClampsAtZero`
- `VmActionFadeSpawnTests.FadeReduceDefault_UsesTenthWhenZero`
- `VmActionFadeSpawnTests.FinishFadeTo_RemovesAtTarget`
- `VmActionFadeSpawnTests.StepFadeToward_MovesTowardTarget`
- `VmActionFadeSpawnTests.NormalizeDebrisMultipliers_DefaultsNonPositive`
- `VmActionFadeSpawnTests.DebrisTypeIsNull`
- `VmActionFadeSpawnTests.DebrisOffsetFromPr_ScalesAround128`
- `VmActionFadeSpawnTests.ClampParticleStartAlpha_BoundsUnit`
- `VmActionFadeSpawnTests.ShouldSpawnParticle_WhenLifetimeNonZero`
- `VmActionFadeSpawnTests.RotateRelPos_MatchesSpawnParticleBranch`
- `VmActionSightParticleTests.ClampParticleStartAlpha_BoundsUnit`
- `VmActionSightParticleTests.ClampFadeStep_AllowsNegativeOne`
- `VmActionSightParticleTests.RotateParticleExRelPos_MatchesNative`
- `VmActionSightParticleTests.NormalizeParticleStyle_ClampsInvalid`
- `VmActionSightParticleTests.ShouldSpawnParticleEx_WhenLifetimeSet`
- `VmActionSightParticleTests.VerticalSightDelta_UsesTopWhenAbove`
- `VmActionSightParticleTests.SquaredDistance2d_IgnoresZIn2dMode`
- `VmActionSightParticleTests.WithinRange_ComparesSquared`
- `VmActionSightParticleTests.DoCheckSightOrRange_DistanceBeforeSight`
- `VmActionSightParticleTests.SquaredRange_DoublesInput`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 573 --to 573` (0 hard / 0 soft).

## Seams

`Thrust`, `V_GetFont`, `strbin1`, `C_MidPrint`, `Printf`, and `P_RemoveThing` are not hosted. `P_RemoveThing`, `Spawn`, `P_SpawnParticle`, and state/translation setup in debris loop are not hosted. `P_SpawnParticle`, `P_CheckSight`, and player iteration in `CheckIfSeen` / `CheckSightOrRange` are not hosted.

## Credit rationale

Managed helpers mirror only the decision math and gating in each action; I/O and actor mutation stay at seams. Alpha stepping, multiplier defaults, PR offsets, and SPF rotation match native formulas; spawn/remove calls are seams. Managed ports clamp/style/relative-vector setup and the distance branch of sight-or-range; sight checks and spawn calls are seams.
