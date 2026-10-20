# Chunk 346 — LookupLevelName and MAPINFO brace format helpers

## Scope

LookupLevelName and MAPINFO brace format helpers

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **LookupLevelName parser format** 337–518 (182 lines)

**Credited native LOC:** 182 (tally `nativeLoc` 182).

## Managed

`VmMapInfoLookupLevelName.cs`, `VmMapInfoLookupLevelNameTests.cs`, `VmMapInfoParserFormat.cs`, `VmMapInfoParserFormatTests.cs`
Managed LOC (tally): 58.
Conversion commit: `6546c28c2e5aea3dd28576b65080a1aa71c80e65`.

## Tests

- `VmMapInfoLookupLevelNameTests.StripLocalizedHeader_RemovesMapPrefix`
- `VmMapInfoParserFormatTests.ResolveOpenBrace_DetectsNewFormat`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 346 --to 346` (0 hard / 0 soft).

## Seams

Inventory-based `CheckLevelRedirect` still depends on engine actors; this chunk ports name stripping and new-vs-old brace parsing only.

## Credit rationale

Credits `g_mapinfo.cpp` 337–518: localized level-name header stripping, validity checks, `cluster_info_t::Reset`, and `FMapInfoParser` open/close brace plus assign/comma/number helpers for new-format MAPINFO.
