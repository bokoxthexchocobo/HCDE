# Chunk 886 — Print/log/recoil, translucency, fade, debris normalize, and particle setup from `p_actionfunctions.cpp` (**1381–1845**); console/spawn seams excluded.

## Scope

Print/log/recoil, translucency, fade, debris normalize, and particle setup from `p_actionfunctions.cpp` (**1381–1845**); console/spawn seams excluded.

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **AActor::A_Recoil** 1386–1387 (2 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_Print** 1405–1419 (15 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_PrintBold** 1443–1451 (9 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_Log** 1470–1473 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_LogInt** 1490–1491 (2 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_LogFloat** 1507–1508 (2 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_SetTranslucent** 1525–1529 (5 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_SetRenderStyle** 1544–1545 (2 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_FadeIn** 1570–1583 (14 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_FadeOut** 1604–1615 (12 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_FadeTo** 1639–1662 (24 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_SpawnDebris** 1685–1690 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_SpawnDebris** 1694–1695 (2 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_SpawnParticle** 1743–1761 (19 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_SpawnParticleEx** 1804–1841 (38 lines)

**Credited native LOC:** 156 (tally `nativeLoc` 156).

## Managed

`VmActionPrintLogRecoil.cs`, `VmActionFadeSpawn.cs`, `VmActionSightParticle.cs`
Managed LOC (tally): 200.
Conversion commit: `b4e05906c8f2a9901b5460545efa0be5b08962c2`.

## Padding review (2026-10-21)

### Behavior ported

| Native | Behavior ported |
|--------|-----------------|
| `A_Recoil` | **yes** (angle; `Thrust` seam) |
| `A_Print` / `A_PrintBold` | **yes** (gates; I/O seam) |
| `A_Log` / `A_LogInt` / `A_LogFloat` | **yes** (`Printf` seam) |
| `A_SetTranslucent` / `A_SetRenderStyle` | **yes** |
| `A_FadeIn` / `A_FadeOut` / `A_FadeTo` | **yes** (`P_RemoveThing` seam) |
| `A_SpawnDebris` (prefix) | **yes** (null/mult/offset PR; `Spawn` seam) |
| `A_SpawnParticle` / `A_SpawnParticleEx` (setup) | **yes** (`P_SpawnParticle` seam) |
| `DBot::ThinkForMove` | **no** |

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `RecoilThrustAngleDegrees` | **1386–1387** |
| `ResolveDollarString` / `ShouldPrintToLocalView` / `SelectMidPrintDuration` / `WithTemporaryMidTime` | **1405–1419** |
| `ResolveDollarString` / `SelectMidPrintDuration` | **1443–1451** |
| `ShouldSkipLocalLog` / `ResolveDollarString` | **1470–1473** |
| `ShouldSkipLocalLog` / `FormatLogInt` | **1490–1491** |
| `ShouldSkipLocalLog` / `FormatLogFloat` | **1507–1508** |
| `MapTranslucentMode` / `ClampAlpha` / `ClearAlpha1RenderStyleFlag` | **1525–1529** |
| `ApplySetRenderStyle` | **1544–1545** |
| `FadeReduceDefault` / `ClearAlpha1RenderStyleFlag` / `StepFadeIn` | **1570–1583** |
| `FadeReduceDefault` / `StepFadeOut` | **1604–1615** |
| `StepFadeToward` / `FinishFadeTo` | **1639–1662** |
| `NormalizeDebrisMultipliers` | **1685–1690** |
| `DebrisOffsetFromPr` | **1694–1695** |
| `ClampParticleStartAlpha` / `ClampPositiveFadeStep` / `AbsParticleSize` / `ShouldSpawnParticle` / `RotateRelPos` | **1743–1761** |
| `ClampParticleStartAlpha` / `ClampFadeStep` / `AbsParticleSize` / `ShouldSpawnParticleEx` / `NormalizeParticleStyle` / `RotateParticleExRelPos` | **1804–1841** |

### Padding map

| Native lines | Reason |
|--------------|--------|
| **1420–1423**, **1452–1454** | `C_MidPrint` / `strbin1` |
| **1474**, **1491–1492**, **1508–1509** | `Printf` |
| **1585–1586**, **1618–1619**, **1665–1666** | `P_RemoveThing` |
| **1692–1693**, **1696–1712** | debris `Spawn` / state / velocity loop body |
| **1762–1773**, **1843** | `P_SpawnParticle` |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **156** |
| Net behavior-ported | **156** |


**Post-review ratio:** 200/156 managed/native ≈ 128% (src file LOC at HEAD; see tally).

## Tests

- `VmActionPrintLogRecoilTests.RecoilThrustAngleDegrees_AddsHalfTurn`
- `VmActionPrintLogRecoilTests.ResolveDollarString_LooksUpWhenPrefixed`
- `VmActionPrintLogRecoilTests.ShouldPrintToLocalView_WhenSelfOrTargetLocal`
- `VmActionPrintLogRecoilTests.SelectMidPrintDuration_UsesDefaultWhenNonPositive`
- `VmActionPrintLogRecoilTests.WithTemporaryMidTime_SwapsDuration`
- `VmActionPrintLogRecoilTests.ShouldSkipLocalLog_WhenLocalAndNotSelfView`
- `VmActionPrintLogRecoilTests.FormatLogInt_MatchesPrintf`
- `VmActionPrintLogRecoilTests.FormatLogFloat_MatchesPrintf`
- `VmActionPrintLogRecoilTests.MapTranslucentMode_MapsFuzzyMode`
- `VmActionPrintLogRecoilTests.ClampAlpha_BoundsToUnitInterval`
- `VmActionPrintLogRecoilTests.ApplySetRenderStyle_ClampsAlpha`
- `VmActionPrintLogRecoilTests.FadeReduceDefault_UsesTenthWhenZero`
- `VmActionPrintLogRecoilTests.StepFadeIn_ClampsAndMarksRemove`
- `VmActionFadeSpawnTests.StepFadeOut_ClampsAtZero`
- `VmActionFadeSpawnTests.StepFadeToward_MovesTowardTarget`
- `VmActionFadeSpawnTests.FinishFadeTo_RemovesAtTarget`
- `VmActionFadeSpawnTests.NormalizeDebrisMultipliers_DefaultsNonPositive`
- `VmActionFadeSpawnTests.DebrisOffsetFromPr_ScalesAround128`
- `VmActionFadeSpawnTests.ClampParticleStartAlpha_BoundsUnit`
- `VmActionFadeSpawnTests.ShouldSpawnParticle_WhenLifetimeNonZero`
- `VmActionFadeSpawnTests.RotateRelPos_MatchesSpawnParticleBranch`
- `VmActionSightParticleTests.ClampFadeStep_AllowsNegativeOne`
- `VmActionSightParticleTests.NormalizeParticleStyle_ClampsInvalid`
- `VmActionSightParticleTests.ShouldSpawnParticleEx_WhenLifetimeSet`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 886 --to 886` (0 hard / 0 soft).

## Seams

`Thrust`, `V_GetFont`, `strbin1`, `C_MidPrint`, `Printf`, `Spawn`, `P_SpawnParticle`, and `P_RemoveThing` are not hosted. Bot `DBot::ThinkForMove` (**b_think.cpp** 90–322) **not** attempted — callee fidelity (incl. `P_CheckPosition` for `SafeCheckPosition`).

## Credit rationale

**Bot area:** no `ThinkForMove` land and **no** new bot sub-150 retire rows while `SafeCheckPosition` ≠ `P_CheckPosition` and roam/inventory paths lack faithful ports. **886** moves native credit for print/fade/particle setup off **576** manifest-only listing into an honest **≥150** `p_actionfunctions` band.
