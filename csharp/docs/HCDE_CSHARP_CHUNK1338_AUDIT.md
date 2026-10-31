# Chunk 1338 — VOX_ReadSpriteNames, VOX_ReadOptions, and R_InitVoxels from voxeldef.cpp (1-331).

## Scope

VOX_ReadSpriteNames, VOX_ReadOptions, and R_InitVoxels from voxeldef.cpp (1-331).

## Native ranges

- `src/r_data/voxeldef.cpp` **R_InitVoxels** 1–331 (331 lines)

**Credited native LOC:** 331 (tally `nativeLoc` 331).

## Managed

`VmVoxelDef1338Slice.cs`
Managed LOC (tally): 426.
Conversion commit: `fbf025ca807f31826266b25efa57c0b1473ebc64`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Voxel option defaults (**43–56**) | yes |
| Sprite name list (**68–105**) | yes |
| Option block (**116–245**) | yes |
| R_InitVoxels (**256–331**) | yes |
| Band hull (**1–331**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| GetSpriteIndex body | callee; only the first four characters are indexed |
| FindLump and CheckNumForFullName | filesystem callees |
| VOX_GetVoxel and VOX_AddVoxel bodies | callees; AddVoxel is recorded per frame |

A five-character sprite uppercases the frame character before the range check. Frame letters run from A through the 29-frame limit. A four-character name applies the voxel to every frame. A missing sprite index is skipped. An equals sign ends the name list, and an empty list reports that no sprites were specified. End of file with names still open reports the unexpected end and returns false. Spin writes both spin fields. Placed and dropped spin write one field. A leading minus negates the following number. Angle offset stores the signed degrees plus 90. Offsets accept an integer or a float. Unknown options are reported, and a following equals consumes one token. An option block that runs out of tokens reports that it is unterminated. Each VOXELDEF lump opens its own scanner. The voxel file name is looked up in the voxel namespace. A missing file or a null voxel is reported and skipped. Override palette removes the palette only when a voxel and at least one sprite exist. Frame 255 walks frames from the last frame down to zero and shares one definition.

| Managed (`VmVoxelDef1338Slice.cs`) | Native (`voxeldef.cpp`) |
|-------------------------------------|-------------------------|
| Sprite names, options, and R_InitVoxels | **1–331** |
| Band hull (exclusive slice) | **1–331** |

### Chunk **1338** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **331** |
| **Net headline** | **+331** vs post-**1337** |


**Post-review ratio:** 426/331 managed/native ≈ 129% (src file LOC at HEAD; see tally).

## Tests

- `VmVoxelDef1338SliceTests.SpriteNamesAndOptions`
- `VmVoxelDef1338SliceTests.InitAppliesEveryFrameAndSkipsMissingFiles`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1338 --to 1338` (0 hard / 0 soft).

## Seams

FindLump, GetSpriteIndex, CheckNumForFullName, and VOX_GetVoxel stay injected. VOX_AddVoxel records the sprite and frame on the shared definition. FScanner token errors throw.

## Credit rationale

UNCredited voxeldef.cpp 1-331 (+331). A full tally line scan shows no overlap. This whole file was tied with s_sound_eternity.cpp 1-331. Path order credited the audio facade as chunk 1337, so this is now the largest remaining hole of at most 400 lines.
