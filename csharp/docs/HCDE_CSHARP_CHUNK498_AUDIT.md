# Chunk 498 — P_ChangeSector attached 3D floors, iterator dispatch, fake-floor water refresh Retired (2026-10-24): `--band` hull overlaps existing credit (p-map-crush-tail-decal-activate-chunk499 src/playsim/p_map.cpp 7238-7238).

## Scope

P_ChangeSector attached 3D floors, iterator dispatch, fake-floor water refresh Retired (2026-10-24): `--band` hull overlaps existing credit (p-map-crush-tail-decal-activate-chunk499 src/playsim/p_map.cpp 7238-7238).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `99447bf5559cda7950d634a57d1252da63166b95`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `AbsMoveAmount` / `ProcessAttached3dFloors` / `ShouldMoveThing` / `UseInvertedCeilingIterator` / `FloorDropWhenNegative` / `FloorRaiseWhenPositive` / `PickFloorDrop` / `PickFloorRaise` / `PickCeilingLower` / `PickCeilingRaise` / `MidtexMoveFlag` / `RunAttachedPass` / `AttachedUsesFloorRaise` / `AttachedUsesFloorDrop` / `DualIteratorForMidtex` / `PortalCheckFloorOrCeiling` / `ResetCrushChange` / `MoveMidtexWhen3d` / `SelectFloorIterator` / `SelectCeilingIterator` | `P_ChangeSector` 7116–7175 (7132: `if (sector->e->XFloor.attached.Size() && floorOrCeil != 2)`; 7139: `if (floorOrCeil == 1)`; 7154: `if (sec->heightsec == sector) continue;`) |
| `MidtexUsesBothIterators` / `InvalidFloorOrCeil` / `CheckPortalAfterMove` / `MarkVisited` / `VisitUnprocessed` | `P_ChangeSector` 7176–7237 (7203: `return false;`; 7223: `if (!n->visited) // unprocessed thing found`; 7226: `if (!(n->m_thing->flags & MF_NOBLOCKMAP) || //jff 4/7/98 don't do these`) |
| `UpdateFakeFloorSectors` / `FakeFloorRefresh` / `ProcessFakeThing` / `WaterTriggerZ` | `P_ChangeSector` 7239–7271 (7239: `if (!cpos.nofit && !isreset /* && sector->MoreFlags & (SECMF_UNDERWAT…`; 7257: `if (!n->visited && n->m_thing->Sector == s) // unprocessed thing found`; 7270: `return cpos.nofit;`) |

### Out-of-body uncredit (498)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **155** |
| **Out-of-body removed** | **0** |
| **Honest** | **155** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 498 --to 498` (0 hard / 0 soft).

## Seams

`P_Recalculate3DFloors`, sector thing lists, pit iterators, and `UpdateWaterLevel` are not hosted.

## Credit rationale

Credits move amount setup, attached-sector iterator inversion, thing visit loops, midtex dual dispatch, portal checks, and fake-floor water updates from `p_map.cpp` (7116–7271).
