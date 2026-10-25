# Chunk 726 — Whole `P_ExplodeMissile` (1958–2096) and `AActor::FloorBounceMissile` (2139–2274) in `p_mobj.cpp`. Retired (2026-10-24): `--band` hull overlaps existing credit (p-mobj-bounce-seek-matrix-water-chunk753 src/playsim/p_mobj.cpp 2098-2105).

## Scope

Whole `P_ExplodeMissile` (1958–2096) and `AActor::FloorBounceMissile` (2139–2274) in `p_mobj.cpp`. Retired (2026-10-24): `--band` hull overlaps existing credit (p-mobj-bounce-seek-matrix-water-chunk753 src/playsim/p_mobj.cpp 2098-2105).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `2a217dca86bf41206b9e172bcaa8750d750ce56d`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `ExplodeMissile` | `P_ExplodeMissile` 1958–2096 |
| `FloorBounceMissile` | `AActor::FloorBounceMissile` 2139–2274 |
| `ExplodeCountGated` | exp countdown 1966–1971 |
| `ApplyDehExplosionStyle` | DEH explosion 2064–2083 |

### Out-of-body uncredit (726)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **275** |
| **Out-of-body removed** | **0** |
| **Honest** | **275** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 726 --to 726` (0 hard / 0 soft).

## Seams

`P_ProjectileHitLinedef`, `FindState`, `SetState`, `S_Sound`, `DImpactDecal`, `SpecialBounceHit`, `P_HitFloor`, `CallDie`, and `PlayBounceSound` are injected.

## Credit rationale

Missile death transition and floor/ceiling bounce reflection.
