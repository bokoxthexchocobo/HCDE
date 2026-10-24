# Chunk 382 — LoadLineDefs and LoadLineDefs2 vertex/zero-length preprocessing and activation normalization

## Scope

LoadLineDefs and LoadLineDefs2 vertex/zero-length preprocessing and activation normalization

## Native ranges

- `src/maploader/maploader.cpp` **MapLoader::LoadLineDefs** 1676–1779 (104 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadLineDefs2** 1787–1879 (93 lines)

**Credited native LOC:** 197 (tally `nativeLoc` 197).

## Managed

`VmMapLoaderLoadLineDefs.cs`, `VmMapLoaderLoadLineDefsTests.cs`
Managed LOC (tally): 69.
Conversion commit: `9fff9396e5b4dc03de61f81de6af4f6bd6880a90`.

## Tests

- `VmMapLoaderLoadLineDefsTests.IsVertexIndexInvalid_DetectsOutOfRange`
- `VmMapLoaderLoadLineDefsTests.IsZeroLengthLine_DetectsCoincidentVertices`
- `VmMapLoaderLoadLineDefsTests.PatchMissingFirstSide_ReplacesNoIndex`
- `VmMapLoaderLoadLineDefsTests.ShouldForceNodeBuildOnSkip_WhenLinesRemoved`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 382 --to 382` (0 hard / 0 soft).

## Seams

`TranslateLineDef`, `AllocateSideDefs`, and `SetLineID` bodies are not fully hosted.

## Credit rationale

Credits `MapLoader::LoadLineDefs` and `MapLoader::LoadLineDefs2` from `maploader.cpp`.
