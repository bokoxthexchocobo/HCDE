# Chunk 297 — ParseEpisodeInfo helpers (merged 297-299)

## Scope

ParseEpisodeInfo helpers (merged 297-299)

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **ParseEpisodeInfo** 2421–2570 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 48).

## Managed

`VmMapInfoEpisodeParser.cs`, `VmMapInfoEpisodeParserTests.cs`
Managed LOC (tally): 55.
Conversion commit: `84ea0afb2166c6af147e43f1c08090551870bd7a`.

## Tests

- `VmMapInfoEpisodeParserTests.ShouldIgnoreExtendedEpisode_WhenNotMenuHack`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 297 --to 297` (0 hard / 0 soft).

## Seams

MAPINFO handlers omit live `FMapInfoParser` scanner state; managed code models option semantics only.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
