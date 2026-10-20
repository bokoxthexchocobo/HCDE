# Chunk 368 — Hexen SetLineID, SaveLineSpecial, FinishLoadingLineDef translucent handling

## Scope

Hexen SetLineID, SaveLineSpecial, FinishLoadingLineDef translucent handling

## Native ranges

- `src/maploader/maploader.cpp` **MapLoader::SetLineID** 1472–1527 (56 lines)
- `src/maploader/maploader.cpp` **MapLoader::SaveLineSpecial** 1535–1552 (18 lines)
- `src/maploader/maploader.cpp` **MapLoader::FinishLoadingLineDef** 1560–1631 (72 lines)
- `src/maploader/maploader.cpp` **MapLoader::FinishLoadingLineDefs** 1639–1645 (7 lines)

**Credited native LOC:** 153 (tally `nativeLoc` 153).

## Managed

`VmMapLoaderLineDefFinish.cs`, `VmMapLoaderLineDefFinishTests.cs`
Managed LOC (tally): 127.
Conversion commit: `bdf2d71fd35de03d5408400e6359fa4c4b6ee965`.

## Tests

- `VmMapLoaderLineDefFinishTests.ResolveHexenLineId_SetIdentification_UsesArg0WhenHack`
- `VmMapLoaderLineDefFinishTests.ResolveHexenLineId_PlaneAlign_RespectsNoSlopeCompat`
- `VmMapLoaderLineDefFinishTests.ShouldSaveLineSpecial_StaticInitColorOnly`
- `VmMapLoaderLineDefFinishTests.ResolveTranslucentLine_NegativeAlpha_IsAdditive`
- `VmMapLoaderLineDefFinishTests.ComputeTexelLength_RoundsHypotenuse`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 368 --to 368` (0 hard / 0 soft).

## Seams

`Level->tagManager`, `sidetemp`, and live `line_t` mutation are not hosted; managed code mirrors ID extraction, side-special save rules, texel length, and translucency policy.

## Credit rationale

Credits `MapLoader::SetLineID`, `SaveLineSpecial`, `FinishLoadingLineDef`, and `FinishLoadingLineDefs` from `maploader.cpp`.
