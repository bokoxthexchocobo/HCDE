# Chunk 358 — MapLoader SetTexture colormap and hex blend path

## Scope

MapLoader SetTexture colormap and hex blend path

## Native ranges

- `src/maploader/maploader.cpp` **SetTexture blend** 243–269 (27 lines)

**Credited native LOC:** 27 (tally `nativeLoc` 27).

## Managed

`VmMapLoaderTextureBlend.cs`, `VmMapLoaderTextureBlendTests.cs`
Managed LOC (tally): 41.
Conversion commit: `a8fd93b9c452c856825bdacbff133b3df9be8ada`.

## Tests

- `VmMapLoaderTextureBlendTests.ResolveBlendName_UsesColormapWhenPresent`
- `VmMapLoaderTextureBlendTests.TryParseHexBlend_ReadsEightChars`
- `VmMapLoaderTextureBlendTests.ResolveBlendName_FallsBackToHex`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 358 --to 358` (0 hard / 0 soft).

## Seams

`R_ColormapNumForName` and sidedef mutation are injected; blend resolution returns flags only.

## Credit rationale

Credits deep-water sidedef `SetTexture` that prefers colormap numbers, wall textures, then 8-char hex blends.
