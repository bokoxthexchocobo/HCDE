# Chunk 690 — Whole `P_GeometryLineAttack`, `P_ProjectileHitLinedef`, `P_ProjectileHitPlane`, `P_CheckLinedefVulnerable`, and `P_CheckSectorVulnerable` (`p_destructible.cpp` 303–376, 697–821).

## Scope

Whole `P_GeometryLineAttack`, `P_ProjectileHitLinedef`, `P_ProjectileHitPlane`, `P_CheckLinedefVulnerable`, and `P_CheckSectorVulnerable` (`p_destructible.cpp` 303–376, 697–821).

## Native ranges

- `src/playsim/p_destructible.cpp` **P_GeometryLineAttack** 303–376 (74 lines)
- `src/playsim/p_destructible.cpp` **P_ProjectileHitLinedef** 697–747 (51 lines)
- `src/playsim/p_destructible.cpp` **P_ProjectileHitPlane** 750–784 (35 lines)
- `src/playsim/p_destructible.cpp` **P_CheckLinedefVulnerable** 795–803 (9 lines)
- `src/playsim/p_destructible.cpp` **P_CheckSectorVulnerable** 812–821 (10 lines)

**Credited native LOC:** 179 (tally `nativeLoc` 179).

## Managed

`VmPDestructibleGeometryLineAttack.cs`, `VmPDestructibleProjectileHits.cs`, `VmPDestructibleVulnerability.cs`
Managed LOC (tally): 192.
Conversion commit: `850fb99c3410ce24158b050bc0569a09bdb4e3de`.

## Padding review (2026-10-21)

| Managed | Native (`p_destructible.cpp`) |
|---------|-------------------------------|
| `VmPDestructibleGeometryLineAttack` | `P_GeometryLineAttack` 303–376 |
| `VmPDestructibleProjectileHits` | `P_ProjectileHitLinedef` 697–747; `P_ProjectileHitPlane` 750–784 |
| `VmPDestructibleVulnerability` | `P_CheckLinedefVulnerable` 795–803; `P_CheckSectorVulnerable` 812–821 |


**Post-review ratio:** 192/179 managed/native ≈ 107% (src file LOC at HEAD; see tally).

## Tests

- `VmPDestructibleGeometryCombatTests.GeometryLineAttack_DamagesLineOnWallHit`
- `VmPDestructibleGeometryCombatTests.ProjectileHitLinedef_DamagesLineWhenVulnerable`
- `VmPDestructibleGeometryCombatTests.ProjectileHitPlane_DamagesFloorWhenBlocking`
- `VmPDestructibleGeometryCombatTests.CheckLinedefVulnerable_RejectsHorizon`
- `VmPDestructibleGeometryCombatTests.CheckSectorVulnerable_RejectsSkyFlat`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 690 --to 690` (0 hard / 0 soft).

## Seams

`PGRA_*` helpers and `P_GeometryRadiusAttack` remain native; missile damage amount from `AActor::GetMissileDamage` stubbed via context.

## Credit rationale

Hitscan and projectile destructible geometry after health-group chunk 689.
