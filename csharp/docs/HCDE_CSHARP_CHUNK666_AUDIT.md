# Chunk 666 — Whole `FPathTraverse::init` (`p_maputl.cpp` 1397–1604).

## Scope

Whole `FPathTraverse::init` (`p_maputl.cpp` 1397–1604).

## Native ranges

- `src/playsim/p_maputl.cpp` **FPathTraverse::init** 1397–1604 (208 lines)

**Credited native LOC:** 208 (tally `nativeLoc` 208).

## Managed

`VmPMapUtlPathTraverseInit.cs`
Managed LOC (tally): 207.
Conversion commit: `0482ded1a8ed6ae788bd139787f5620b0e78bf85`.

## Padding review (2026-10-21)

| Managed | Native (`p_maputl.cpp`) |
|---------|-------------------------|
| `PathTraverseInit` | `FPathTraverse::init` 1397–1604 |

### Out-of-body uncredit (666)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **208** |
| **Out-of-body removed** | **0** |
| **Honest** | **208** |


**Post-review ratio:** 207/208 managed/native ≈ 100% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapUtlPathTraverseInitTests.PathTraverseInit_WalksBlocksAlongTrace`
- `VmPMapUtlPathTraverseInitTests.PathTraverseInit_CompatAbortsOnDiagonalCorner`
- `VmPMapUtlPathTraverseInitTests.PathTraverseInit_CornerCaseAddsNeighborBlocks`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 666 --to 666` (0 hard / 0 soft).

## Seams

`AddLineIntercepts`, `AddThingIntercepts`, and `FBlockThingsIterator` remain native.

## Credit rationale

First credited `p_maputl.cpp` blockmap trace setup.
