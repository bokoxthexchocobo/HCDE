# Chunk 210 — MAPINFO DoomEdNum table, parse loop, sorted dump labels.

## Scope

MAPINFO DoomEdNum table, parse loop, sorted dump labels.

## Native ranges

- `src/gamedata/g_doomedmap.cpp` **SpecialMapthingNames** 33–60 (28 lines)
- `src/gamedata/g_doomedmap.cpp` **CCMD dumpmapthings** 98–134 (37 lines)
- `src/gamedata/g_doomedmap.cpp` **FMapInfoParser::ParseDoomEdNums** 137–249 (113 lines)

**Credited native LOC:** 178 (tally `nativeLoc` 178).

## Managed

`VmDoomEdMapinfoParser.cs`, `VmDoomEdMapinfoParserTests.cs`
Managed LOC (tally): 155.
Conversion commit: `2885cdbe470a50fc97e90771ba26b58d6e396a0e`.

## Tests

- `VmDoomEdMapinfoParserTests.MatchSpecialMapthing_ResolvesPlayerStart`
- `VmDoomEdMapinfoParserTests.EnumerateSortedForDump_OrdersByEdNum`
- `VmDoomEdMapinfoParserTests.TryParseDoomEdNums_ReadsClassAndSpecial`
- `VmDoomEdMapinfoParserTests.TryParseDoomEdNums_DetectsDuplicate`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 210 --to 210` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
