# Chunk 676 — Whole `GetIntersection`, BSP partition helpers, `SplitPoly`, `FPolyObj::CreateSubsectorLinks`, `PO_LinkToSubsectors`, poly node pool (`po_man.cpp` 1394–1769).

## Scope

Whole `GetIntersection`, BSP partition helpers, `SplitPoly`, `FPolyObj::CreateSubsectorLinks`, `PO_LinkToSubsectors`, poly node pool (`po_man.cpp` 1394–1769).

## Native ranges

- `src/playsim/po_man.cpp` **GetIntersection** 1394–1422 (29 lines)
- `src/playsim/po_man.cpp` **PartitionDistance** 1432–1435 (4 lines)
- `src/playsim/po_man.cpp` **AddToBBox** 1443–1461 (19 lines)
- `src/playsim/po_man.cpp` **AddToBBox** 1469–1489 (21 lines)
- `src/playsim/po_man.cpp` **SplitPoly** 1497–1644 (148 lines)
- `src/playsim/po_man.cpp` **FPolyObj::CreateSubsectorLinks** 1652–1695 (44 lines)
- `src/playsim/po_man.cpp` **PO_LinkToSubsectors** 1703–1712 (10 lines)
- `src/playsim/po_man.cpp` **NewPolyNode** 1720–1740 (21 lines)
- `src/playsim/po_man.cpp` **NewPolyNode** 1748–1753 (6 lines)
- `src/playsim/po_man.cpp` **NewPolyNode** 1761–1769 (9 lines)

**Credited native LOC:** 311 (tally `nativeLoc` 311).

## Managed

`VmPoManBspSplit.cs`
Managed LOC (tally): 325.
Conversion commit: `dd3de7ae57c097a41a23212e594e178aeea10ff8`.

## Padding review (2026-10-21)

| Managed | Native (`po_man.cpp`) |
|---------|-------------------------|
| `GetIntersection` | 1394–1422 |
| `PartitionDistance` | 1432–1435 |
| `AddToBBox` (child/parent) | 1443–1461 |
| `AddToBBox` (vertex) | 1469–1489 |
| `SplitPoly` | 1497–1644 |
| `CreateSubsectorLinks` | 1652–1695 |
| `PoLinkToSubsectors` | 1703–1718 |
| `RentPolyNode` / `FreePolyNode` / `ReleaseAllPolyNodes` | 1720–1769 |

### Out-of-body uncredit (676)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **338** |
| **Out-of-body removed** | **27** |
| **Honest** | **311** |


**Post-review ratio:** 325/311 managed/native ≈ 105% (src file LOC at HEAD; see tally).

## Tests

- `VmPoManBspSplitTests.GetIntersection_FindsPointOnBothSegments`
- `VmPoManBspSplitTests.PartitionDistance_MeasuresPerpendicularDistance`
- `VmPoManBspSplitTests.SplitPoly_SplitsSegAcrossPartition`
- `VmPoManBspSplitTests.SplitPoly_PutsEpsilonSegOnCenterSide`
- `VmPoManBspSplitTests.CreateSubsectorLinks_CompatLinksCenterSubsector`
- `VmPoManBspSplitTests.PoLinkToSubsectors_CreatesWhenMissing`
- `VmPoManBspSplitTests.PolyNodePool_ReusesFreedNodes`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 676 --to 676` (0 hard / 0 soft).

## Seams

`R_PointOnSide`, subsector BSP storage, and native `new`/`delete` remain at seams.

## Credit rationale

Full subsector link construction after spatial clear in chunk 673.
