# Chunk 690 — Whole `P_GeometryLineAttack`, `P_ProjectileHitLinedef`, `P_ProjectileHitPlane`, `P_CheckLinedefVulnerable`, and `P_CheckSectorVulnerable` (`p_destructible.cpp` 303–376, 697–821). Retired (2026-10-24): `--band` hull overlaps existing credit (p-destructible-geometry-radius-chunk661 src/playsim/p_destructible.cpp 442-688).

## Scope

Whole `P_GeometryLineAttack`, `P_ProjectileHitLinedef`, `P_ProjectileHitPlane`, `P_CheckLinedefVulnerable`, and `P_CheckSectorVulnerable` (`p_destructible.cpp` 303–376, 697–821). Retired (2026-10-24): `--band` hull overlaps existing credit (p-destructible-geometry-radius-chunk661 src/playsim/p_destructible.cpp 442-688).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `8e1717275d73b9fc7db7dac0b531b69ff1c6aff4`.

## Padding review (2026-10-21)

| Managed | Native (`p_destructible.cpp`) |
|---------|-------------------------------|
| `VmPDestructibleGeometryLineAttack` | `P_GeometryLineAttack` 303–376 |
| `VmPDestructibleProjectileHits` | `P_ProjectileHitLinedef` 697–747; `P_ProjectileHitPlane` 750–784 |
| `VmPDestructibleVulnerability` | `P_CheckLinedefVulnerable` 795–803; `P_CheckSectorVulnerable` 812–821 |

### Out-of-body uncredit (690)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **179** |
| **Out-of-body removed** | **0** |
| **Honest** | **179** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 690 --to 690` (0 hard / 0 soft).

## Seams

`PGRA_*` helpers and `P_GeometryRadiusAttack` remain native; missile damage amount from `AActor::GetMissileDamage` stubbed via context.

## Credit rationale

Hitscan and projectile destructible geometry after health-group chunk 689.
