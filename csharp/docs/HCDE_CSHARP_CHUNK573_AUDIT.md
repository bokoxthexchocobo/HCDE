# Chunk 573 — Re-land: `A_FadeOut`, `A_FadeTo`, `A_SpawnDebris`, and `A_SpawnParticle` setup (`p_actionfunctions.cpp` **1598–1774**). `VmActionPrintLogRecoil` stays on **572** only; translucent/fade-in **1519–1589** deferred.

## Scope

Re-land: `A_FadeOut`, `A_FadeTo`, `A_SpawnDebris`, and `A_SpawnParticle` setup (`p_actionfunctions.cpp` **1598–1774**). `VmActionPrintLogRecoil` stays on **572** only; translucent/fade-in **1519–1589** deferred.

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_FadeOut** 1598–1622 (25 lines)
- `src/playsim/p_actionfunctions.cpp` **A_FadeTo** 1632–1668 (37 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SpawnDebris** 1675–1714 (40 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SpawnParticle** 1722–1774 (53 lines)

**Credited native LOC:** 155 (tally `nativeLoc` 155).

## Managed

`VmActionFadeSpawn.cs`
Managed LOC (tally): 78.
Conversion commit: `6c5f402f83d5e8b085bd5b8c9832524653e54ad0`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `FadeReduceDefault` / `StepFadeOut` | `A_FadeOut` **1598–1622** |
| `StepFadeToward` / `FinishFadeTo` | `A_FadeTo` **1632–1668** |
| `NormalizeDebrisMultipliers` / `DebrisOffsetFromPr` | `A_SpawnDebris` **1675–1714** |
| `ClampParticleStartAlpha` / `ClampPositiveFadeStep` / `AbsParticleSize` / `ShouldSpawnParticle` / `RotateRelPos` | `A_SpawnParticle` **1722–1774** |

### vs chunk **572**

| | Lines |
|--|------:|
| **572** (`VmActionPrintLogRecoil` + rail) | **1267–1512** |
| **573** (`VmActionFadeSpawn` only) | **1598–1774** |
| Deferred (same file as **572**, uncredited) | **1519–1589** translucent/fade-in |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **155** |
| Net behavior-ported | **155** |


**Post-review ratio:** 78/155 managed/native ≈ 50% (src file LOC at HEAD; see tally).

## Tests

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

Credits fade-out/to stepping, debris multiplier/offset gates, and particle clamp/rotate setup; spawn/remove at seams.
