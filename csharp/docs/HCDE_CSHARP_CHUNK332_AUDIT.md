# Chunk 332 — GetDefaultLevelNum and ParseMapHeader map title paths

## Scope

GetDefaultLevelNum and ParseMapHeader map title paths

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **GetDefaultLevelNum ParseMapHeader** 2295–2443 (149 lines)

**Credited native LOC:** 149 (tally `nativeLoc` 149).

## Managed

`VmMapInfoParseMapHeader.cs`, `VmMapInfoParseMapHeaderTests.cs`
Managed LOC (tally): 48.
Conversion commit: `ffb09f7b037de026d3b2515afca8e5b2bc045eb0`.

## Tests

- `VmMapInfoParseMapHeaderTests.ResolveMapNameToken_OldHexenNumber`
- `VmMapInfoParseMapHeaderTests.ApplyLevelTitle_DollarPrefixSetsLookup`
- `VmMapInfoParseMapHeaderTests.AssignDefaultLevelNum_ParsesMap01`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 332 --to 332` (0 hard / 0 soft).

## Seams

`GStrings`, `HexenMusic`, and `wadlevelinfos` storage are injected or reduced to draft fields; Hexen IWAD localization is represented only by the lookup-level-name decision.

## Credit rationale

Fills 2295–2443 in `g_mapinfo.cpp`: the file-local `GetDefaultLevelNum` helper and `ParseMapHeader` map-name token handling, Hexen-hack default flag seeding, `$`/lookup level titles, and default `levelnum` assignment ahead of episode parsing at 2459.
