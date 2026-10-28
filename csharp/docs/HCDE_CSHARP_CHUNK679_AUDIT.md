# Chunk 679 — Polyobject ctor, thrust, bbox/link prep, move/rotate, unlink (`po_man.cpp` **715–1017**).

## Scope

Polyobject ctor, thrust, bbox/link prep, move/rotate, unlink (`po_man.cpp` **715–1017**).

## Native ranges

- `src/playsim/po_man.cpp` **FPolyObj::FPolyObj** 715–730 (16 lines)
- `src/playsim/po_man.cpp` **FPolyObj::GetMirror** 738–741 (4 lines)
- `src/playsim/po_man.cpp` **FPolyObj::ThrustMobj** 749–795 (47 lines)
- `src/playsim/po_man.cpp` **FPolyObj::UpdateLinks** 803–831 (29 lines)
- `src/playsim/po_man.cpp` **FPolyObj::UpdateBBox** 833–840 (8 lines)
- `src/playsim/po_man.cpp` **FPolyObj::CalcCenter** 842–850 (9 lines)
- `src/playsim/po_man.cpp` **FPolyObj::MovePolyobj** 858–888 (31 lines)
- `src/playsim/po_man.cpp` **FPolyObj::DoMovePolyobj** 896–910 (15 lines)
- `src/playsim/po_man.cpp` **RotatePt** 918–927 (10 lines)
- `src/playsim/po_man.cpp` **FPolyObj::RotatePolyobj** 935–982 (48 lines)
- `src/playsim/po_man.cpp` **FPolyObj::UnLinkPolyobj** 990–1017 (28 lines)

**Credited native LOC:** 245 (tally `nativeLoc` 245).

## Managed

`VmPoManPolyThinkerBands.cs`, `VmPoManMoveRotate.cs`
Managed LOC (tally): 428.
Conversion commit: `399da7a7facb1dab62d7cf7cbe0662b041f436e5`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`po_man.cpp`) |
|---------|-------------------------|
| `FPolyObj` | **715–730** |
| `GetMirror` | **738–741** |
| `ThrustMobj` | **749–795** |
| `UpdateLinks` | **803–831** |
| `UpdateBBox` | **833–840** |
| `CalcCenter` | **842–850** |
| `MovePolyobj` | **858–888** |
| `DoMovePolyobj` | **896–910** |
| `RotatePt` | **918–927** |
| `RotatePolyobj` | **935–982** |
| `UnLinkPolyobj` | **990–1017** |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **245** |
| Net behavior-ported | **245** |


**Post-review ratio:** 428/245 managed/native ≈ 175% (src file LOC at HEAD; see tally).

## Tests

- `VmPoManPolyThinkerBandsTests.FPolyObjConstruct_ClearsDefaults`
- `VmPoManPolyThinkerBandsTests.FPolyObjGetMirror_ReturnsMirrorNum`
- `VmPoManThrustBlockTests.ThrustMobj_AddsVelocityFromSideNormal`
- `VmPoManLinkUpdateTests.UpdateLinks_MovesDisplacementGroupOncePerPortalGroup`
- `VmPoManLinkUpdateTests.UpdateBBox_AdjustsLinesAndCenter`
- `VmPoManLinkUpdateTests.CalcCenter_AveragesVertices`
- `VmPoManMoveRotateTests.MovePolyobj_RollsBackWhenBlocked`
- `VmPoManMoveRotateTests.MovePolyobj_AppliesDeltaWhenClear`
- `VmPoManPolyThinkerBandsTests.RotatePt_RotatesAboutStart`
- `VmPoManMoveRotateTests.RotatePolyobj_RestoresVerticesWhenBlocked`
- `VmPoManLinkUpdateTests.LinkPolyobj_AssignsCellsInComputedBox`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 679 --to 679` (0 hard / 0 soft).

## Seams

Blockmap polyblocks, `P_CheckMove`/`P_DamageMobj`/`P_FindFloorCeil`, portal geometry injected at seams; thinker bands outside this hull stay other rows.

## Credit rationale

Contiguous uncredited `po_man.cpp` band between **671** (**≤703**) and **676** (**≥1394**). Part **1/2** of retired **670/673/678** merge (no overlap with **671**).
