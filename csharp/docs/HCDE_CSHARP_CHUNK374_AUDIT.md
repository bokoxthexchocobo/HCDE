# Chunk 374 — FloodZones sound zones and extended/ZDoom node lump routing

## Scope

FloodZones sound zones and extended/ZDoom node lump routing

## Native ranges

- `src/maploader/maploader.cpp` **MapLoader::FloodZone** 342–370 (29 lines)
- `src/maploader/maploader.cpp` **MapLoader::FloodZones** 372–395 (24 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadExtendedNodes** 672–760 (89 lines)
- `src/maploader/maploader.cpp` **P_CheckV4Nodes** 771–782 (12 lines)

**Credited native LOC:** 154 (tally `nativeLoc` 154).

## Managed

`VmMapLoaderExtendedNodes.cs`, `VmMapLoaderExtendedNodesTests.cs`
Managed LOC (tally): 78.
Conversion commit: `d0cd2379a7b1f387ac7d34c58f16dbe2c4d8876b`.

## Tests

- `VmMapLoaderExtendedNodesTests.NextZoneIndex_CountsUnassignedSectors`
- `VmMapLoaderExtendedNodesTests.TryParseExtendedNodeId_Znod_IsCompressedType0`
- `VmMapLoaderExtendedNodesTests.IsV4NodeHeader_MatchesSignature`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 374 --to 374` (0 hard / 0 soft).

## Seams

Recursive `FloodZone`, decompressors, and `LoadZNodes` IO are not hosted; managed code mirrors zone assignment and node id routing.

## Credit rationale

Credits `FloodZone`, `FloodZones`, `LoadExtendedNodes`, and `P_CheckV4Nodes` from `maploader.cpp`.
