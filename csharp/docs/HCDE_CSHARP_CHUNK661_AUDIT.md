# Chunk 661 — Whole `P_GeometryRadiusAttack` (`p_destructible.cpp` 442–688).

## Scope

Whole `P_GeometryRadiusAttack` (`p_destructible.cpp` 442–688).

## Native ranges

- `src/playsim/p_destructible.cpp` **P_GeometryRadiusAttack** 442–688 (247 lines)

**Credited native LOC:** 247 (tally `nativeLoc` 247).

## Managed

`VmPDestructibleGeometryRadiusAttack.cs`
Managed LOC (tally): 186.
Conversion commit: `4fdffe0c4d68cab662ff73bff8fb2ff70531ea06`.

## Padding review (2026-10-21)

| Managed | Native (`p_destructible.cpp`) |
|---------|-------------------------------|
| `GeometryRadiusAttack` / `GeometryRadiusAttackContext` | `P_GeometryRadiusAttack` 442–688 |

### Out-of-body uncredit (661)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **247** |
| **Out-of-body removed** | **0** |
| **Honest** | **247** |


**Post-review ratio:** 186/247 managed/native ≈ 75% (src file LOC at HEAD; see tally).

## Tests

- `VmPDestructibleGeometryRadiusAttackTests.GeometryRadiusAttack_InsertsCeilingGroup`
- `VmPDestructibleGeometryRadiusAttackTests.GeometryRadiusAttack_SkipsNullSpot`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 661 --to 661` (0 hard / 0 soft).

## Seams

`PGRA_InsertIfCloser`, `FBlockLinesIterator`, and `P_TraceLine` remain native.

## Credit rationale

Fresh playsim TU; sector/ffloor/line damage grouping hosted.
