# Chunk 212 — R_InitSpriteDefs texture hash and voxel name spin parse.

## Scope

R_InitSpriteDefs texture hash and voxel name spin parse.

## Native ranges

- `src/r_data/sprites.cpp` **R_InitSpriteDefs hash** 328–341 (14 lines)
- `src/r_data/sprites.cpp` **voxel name parse** 347–395 (49 lines)

**Credited native LOC:** 63 (tally `nativeLoc` 63).

## Managed

`VmSpriteTextureHasher.cs`, `VmSpriteTextureHasherTests.cs`
Managed LOC (tally): 55.
Conversion commit: `1509d90300cfd1ec535a43fd9dff5a3febd3bb9c`.

## Tests

- `VmSpriteTextureHasherTests.BuildSpriteTextureChains_LinksSamePrefix`
- `VmVoxelSpinNameParserTests.TryParse_ReadsSpinSign`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 212 --to 212` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
