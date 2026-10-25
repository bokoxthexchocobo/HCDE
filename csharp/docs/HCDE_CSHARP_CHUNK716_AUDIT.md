# Chunk 716 — Whole `P_UseTraverse`, `P_NoWayTraverse`, `P_UseLines`, and `P_UsePuzzleItem` (`p_map.cpp` 5829–6100). Function boundaries only; gap 5958–6002 is comment block between `P_NoWayTraverse` and `P_UseLines`.

## Scope

Whole `P_UseTraverse`, `P_NoWayTraverse`, `P_UseLines`, and `P_UsePuzzleItem` (`p_map.cpp` 5829–6100). Function boundaries only; gap 5958–6002 is comment block between `P_NoWayTraverse` and `P_UseLines`.

## Native ranges

- `src/playsim/p_map.cpp` **P_UseTraverse** 5829–5957 (129 lines)
- `src/playsim/p_map.cpp` **P_NoWayTraverse** 5972–5993 (22 lines)
- `src/playsim/p_map.cpp` **P_UseLines** 6003–6027 (25 lines)
- `src/playsim/p_map.cpp` **P_UsePuzzleItem** 6037–6100 (64 lines)

**Credited native LOC:** 240 (tally `nativeLoc` 240).

## Managed

`VmPMapUseTraverse.cs`
Managed LOC (tally): 282.
Conversion commit: `761a49d8ac72abd569995ed14f6bec4e6f86bf3c`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `UseTraverse` / `ThingUseSpecial` / `BlockedUse` | `P_UseTraverse` 5829–5957 |
| `NoWayTraverse` / `IsPortalLine` | `P_NoWayTraverse` 5972–5993 |
| `UseLines` / `UseButtonPressed` | `P_UseLines` 6003–6027 |
| `UsePuzzleItem` | `P_UsePuzzleItem` 6037–6100 |

### Out-of-body uncredit (716)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **240** |
| **Out-of-body removed** | **0** |
| **Honest** | **240** |


**Post-review ratio:** 282/240 managed/native ≈ 118% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapUseTraverseTests.UseTraverse_ActivatesUseLine`
- `VmPMapUseTraverseTests.UseTraverse_SetsFoundLineWhenBlocked`
- `VmPMapUseTraverseTests.NoWayTraverse_BlocksWhenRangeZero`
- `VmPMapUseTraverseTests.UseLines_PlaysFailWhenBlocked`
- `VmPMapUseTraverseTests.UsePuzzleItem_MatchesLineArgs`
- `VmPMapUseTraverseTests.UsePuzzleItem_RejectsWrongType`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 716 --to 716` (0 hard / 0 soft).

## Seams

`FPathTraverse`, `P_LineOpening`, `P_ActivateLine`, VM `Used`, `P_StartScript`, and sector actions remain native.

## Credit rationale

Use-line traversal, blocked-use oof, player use driver, and puzzle-item trace; absorbs retired chunk718 p_map bands.
