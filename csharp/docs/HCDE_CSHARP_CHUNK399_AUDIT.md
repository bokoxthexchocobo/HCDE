# Chunk 399 — ParseGameInfo, GetReserved, CheckGameInfo, SetMapxxFlag

## Scope

ParseGameInfo, GetReserved, CheckGameInfo, SetMapxxFlag

## Native ranges

- `src/d_main.cpp` **ParseGameInfo** 2311–2435 (125 lines)
- `src/d_main.cpp` **GetReserved** 2437–2443 (7 lines)
- `src/d_main.cpp` **CheckGameInfo** 2445–2465 (21 lines)
- `src/d_main.cpp` **SetMapxxFlag** 2473–2480 (8 lines)

**Credited native LOC:** 161 (tally `nativeLoc` 161).

## Managed

`VmDMainGameInfoScan.cs`, `VmDMainGameInfoScanTests.cs`
Managed LOC (tally): 132.
Conversion commit: `015376bee1e0b773286acc53bcdf36d27532ed7e`.

## Tests

- `VmDMainGameInfoScanTests.TryApplyGameInfoKey_StartupTitle`
- `VmDMainGameInfoScanTests.ParseStartupType_Doom`
- `VmDMainGameInfoScanTests.ReservedFolders_IncludesMaps`
- `VmDMainGameInfoScanTests.ShouldSetMapxxFlag_WhenAnyMap01Present`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 399 --to 399` (0 hard / 0 soft).

## Seams

`FScanner`, `FileSystem::InitMultipleFiles`, and `gameinfo` mutation are not hosted.

## Credit rationale

Credits static `ParseGameInfo`, `GetReserved`, `CheckGameInfo`, and `SetMapxxFlag` from `d_main.cpp`.
