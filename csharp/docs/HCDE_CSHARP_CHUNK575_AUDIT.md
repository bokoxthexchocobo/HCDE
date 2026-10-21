# Chunk 575 — A_SpawnParticleEx, CheckIfSeen, DoCheckSightOrRange, CheckSightOrRange prefix (1777–1970)

## Scope

A_SpawnParticleEx, CheckIfSeen, DoCheckSightOrRange, CheckSightOrRange prefix (1777–1970)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_SpawnParticleEx** 1777–1846 (70 lines)
- `src/playsim/p_actionfunctions.cpp` **CheckIfSeen** 1855–1878 (24 lines)
- `src/playsim/p_actionfunctions.cpp` **DoCheckSightOrRange** 1887–1922 (36 lines)
- `src/playsim/p_actionfunctions.cpp` **CheckSightOrRange** 1924–1951 (28 lines)

**Credited native LOC:** 158 (tally `nativeLoc` 158).

## Managed

`VmActionSightParticle.cs`
Managed LOC (tally): 89.
Conversion commit: `44affc81b1f6b93b0afc180bcea590034abc88b9`.

## Padding review (2026-10-21)

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `NormalizeParticleStyle` / `ClampFadeStep` / `ShouldSpawnParticleEx` | `A_SpawnParticleEx` 1804–1843 |
| `CheckIfSeenShouldJump` | `CheckIfSeen` 1859–1877 |
| `VerticalSightDelta` / `SquaredDistance2d` / `WithinRange` | `DoCheckSightOrRange` 1896–1921 |
| `SquaredRange` | `CheckSightOrRange` 1930 |

**Post-review ratio:** 89/158 managed/native ≈ 56% (src file LOC at HEAD; see tally).

## Tests

- `VmActionSightParticleTests.NormalizeParticleStyle_ClampsInvalid`
- `VmActionSightParticleTests.ClampFadeStep_AllowsNegativeOne`
- `VmActionSightParticleTests.ShouldSpawnParticleEx_WhenLifetimeSet`
- `VmActionSightParticleTests.CheckIfSeenShouldJump_WhenNobodySees`
- `VmActionSightParticleTests.VerticalSightDelta_UsesTopWhenAbove`
- `VmActionSightParticleTests.SquaredDistance2d_IgnoresZIn2dMode`
- `VmActionSightParticleTests.WithinRange_ComparesSquared`
- `VmActionSightParticleTests.SquaredRange_DoublesInput`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 575 --to 575` (0 hard / 0 soft).

## Seams

`P_SpawnParticle`, `P_CheckSight`, and player iteration are not hosted.

## Credit rationale

Faithful managed mirrors for particle-ex style clamping and sight/range distance planning.
