# Chunk 394 — GroupLines sector tables and LoadReject matrix ingest

## Scope

GroupLines sector tables and LoadReject matrix ingest

## Native ranges

- `src/maploader/maploader.cpp` **MapLoader::GroupLines** 2659–2786 (128 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadReject** 2794–2849 (56 lines)

**Credited native LOC:** 184 (tally `nativeLoc` 184).

## Managed

`VmMapLoaderGroupLines.cs`, `VmMapLoaderGroupLinesLoadRejectTests.cs`, `VmMapLoaderLoadReject.cs`, `VmMapLoaderProcessSideTexturesTests.cs`
Managed LOC (tally): 150.
Conversion commit: `49afaab7fa92ed9d1eaa6195f2afb38c504119fa`.

## Tests

- `VmMapLoaderGroupLinesTests.ComputeBoundingBoxCenter_Midpoint`
- `VmMapLoaderGroupLinesTests.AssignLinesToSectors_Balanced`
- `VmMapLoaderGroupLinesTests.ShouldInvokeSetSlopes_WhenNotBuildMap`
- `VmMapLoaderLoadRejectTests.ComputeNeededRejectBytes_ForFourSectors`
- `VmMapLoaderLoadRejectTests.IsRejectMatrixAllZero_DetectsNonZeroTail`
- `VmMapLoaderLoadRejectTests.ShouldClearReject_WhenLumpTooSmall`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 394 --to 394` (0 hard / 0 soft).

## Seams

`Level->tagManager`, `SetSlopes`, and `MapData` lump IO are not hosted.

## Credit rationale

Credits `MapLoader::GroupLines` and `MapLoader::LoadReject` from `maploader.cpp`.
