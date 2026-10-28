# Chunk 708 — Whole `P_ThinkParticles`, `P_SpawnParticle`, `P_RunEffect` (`p_effect.cpp` 278–567). Retired (2026-10-24): `--band` hull overlaps existing credit (p-effect-particle-pool-splash-chunk775 src/playsim/p_effect.cpp 401-425).

## Scope

Whole `P_ThinkParticles`, `P_SpawnParticle`, `P_RunEffect` (`p_effect.cpp` 278–567). Retired (2026-10-24): `--band` hull overlaps existing credit (p-effect-particle-pool-splash-chunk775 src/playsim/p_effect.cpp 401-425).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `1ebe25a24865d683060dc5335e130ad082e7f277`.

## Padding review (2026-10-21)

| Managed | Native (`p_effect.cpp`) |
|---------|------------------------|
| `ThinkParticles` | `P_ThinkParticles` 278–351 |
| `SpawnParticle` | `P_SpawnParticle` 353–394 |
| `RunEffect` | `P_RunEffect` 457–567 |

### Out-of-body uncredit (708)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **227** |
| **Out-of-body removed** | **42** |
| **Honest** | **185** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 708 --to 708` (0 hard / 0 soft).

## Seams

`NewParticle`, `JitterParticle`, `P_DrawSplash2`, and trail cvars are injected.

## Credit rationale

Particle think/spawn and actor effect trails.
