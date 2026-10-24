# Chunk 209 — VOXELDEF sprite name list, option block, ApplyVoxelToSprites.

## Scope

VOXELDEF sprite name list, option block, ApplyVoxelToSprites.

## Native ranges

- `src/r_data/voxeldef.cpp` **VOX_ReadSpriteNames** 68–105 (38 lines)
- `src/r_data/voxeldef.cpp` **VOX_ReadOptions** 116–245 (130 lines)
- `src/r_data/voxeldef.cpp` **R_InitVoxels apply bindings** 312–327 (16 lines)

**Credited native LOC:** 184 (tally `nativeLoc` 184).

## Managed

`VmVoxelDefParser.cs`, `VmVoxelDefParserTests.cs`
Managed LOC (tally): 145.
Conversion commit: `357640266f9410382209afa1db4ece460dd6cf6a`.

## Tests

- `VmVoxelDefParserTests.TryReadSpriteNames_StopsAtEquals`
- `VmVoxelDefParserTests.TryReadOptions_ReadsScaleSpinAndOffsets`
- `VmVoxelDefParserTests.ApplyVoxelToSprites_ExpandsAllFrames`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 209 --to 209` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
