# Chunk 710 — Whole `P_LineAttack` class overload (`p_map.cpp` 4699–5050). Trims recognition band; chunk480 helpers at 5052+ unchanged.

## Scope

Whole `P_LineAttack` class overload (`p_map.cpp` 4699–5050). Trims recognition band; chunk480 helpers at 5052+ unchanged.

## Native ranges

- `src/playsim/p_map.cpp` **P_LineAttack** 4699–5050 (352 lines)

**Credited native LOC:** 352 (tally `nativeLoc` 352).

## Managed

`VmPMapLineAttack.cs`
Managed LOC (tally): 268.
Conversion commit: `60910725772503567789470bea815721361d2e3a`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `LineAttack` | `P_LineAttack` 4699–5050 |
| `ShouldSuppressHitscan` / `PreFireBlocked` | 4710–4717 |
| `ThruGhost` / `SpawnSkyPuff` / `UsePuffDamageType` | 4768–4813 |
| `HitActor` / `HitSky` / `AlwaysPuff` / `ShouldDamage` | 4877–5010 |
| `BuildDamageFlags` / `BloodlessImpact` / `SkipDecal` | 4986–5011 |


**Post-review ratio:** 268/352 managed/native ≈ 76% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapLineAttackTests.LineAttack_SuppressesLocalPlayerHitscan`
- `VmPMapLineAttackTests.LineAttack_SpawnsPuffOnMissWhenAlwaysPuff`
- `VmPMapLineAttackTests.LineAttack_NoInteractReturnsPuffDirectly`
- `VmPMapLineAttackTests.UsePuffDamageType_OverridesHitscan`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 710 --to 710` (0 hard / 0 soft).

## Seams

`Trace`, `P_SpawnPuff`, `P_DamageMobj`, `P_GeometryLineAttack`, lag-comp, and event hooks are injected.

## Credit rationale

Hitscan orchestration for puff spawn, geometry hits, and actor damage policy.
