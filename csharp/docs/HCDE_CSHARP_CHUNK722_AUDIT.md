# Chunk 722 — Whole `GetRadiusDamage`, `GetOldRadiusDamage`, `P_GetRadiusDamage`, and `P_RadiusAttack` (`p_map.cpp` 6130–6480).

## Scope

Whole `GetRadiusDamage`, `GetOldRadiusDamage`, `P_GetRadiusDamage`, and `P_RadiusAttack` (`p_map.cpp` 6130–6480).

## Native ranges

- `src/playsim/p_map.cpp` **GetRadiusDamage** 6130–6202 (73 lines)
- `src/playsim/p_map.cpp` **GetOldRadiusDamage** 6215–6249 (35 lines)
- `src/playsim/p_map.cpp` **P_GetRadiusDamage** 6259–6282 (24 lines)
- `src/playsim/p_map.cpp` **P_RadiusAttack** 6291–6480 (190 lines)

**Credited native LOC:** 322 (tally `nativeLoc` 322).

## Managed

`VmPMapRadiusDamage.cs`
Managed LOC (tally): 197.
Conversion commit: `447135a26d6be1847847cf6e6b6ac2a311081a92`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `GetRadiusDamage` | `GetRadiusDamage` 6130–6202 |
| `GetOldRadiusDamage` | `GetOldRadiusDamage` 6215–6249 |
| `GetRadiusDamageAction` | `P_GetRadiusDamage` 6259–6282 |
| `RadiusAttack` | `P_RadiusAttack` 6291–6480 |


**Post-review ratio:** 197/322 managed/native ≈ 61% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapRadiusDamageTests.GetRadiusDamageAction_ReturnsFullDamageOnSelf`
- `VmPMapRadiusDamageTests.GetRadiusDamage_ModernSquareDistanceUsesZWhenOutsideBand`
- `VmPMapRadiusDamageTests.GetOldRadiusDamage_ReturnsNegativeWhenOutOfRange`
- `VmPMapRadiusDamageTests.RadiusAttack_SkipsNonShootableWithoutVulnerable`
- `VmPMapRadiusDamageTests.RadiusAttack_DamagesWhenInSight`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 722 --to 722` (0 hard / 0 soft).

## Seams

`FMultiBlockThingsIterator`, `P_GeometryRadiusAttack`, `P_DamageMobj`, `P_TraceBleed`, net mirror filters, and `P_CheckSight` are injected.

## Credit rationale

Flat and old radius falloff plus target collection/damage loop (excludes chunk455 `PIT_CheckThing`).
