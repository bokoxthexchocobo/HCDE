# Chunk 682 — Whole `FPathTraverse` portal relocation + destructor trim, `P_CheckFov`, `P_BlockmapSearch`, and `RoughBlockCheck` (`p_maputl.cpp` 1612–1643, 1651–1654, 1661–1664, 1682–1772, 1788–1819).

## Scope

Whole `FPathTraverse` portal relocation + destructor trim, `P_CheckFov`, `P_BlockmapSearch`, and `RoughBlockCheck` (`p_maputl.cpp` 1612–1643, 1651–1654, 1661–1664, 1682–1772, 1788–1819).

## Native ranges

- `src/playsim/p_maputl.cpp` **FPathTraverse::PortalRelocate** 1612–1633 (22 lines)
- `src/playsim/p_maputl.cpp` **FPathTraverse::PortalRelocate** 1635–1643 (9 lines)
- `src/playsim/p_maputl.cpp` **FPathTraverse::~FPathTraverse** 1651–1654 (4 lines)
- `src/playsim/p_maputl.cpp` **P_CheckFov** 1661–1664 (4 lines)
- `src/playsim/p_maputl.cpp` **P_BlockmapSearch** 1682–1772 (91 lines)
- `src/playsim/p_maputl.cpp` **RoughBlockCheck** 1788–1819 (32 lines)

**Credited native LOC:** 162 (tally `nativeLoc` 162).

## Managed

`VmPMapUtlPathPortalSearch.cs`
Managed LOC (tally): 167.
Conversion commit: `6f4e913dd1a7e157404fcab7d7a18439a5166f33`.

## Padding review (2026-10-21)

| Managed | Native (`p_maputl.cpp`) |
|---------|-------------------------|
| `PortalRelocate` | `FPathTraverse::PortalRelocate` 1612–1633 |
| `PortalRelocateByDisplacement` | `FPathTraverse::PortalRelocate` 1635–1643 |
| `TrimIntercepts` | `FPathTraverse::~FPathTraverse` 1651–1654 |
| `CheckFov` | `P_CheckFov` 1661–1664 |
| `BlockmapSearch` | `P_BlockmapSearch` 1682–1772 |
| `RoughBlockCheck` | `RoughBlockCheck` 1788–1819 |

### Out-of-body uncredit (682)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **162** |
| **Out-of-body removed** | **0** |
| **Honest** | **162** |


**Post-review ratio:** 167/162 managed/native ≈ 103% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapUtlPathPortalSearchTests.PortalRelocate_ReinitsTraceForLinkedPortal`
- `VmPMapUtlPathPortalSearchTests.CheckFov_AcceptsTargetInsideCone`
- `VmPMapUtlPathPortalSearchTests.BlockmapSearch_FindsCenterBlockFirst`
- `VmPMapUtlPathPortalSearchTests.BlockmapSearch_ScansRingWhenCenterEmpty`
- `VmPMapUtlPathPortalSearchTests.RoughBlockCheck_ReturnsAttackableActorInFov`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 682 --to 682` (0 hard / 0 soft).

## Seams

`P_TranslatePortalXY`, `P_PointOnLineSidePrecise`, `AActor::IsOkayToAttack`, and blockmap storage remain native.

## Credit rationale

Portal trace relocation and monster search rings after actor link chunk 681.
