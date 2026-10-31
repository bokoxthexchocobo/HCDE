# Chunk 1269 — Sorted 3D-floor heights, skybox save, and the clip-stack reset from r_3dfloors.cpp (1-199).

## Scope

Sorted 3D-floor heights, skybox save, and the clip-stack reset from r_3dfloors.cpp (1-199).

## Native ranges

- `src/rendering/swrenderer/scene/r_3dfloors.cpp` **Clip3DFloors** 1–199 (199 lines)

**Credited native LOC:** 199 (tally `nativeLoc` 199).

## Managed

`VmFloors1269Slice.cs`
Managed LOC (tally): 224.
Conversion commit: `c570dee80b0e31cadb0b68dc20322516f396ef32`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Clip3DFloors (**39–42**) | yes |
| SetFakeFloor (**44–52**) | yes |
| Cleanup (**54–66**) | yes |
| DeleteHeights (**68–79**) | yes |
| AddHeight (**81–125**) | yes |
| NewClip (**127–149**) | yes |
| ResetClip (**151–164**) | yes |
| EnterSkybox (**166–181**) | yes |
| LeaveSkybox (**183–198**) | yes |
| Band hull (**1–199**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Plane sample | host sector |
| Clip bytes | host viewport |

A height at or outside the sector floor or ceiling is ignored and does not mark the floor active. A repeated height marks the floor active and does not insert or advance the count. New heights are inserted in ascending order, including in front of the head, and the tail stays put unless the height is appended. Entering a skybox saves the list and starts empty without freeing the saved nodes. Leaving restores that list. A leave with nothing saved replaces the live list with an empty one and does not free the dropped nodes, and the skybox count goes negative. Cleanup frees the current list before each restore, then resets the clip stack and frees the restored list. Clip copies are independent of the source buffers. A second request for the same floor reuses the record. A clip with no floor faults.

| Managed (`VmFloors1269Slice.cs`) | Native (`r_3dfloors.cpp`) |
|-----------------------------------|---------------------------|
| Heights, skybox, and clips | **1–199** |
| Band hull (exclusive slice) | **1–199** |

### Chunk **1269** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **199** |
| **Net headline** | **+199** vs post-**1268** |


**Post-review ratio:** 224/199 managed/native ≈ 113% (src file LOC at HEAD; see tally).

## Tests

- `VmFloors1269SliceTests.Heights_InsertInOrderAndASpareLeaveDropsTheList`
- `VmFloors1269SliceTests.Clips_CopyTheBuffersAndCleanupFreesBothLists`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1269 --to 1269` (0 hard / 0 soft).

## Seams

The plane height and the clip-buffer bytes are host inputs. The slice keeps the sorted insert, the skybox save, the spare leave, and the cleanup free order.

## Credit rationale

UNCredited r_3dfloors.cpp 1-199 (+199). A full tally line scan shows no overlap. The file ends at line 199.
