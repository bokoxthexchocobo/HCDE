# Chunk 573 — Re-land: translucent/render/fade, debris, and particle setup (`p_actionfunctions.cpp` **1519–1774**). Print/log on **572** (**1267–1512**); sight/spawn bands stay off-tally.

## Scope

Re-land: translucent/render/fade, debris, and particle setup (`p_actionfunctions.cpp` **1519–1774**). Print/log on **572** (**1267–1512**); sight/spawn bands stay off-tally.

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_SetTranslucent** 1519–1531 (13 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetRenderStyle** 1538–1547 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **A_FadeIn** 1563–1589 (27 lines)
- `src/playsim/p_actionfunctions.cpp` **A_FadeOut** 1598–1622 (25 lines)
- `src/playsim/p_actionfunctions.cpp` **A_FadeTo** 1632–1668 (37 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SpawnDebris** 1675–1714 (40 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SpawnParticle** 1722–1774 (53 lines)

**Credited native LOC:** 205 (tally `nativeLoc` 205).

## Managed

`VmActionPrintLogRecoil.cs`, `VmActionFadeSpawn.cs`
Managed LOC (tally): 141.
Conversion commit: `b5cae9800c5a03167d40da5fb7dfb0d3d307aac9`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `MapTranslucentMode` / `ShouldClearAlpha1Flag` / `ClampAlpha` | `A_SetTranslucent` **1519–1531** |
| `ApplySetRenderStyle` | `A_SetRenderStyle` **1538–1547** |
| `FadeReduceDefault` / `ClearAlpha1RenderStyleFlag` / `AddFadeInReduce` / `StepFadeIn` | `A_FadeIn` **1563–1589** |
| `FadeReduceDefault` / `StepFadeOut` | `A_FadeOut` **1598–1622** |
| `StepFadeToward` / `FinishFadeTo` | `A_FadeTo` **1632–1668** |
| `NormalizeDebrisMultipliers` / `DebrisOffsetFromPr` | `A_SpawnDebris` **1675–1714** |
| `ClampParticleStartAlpha` / `ClampPositiveFadeStep` / `AbsParticleSize` / `ShouldSpawnParticle` / `RotateRelPos` | `A_SpawnParticle` **1722–1774** (`P_SpawnParticle` seam) |

### vs chunk **572**

| | Lines |
|--|------:|
| **572** (rail/print/log) | **1267–1512** |
| **573** (this row) | **1519–1774** |
| Gap (comments/blanks) | **1513–1518**, inter-function separators |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **205** |
| Net behavior-ported | **205** |


**Post-review ratio:** 141/205 managed/native ≈ 69% (src file LOC at HEAD; see tally).

## Tests

- `VmActionPrintLogRecoilTests.MapTranslucentMode_MapsFuzzyMode`
- `VmActionPrintLogRecoilTests.ClampAlpha_BoundsToUnitInterval`
- `VmActionPrintLogRecoilTests.ApplySetRenderStyle_ClampsAlpha`
- `VmActionPrintLogRecoilTests.FadeReduceDefault_UsesTenthWhenZero`
- `VmActionPrintLogRecoilTests.StepFadeIn_ClampsAndMarksRemove`
- `VmActionFadeSpawnTests.StepFadeOut_ClampsAtZero`
- `VmActionFadeSpawnTests.FadeReduceDefault_UsesTenthWhenZero`
- `VmActionFadeSpawnTests.StepFadeToward_MovesTowardTarget`
- `VmActionFadeSpawnTests.FinishFadeTo_RemovesAtTarget`
- `VmActionFadeSpawnTests.NormalizeDebrisMultipliers_DefaultsNonPositive`
- `VmActionFadeSpawnTests.DebrisOffsetFromPr_ScalesAround128`
- `VmActionFadeSpawnTests.ClampParticleStartAlpha_BoundsUnit`
- `VmActionFadeSpawnTests.RotateRelPos_MatchesSpawnParticleBranch`
- `VmActionFadeSpawnTests.ShouldSpawnParticle_WhenLifetimeNonZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 573 --to 573` (0 hard / 0 soft).

## Seams

`P_RemoveThing`, `Spawn`, `SetState`, `P_SpawnParticle`, and `P_CheckSight` are not hosted.

## Credit rationale

Credits style/alpha policy, fade stepping, debris multiplier/offset gates, and particle clamp/rotate setup; engine spawn/remove at seams.
