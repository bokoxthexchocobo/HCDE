# Chunk 710 — Whole `P_LineAttack` class overload (`p_map.cpp` 4699–5050). Trims recognition band; chunk480 helpers at 5052+ unchanged. Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

Whole `P_LineAttack` class overload (`p_map.cpp` 4699–5050). Trims recognition band; chunk480 helpers at 5052+ unchanged. Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `f6ef2d69fbb66db3994569388886f0219f7e1e18`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `LineAttack` | `P_LineAttack` 4699–5050 |
| `ShouldSuppressHitscan` / `PreFireBlocked` | 4710–4717 |
| `ThruGhost` / `SpawnSkyPuff` / `UsePuffDamageType` | 4768–4813 |
| `HitActor` / `HitSky` / `AlwaysPuff` / `ShouldDamage` | 4877–5010 |
| `BuildDamageFlags` / `BloodlessImpact` / `SkipDecal` | 4986–5011 |

### Out-of-body uncredit (710)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **352** |
| **Out-of-body removed** | **352** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 710 --to 710` (0 hard / 0 soft).

## Seams

`Trace`, `P_SpawnPuff`, `P_DamageMobj`, `P_GeometryLineAttack`, lag-comp, and event hooks are injected.

## Credit rationale

Hitscan orchestration for puff spawn, geometry hits, and actor damage policy.
