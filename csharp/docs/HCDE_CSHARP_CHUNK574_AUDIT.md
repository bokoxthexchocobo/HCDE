# Chunk 574 — A_FadeOut, A_FadeTo, A_SpawnDebris, A_SpawnParticle (1598–1775)

## Scope

A_FadeOut, A_FadeTo, A_SpawnDebris, A_SpawnParticle (1598–1775)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_FadeOut** 1598–1622 (25 lines)
- `src/playsim/p_actionfunctions.cpp` **A_FadeTo** 1632–1668 (37 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SpawnDebris** 1675–1714 (40 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SpawnParticle** 1722–1775 (54 lines)

**Credited native LOC:** 156 (tally `nativeLoc` 156).

## Managed

`VmActionFadeSpawn.cs`
Managed LOC (tally): 78.
Conversion commit: `fed611ed7271515762ef55c04bc83f1846e47235`.

## Padding review (2026-10-21)

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `StepFadeOut` / `FadeReduceDefault` | `A_FadeOut` 1604–1620 |
| `StepFadeToward` / `FinishFadeTo` | `A_FadeTo` 1641–1666 |
| `NormalizeDebrisMultipliers` / `DebrisOffsetFromPr` | `A_SpawnDebris` 1689–1710 |
| `ClampParticleStartAlpha` / `RotateRelPos` / `ShouldSpawnParticle` | `A_SpawnParticle` 1743–1772 |

**Post-review ratio:** 78/156 managed/native ≈ 50% (src file LOC at HEAD; see tally).

## Tests

- `VmActionFadeSpawnTests.StepFadeOut_ClampsAtZero`
- `VmActionFadeSpawnTests.FadeReduceDefault_UsesTenthWhenZero`
- `VmActionFadeSpawnTests.FinishFadeTo_RemovesAtTarget`
- `VmActionFadeSpawnTests.StepFadeToward_MovesTowardTarget`
- `VmActionFadeSpawnTests.NormalizeDebrisMultipliers_DefaultsNonPositive`
- `VmActionFadeSpawnTests.DebrisOffsetFromPr_ScalesAround128`
- `VmActionFadeSpawnTests.ClampParticleStartAlpha_BoundsUnit`
- `VmActionFadeSpawnTests.RotateRelPos_MatchesSpawnParticleBranch`
- `VmActionFadeSpawnTests.ShouldSpawnParticle_WhenLifetimeNonZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 574 --to 574` (0 hard / 0 soft).

## Seams

`P_RemoveThing`, `Spawn`, and `P_SpawnParticle` are not hosted.

## Credit rationale

Faithful managed mirrors for fade steps, debris multipliers, and particle clamp/rotation setup.
