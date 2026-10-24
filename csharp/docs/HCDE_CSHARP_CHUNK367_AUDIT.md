# Chunk 367 — LoadSectors defaults/fade policy and LoadNodes BSP child validation

## Scope

LoadSectors defaults/fade policy and LoadNodes BSP child validation

## Native ranges

- `src/maploader/maploader.cpp` **MapLoader::LoadSectors** 1056–1146 (91 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadNodes** 1156–1230 (75 lines)

**Credited native LOC:** 166 (tally `nativeLoc` 166).

## Managed

`VmMapLoaderClassicNodes.cs`, `VmMapLoaderClassicNodesTests.cs`, `VmMapLoaderSectorInit.cs`, `VmMapLoaderSectorInitTests.cs`
Managed LOC (tally): 86.
Conversion commit: `6e514d7088c622bfc2a6f3c6827a24d90cc32b8c`.

## Tests

- `VmMapLoaderSectorInitTests.ResolveDefaultSeqType_TotalCtrl_IsZero`
- `VmMapLoaderSectorInitTests.ShouldSetFloorDropFlag_OnlyWhenNoBehavior`
- `VmMapLoaderSectorInitTests.ResolveFadeColor_OutsideFog_WhenSkyCeiling`
- `VmMapLoaderClassicNodesTests.ShouldAttemptLoad_RejectsEmptyNodesWithoutSingleSubsector`
- `VmMapLoaderClassicNodesTests.ResolveChild_InvalidSubsector_WhenIndexTooLarge`
- `VmMapLoaderClassicNodesTests.ResolveChild_NodeAlreadyUsed_WhenDuplicate`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 367 --to 367` (0 hard / 0 soft).

## Seams

Sector allocation, `SetTexture`, and pointer wiring are not hosted; managed code mirrors seq-type defaults, behavior flags, fade selection, and BSP child checks.

## Credit rationale

Credits `MapLoader::LoadSectors` and template `MapLoader::LoadNodes` from `maploader.cpp`.
