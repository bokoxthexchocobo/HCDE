# Chunk 574 — `A_SpawnParticleEx`, `DoCheckSightOrRange`, `CheckSightOrRange`, and `CheckRange` (`p_actionfunctions.cpp` **1777–1981**). `CheckIfSeen` (**1854–1878**) excluded (sight seam).

## Scope

`A_SpawnParticleEx`, `DoCheckSightOrRange`, `CheckSightOrRange`, and `CheckRange` (`p_actionfunctions.cpp` **1777–1981**). `CheckIfSeen` (**1854–1878**) excluded (sight seam).

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_SpawnParticleEx** 1777–1846 (70 lines)
- `src/playsim/p_actionfunctions.cpp` **DoCheckSightOrRange** 1887–1922 (36 lines)
- `src/playsim/p_actionfunctions.cpp` **CheckSightOrRange** 1924–1951 (28 lines)
- `src/playsim/p_actionfunctions.cpp` **CheckRange** 1954–1981 (28 lines)

**Credited native LOC:** 162 (tally `nativeLoc` 162).

## Managed

`VmActionSightParticle.cs`
Managed LOC (tally): 82.
Conversion commit: `9bc0de4c8f439dade920e341e7315c0dc0163e6e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `ClampParticleStartAlpha` / `ClampFadeStep` / `AbsParticleSize` / `NormalizeParticleStyle` / `RotateParticleExRelPos` / `ShouldSpawnParticleEx` | `A_SpawnParticleEx` **1777–1846** (`P_SpawnParticle` **1843** seam) |
| `VerticalSightDelta` / `SquaredDistance2d` / `WithinRange` / `DoCheckSightOrRange` | `DoCheckSightOrRange` **1887–1922** |
| `SquaredRange` / `PlayerSightOrRangeHit` | `CheckSightOrRange` **1924–1951** (loop gating; `P_CheckSight` seam) |
| `SquaredRange` / `RangeOnlyHit` / `PlayerSightOrRangeHit` | `CheckRange` **1954–1981** |

### vs chunk **573**

| | Lines |
|--|------:|
| **573** (`VmActionFadeSpawn`) | **1598–1774** |
| **574** (this row) | **1777–1981** |
| Gap | **1775–1776** |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **162** |
| Net behavior-ported | **162** |


**Post-review ratio:** 82/162 managed/native ≈ 51% (src file LOC at HEAD; see tally).

## Tests

- `VmActionSightParticleTests.ClampParticleStartAlpha_BoundsUnit`
- `VmActionSightParticleTests.NormalizeParticleStyle_ClampsInvalid`
- `VmActionSightParticleTests.RotateParticleExRelPos_MatchesNative`
- `VmActionSightParticleTests.DoCheckSightOrRange_DistanceBeforeSight`
- `VmActionSightParticleTests.VerticalSightDelta_UsesTopWhenAbove`
- `VmActionSightParticleTests.SquaredRange_DoublesInput`
- `VmActionSightParticleTests.WithinRange_ComparesSquared`
- `VmActionSightParticleTests.SquaredDistance2d_IgnoresZIn2dMode`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 574 --to 574` (0 hard / 0 soft).

## Seams

`P_SpawnParticle`, `P_CheckSight`, and player-table iteration bodies are not hosted.

## Credit rationale

ParticleEx clamp/style/rotate setup plus shared sight-or-range distance and per-player gating helpers.
