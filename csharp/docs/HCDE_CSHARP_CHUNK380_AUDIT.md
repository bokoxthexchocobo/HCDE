# Chunk 380 — MakeSkill, LoadThings doom conversion, and LoadThings2 hexen read defaults

## Scope

MakeSkill, LoadThings doom conversion, and LoadThings2 hexen read defaults

## Native ranges

- `src/maploader/maploader.cpp` **MakeSkill** 1281–1288 (8 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadThings** 1290–1369 (80 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadThings2** 1383–1428 (46 lines)

**Credited native LOC:** 134 (tally `nativeLoc` 134).

## Managed

`VmMapLoaderLoadThings.cs`, `VmMapLoaderLoadThingsTests.cs`
Managed LOC (tally): 147.
Conversion commit: `92408972b5c2296335e7e8a7d40fb80fa2d2639f`.

## Tests

- `VmMapLoaderLoadThingsTests.MakeSkill_MapsDoomSpawnBits`
- `VmMapLoaderLoadThingsTests.TranslateDoomThingFlags_AppliesFriendlyBit`
- `VmMapLoaderLoadThingsTests.CountDoomThings_DividesLumpSize`
- `VmMapLoaderLoadThingsTests.ApplyHexenHackMask_StripsStrifeBits`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 380 --to 380` (0 hard / 0 soft).

## Seams

`DoomEdMap`, `ProcessEDMapthing`, and `SpawnMapThing` are not hosted.

## Credit rationale

Credits `MakeSkill`, `MapLoader::LoadThings`, and `MapLoader::LoadThings2` from `maploader.cpp`.
