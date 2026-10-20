# Chunk 308 — G_ParseMapInfo FindLumpMulti skip/precedence

## Scope

G_ParseMapInfo FindLumpMulti skip/precedence

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **G_ParseMapInfo lump scan** 3032–3066 (35 lines)

**Credited native LOC:** 35 (tally `nativeLoc` 35).

## Managed

`VmMapInfoLumpPrecedence.cs`, `VmMapInfoLumpPrecedenceTests.cs`
Managed LOC (tally): 38.
Conversion commit: `594a951b821d96b72be08b38bcb7380dfed3a002`.

## Tests

- `VmMapInfoLumpPrecedenceTests.ShouldSkipClassicMapinfo_WhenZmapinfoPresent`
- `VmMapInfoLumpPrecedenceTests.ShouldSkipEmapinfo_WhenNativeMapinfoInSameWad`
- `VmMapInfoLumpPrecedenceTests.ShouldSkipUmapinfo_WhenZmapinfoPresent`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 308 --to 308` (0 hard / 0 soft).

## Seams

MAPINFO handlers omit live `FMapInfoParser` scanner state; managed code models option semantics only.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
