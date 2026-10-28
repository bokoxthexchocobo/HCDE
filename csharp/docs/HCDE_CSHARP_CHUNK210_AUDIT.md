# Chunk 210 — MAPINFO DoomEdNum table, parse loop, sorted dump labels.

## Scope

MAPINFO DoomEdNum table, parse loop, sorted dump labels.

## Native ranges

- `src/gamedata/g_doomedmap.cpp` **SpecialMapthingNames** 33–58 (26 lines)
- `src/gamedata/g_doomedmap.cpp` **SpecialMapthingNames** 60–60 (1 lines)
- `src/gamedata/g_doomedmap.cpp` **CCMD dumpmapthings** 98–134 (37 lines)
- `src/gamedata/g_doomedmap.cpp` **FMapInfoParser::ParseDoomEdNums** 137–249 (113 lines)

**Credited native LOC:** 177 (tally `nativeLoc` 177).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 181.
Conversion commit: `a47004baaf54b86feee4fbdbd42c0bb18b16055f`.

## Padding review (2026-10-21)



### Out-of-body uncredit (210)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **178** |
| **Out-of-body removed** | **1** |
| **Honest** | **177** |


**Post-review ratio:** 181/177 managed/native ≈ 102% (src file LOC at HEAD; see tally).

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
