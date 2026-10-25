# Chunk 667 — Whole `P_InterceptVector` and `P_LineOpening` (`p_maputl.cpp` 78–239).

## Scope

Whole `P_InterceptVector` and `P_LineOpening` (`p_maputl.cpp` 78–239).

## Native ranges

- `src/playsim/p_maputl.cpp` **P_InterceptVector** 78–99 (22 lines)
- `src/playsim/p_maputl.cpp` **P_LineOpening** 110–239 (130 lines)

**Credited native LOC:** 152 (tally `nativeLoc` 152).

## Managed

`VmPMapUtlLineOpening.cs`
Managed LOC (tally): 144.
Conversion commit: `f70c39f226eddf9ef262054359996fd64e3b4835`.

## Padding review (2026-10-21)

| Managed | Native (`p_maputl.cpp`) |
|---------|-------------------------|
| `InterceptVector` | `P_InterceptVector` 78–99 |
| `LineOpening` | `P_LineOpening` 110–239 |

### Out-of-body uncredit (667)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **152** |
| **Out-of-body removed** | **0** |
| **Honest** | **152** |


**Post-review ratio:** 144/152 managed/native ≈ 95% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapUtlLineOpeningTests.InterceptVector_ParallelReturnsZero`
- `VmPMapUtlLineOpeningTests.LineOpening_OneSidedClearsRange`
- `VmPMapUtlLineOpeningTests.LineOpening_TwoSidedUsesLowerFrontFloor`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 667 --to 667` (0 hard / 0 soft).

## Seams

`P_LineOpening_XFloors`, `P_LineOpening_3dMidtex`, and `NextLowestFloorAt` remain native.

## Credit rationale

Core line opening setup for map utilities TU.
