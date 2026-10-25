# Chunk 722 — Whole `GetRadiusDamage`, `GetOldRadiusDamage`, `P_GetRadiusDamage`, and `P_RadiusAttack` (`p_map.cpp` 6130–6480). Retired (2026-10-23): out-of-body credit removed; honest **132** < **150**.

## Scope

Whole `GetRadiusDamage`, `GetOldRadiusDamage`, `P_GetRadiusDamage`, and `P_RadiusAttack` (`p_map.cpp` 6130–6480). Retired (2026-10-23): out-of-body credit removed; honest **132** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `743e0b98e7914998c6580d91b0c35fa471a01b30`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `GetRadiusDamage` | `GetRadiusDamage` 6130–6202 |
| `GetOldRadiusDamage` | `GetOldRadiusDamage` 6215–6249 |
| `GetRadiusDamageAction` | `P_GetRadiusDamage` 6259–6282 |
| `RadiusAttack` | `P_RadiusAttack` 6291–6480 |

### Out-of-body uncredit (722)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **322** |
| **Out-of-body removed** | **190** |
| **Honest** | **132** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 722 --to 722` (0 hard / 0 soft).

## Seams

`FMultiBlockThingsIterator`, `P_GeometryRadiusAttack`, `P_DamageMobj`, `P_TraceBleed`, net mirror filters, and `P_CheckSight` are injected.

## Credit rationale

Flat and old radius falloff plus target collection/damage loop (excludes chunk455 `PIT_CheckThing`).
