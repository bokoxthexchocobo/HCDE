# Chunk 345 — WarpTrans resolution cluster lookup level reset

## Scope

WarpTrans resolution cluster lookup level reset

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **CheckWarpTrans through level_info Reset** 128–334 (207 lines)

**Credited native LOC:** 207 (tally `nativeLoc` 207).

## Managed

`VmMapInfoLevelReset.cs`, `VmMapInfoLevelResetTests.cs`, `VmMapInfoWarpTrans.cs`, `VmMapInfoWarpTransTests.cs`
Managed LOC (tally): 72.
Conversion commit: `7d8fadf33724f5fde5b2221019e24fc6d1c81dad`.

## Tests

- `VmMapInfoWarpTransTests.TryResolveWarpTransMap_UsesLevelRecord`
- `VmMapInfoLevelResetTests.CreateDefaults_SetsDoomLaxMonster`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 345 --to 345` (0 hard / 0 soft).

## Seams

Snapshot `Clean` and ACS string locks are modeled as policy hooks; full `level_info_t` storage stays native-shaped drafts.

## Credit rationale

Credits uncredited `g_mapinfo.cpp` 128–334: `CheckWarpTransMap`, secret-level visitation, cluster lookup, deferred clearing, and `level_info_t::Reset` defaults including Hexen vs Doom `flags2` behavior.
