# Chunk 677 — Polyobject blockmap link, mobj blocking, floor/ceil refresh, spatial helpers (`po_man.cpp` **1025–1384**).

## Scope

Polyobject blockmap link, mobj blocking, floor/ceil refresh, spatial helpers (`po_man.cpp` **1025–1384**).

## Native ranges

- `src/playsim/po_man.cpp` **FPolyObj::CheckMobjBlocking** 1025–1146 (122 lines)
- `src/playsim/po_man.cpp` **FPolyObj::LinkPolyobj** 1154–1217 (64 lines)
- `src/playsim/po_man.cpp` **FPolyObj::RecalcActorFloorCeil** 1228–1248 (21 lines)
- `src/playsim/po_man.cpp` **FPolyObj::ClosestPoint** 1259–1317 (59 lines)
- `src/playsim/po_man.cpp` **PO_Busy** 1325–1331 (7 lines)
- `src/playsim/po_man.cpp` **FPolyObj::ClearSubsectorLinks** 1341–1375 (35 lines)
- `src/playsim/po_man.cpp` **FLevelLocals::ClearAllSubsectorLinks** 1377–1384 (8 lines)

**Credited native LOC:** 316 (tally `nativeLoc` 316).

## Managed

`VmPoManThrustBlock.cs`, `VmPoManLinkUpdate.cs`, `VmPoManSpatial.cs`
Managed LOC (tally): 461.
Conversion commit: `d3c0970e81081f8117b1334c6cf25086566f581d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`po_man.cpp`) |
|---------|-------------------------|
| `CheckMobjBlocking` | **1025–1146** |
| `LinkPolyobj` | **1154–1217** |
| `RecalcActorFloorCeil` | **1228–1248** |
| `ClosestPoint` | **1259–1317** |
| `PO_Busy` | **1325–1331** |
| `ClearSubsectorLinks` | **1341–1375** |
| `ClearAllSubsectorLinks` | **1377–1384** |

### vs chunk **679**

| | Lines |
|--|------:|
| **679** (`po-man-core-move-chunk679`) | **715–1017** |
| **677** (this row) | **1025–1384** |
| Gap (comments only, uncredited) | **1018–1024** |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **316** |
| Net behavior-ported | **316** |


**Post-review ratio:** 461/316 managed/native ≈ 146% (src file LOC at HEAD; see tally).

## Tests

- `VmPoManThrustBlockTests.CheckMobjBlocking_ThrustsSolidActorInRange`
- `VmPoManThrustBlockTests.CheckMobjBlocking_SkipsDuplicateMobj`
- `VmPoManLinkUpdateTests.LinkPolyobj_AssignsCellsInComputedBox`
- `VmPoManLinkUpdateTests.LinkPolyobj_SkipsCellsOutsideMap`
- `VmPoManLinkUpdateTests.RecalcActorFloorCeil_SkipsActorsOutsideBounds`
- `VmPoManSpatialTests.ClosestPoint_PicksNearestSegmentPoint`
- `VmPoManSpatialTests.PoBusy_TrueOnlyWhenSpecialDataPresent`
- `VmPoManSpatialTests.ClearSubsectorLinks_UnhooksHeadList`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 677 --to 677` (0 hard / 0 soft).

## Seams

Blockmap iteration, `P_IsBlockedByLine`, `P_FindFloorCeil`, poly node pool remain native at seams.

## Credit rationale

Part **2/2** of uncredited `po_man` **704–1393** band; hull **1025–1384** below **676** BSP credit (**1394+**). Pairs with **679** (no overlap).
