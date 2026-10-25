# Chunk 701 — Whole `P_DrawRailTrail` (`p_effect.cpp` 674–985).

## Scope

Whole `P_DrawRailTrail` (`p_effect.cpp` 674–985).

## Native ranges

- `src/playsim/p_effect.cpp` **P_DrawRailTrail** 674–985 (312 lines)

**Credited native LOC:** 312 (tally `nativeLoc` 312).

## Managed

`VmPEffectDrawRailTrail.cs`
Managed LOC (tally): 368.
Conversion commit: `04c445f698ce1ac35d8d545db902f94ecac27e7a`.

## Padding review (2026-10-21)

| Managed | Native (`p_effect.cpp`) |
|---------|------------------------|
| `VmPEffectDrawRailTrail` | `P_DrawRailTrail` 674–985 |

### Out-of-body uncredit (701)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **312** |
| **Out-of-body removed** | **0** |
| **Honest** | **312** |


**Post-review ratio:** 368/312 managed/native ≈ 118% (src file LOC at HEAD; see tally).

## Tests

- `VmPEffectDrawRailTrailTests.DrawRailTrail_SkipsWhenTooFewPortalHits`
- `VmPEffectDrawRailTrailTests.DrawRailTrail_ZeroLengthReturnsBeforeParticles`
- `VmPEffectDrawRailTrailTests.DrawRailTrail_PlaysSoundAtShooterWhenNearCamera`
- `VmPEffectDrawRailTrailTests.DrawRailTrail_SpawnsParticlesWhenColorsSet`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 701 --to 701` (0 hard / 0 soft).

## Seams

`S_Sound`, `NewParticle`/`JitterParticle`, `Spawn`, and rail cvars (`r_rail_*`) are injected on `DrawRailTrailState`.

## Credit rationale

Rail presentation trail after chunk484 combat rail attack; keeps renderer spawn/sound at seams for headless tests.
