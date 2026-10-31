# Chunk 1324 — The voxel palette texture, the slab faces, and the frame draw from models_voxel.cpp (1-398).

## Scope

The voxel palette texture, the slab faces, and the frame draw from models_voxel.cpp (1-398).

## Native ranges

- `src/common/models/models_voxel.cpp` **FVoxelModel::Initialize** 1–398 (398 lines)

**Credited native LOC:** 398 (tally `nativeLoc` 398).

## Managed

`VmVoxelModel1324Slice.cs`
Managed LOC (tally): 544.
Conversion commit: `a75bff3a2e10d9111e87f245291ede1f3644b3b7`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Palette texture and the 6-bit expand (**45–144**) | yes |
| Vertex weld and the quad indices (**152–232**) | yes |
| Slab faces and the mip walk (**240–307**) | yes |
| Buffer, skins, frame, and the aspect (**315–398**) | yes |
| Band hull (**1–398**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| The commented no-compress flag | it does not run |
| KVX load and the mip allocator | they live in voxels.cpp |

The texture is 16 by 16. A palette byte expands with `(c << 2) | (c >> 4)`. Luminance stores the expanded red. Any other conversion asks the matcher, which returns 1 when no palette is set and otherwise searches indices 1 through 254, or 0 through 254 when an index map is set. Missing palette bytes become the identity 0..255. The square is transposed about the diagonal. CopyPixels writes identity indices through a BGRA palette and forces alpha 255, including the base-color fallback. Faces subtract the mip pivot, swap Y/Z with a sign flip, and pack `(col & 15)` and `col / 16` into U and V. The quad stores the fourth corner before the third and emits indices 0,1,3 then 1,2,3. Equal colors in a slab share one side run. Cull bits 1, 2, 4, 8, 16, and 32 select the six faces. The bottom face uses the last color. Vertices with the same position and U/V are stored once. BuildVertexBuffer fills one buffer per renderer type and then drops the temporary arrays. AddSkins sets the flat bit on the palette texture. Load returns false. FindFrame returns -2 when no default is allowed, otherwise 0. The aspect factor is the stretch. RenderFrame clamps the skin, sets up frame 0, and draws every index.

| Managed (`VmVoxelModel1324Slice.cs`) | Native (`models_voxel.cpp`) |
|--------------------------------------|-----------------------------|
| Texture, slabs, buffer, and the frame | **1–398** |
| Band hull (exclusive slice) | **1–398** |

### Chunk **1324** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **398** |
| **Net headline** | **+398** vs post-**1323** |


**Post-review ratio:** 544/398 managed/native ≈ 137% (src file LOC at HEAD; see tally).

## Tests

- `VmVoxelModel1324SliceTests.Palette_ExpandsLuminanceAndCopiesTheBitmap`
- `VmVoxelModel1324SliceTests.Slab_WeldsTheSharedEdgeAndSplitsTheColorRun`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1324 --to 1324` (0 hard / 0 soft).

## Seams

ColorMatcher.Pick and BestColor live in colormatcher.h and palette.cpp. FlipSquareBlock lives in imagehelpers.h. CopyPixelData lives in bitmap.cpp. The vertex map compare lives in model_kvx.h. This slice includes those callees so the palette indices, the transposed pixels, the BGRA copy, and the welded vertices match.

## Credit rationale

UNCredited models_voxel.cpp 1-398 (+398). A full tally line scan shows no overlap. The file ends at line 398.
