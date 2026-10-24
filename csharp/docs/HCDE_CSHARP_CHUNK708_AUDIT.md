# Chunk 708 — Whole `P_ThinkParticles`, `P_SpawnParticle`, `P_RunEffect` (`p_effect.cpp` 278–567).

## Scope

Whole `P_ThinkParticles`, `P_SpawnParticle`, `P_RunEffect` (`p_effect.cpp` 278–567).

## Native ranges

- `src/playsim/p_effect.cpp` **P_ThinkParticles** 278–351 (74 lines)
- `src/playsim/p_effect.cpp` **P_SpawnParticle** 353–394 (42 lines)
- `src/playsim/p_effect.cpp` **P_RunEffect** 457–567 (111 lines)

**Credited native LOC:** 227 (tally `nativeLoc` 227).

## Managed

`VmPEffectParticleRun.cs`
Managed LOC (tally): 230.
Conversion commit: `25084d77eedd7c5f1bbbc3f45baf658b5f422a69`.

## Padding review (2026-10-21)

| Managed | Native (`p_effect.cpp`) |
|---------|------------------------|
| `ThinkParticles` | `P_ThinkParticles` 278–351 |
| `SpawnParticle` | `P_SpawnParticle` 353–394 |
| `RunEffect` | `P_RunEffect` 457–567 |


**Post-review ratio:** 230/227 managed/native ≈ 101% (src file LOC at HEAD; see tally).

## Tests

- `VmPEffectParticleRunTests.ThinkParticles_ExpiresWhenTtlZero`
- `VmPEffectParticleRunTests.ThinkParticles_SkipsFadeWhenFrozen`
- `VmPEffectParticleRunTests.SpawnParticle_SetsFadeFromTtlWhenNegativeStep`
- `VmPEffectParticleRunTests.RunEffect_SpawnsRocketTrailParticles`
- `VmPEffectParticleRunTests.RunEffect_SpawnsGrenadeTrailParticles`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 708 --to 708` (0 hard / 0 soft).

## Seams

`NewParticle`, `JitterParticle`, `P_DrawSplash2`, and trail cvars are injected.

## Credit rationale

Particle think/spawn and actor effect trails.
