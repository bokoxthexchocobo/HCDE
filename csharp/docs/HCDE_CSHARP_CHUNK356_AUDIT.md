# Chunk 356 — AM_ParseArrow lump parser and AM_StaticInit mark textures

## Scope

AM_ParseArrow lump parser and AM_StaticInit mark textures

## Native ranges

- `src/am_map.cpp` **thintriangle_guy** 853–857 (5 lines)
- `src/am_map.cpp` **AM_ParseArrow** 865–895 (31 lines)
- `src/am_map.cpp` **AM_StaticInit arrows and marks** 897–917 (21 lines)

**Credited native LOC:** 57 (tally `nativeLoc` 57).

## Managed

`VmAutomapArrowParser.cs`, `VmAutomapArrowParserTests.cs`
Managed LOC (tally): 97.
Conversion commit: `10f56fc601b49cd82c156ee148e48d75b7411ad5`.

## Tests

- `VmAutomapArrowParserTests.DefaultThinTriangle_HasThreeSegments`
- `VmAutomapArrowParserTests.ParseArrowLump_ReadsOneSegment`
- `VmAutomapArrowParserTests.ValidatePrimaryArrowDefined_ThrowsWhenEmpty`
- `VmAutomapArrowParserTests.FormatMarkTextureName_MatchesAmmnumPattern`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 356 --to 356` (0 hard / 0 soft).

## Seams

`fileSystem` lump IO and `TexMan` lookups are callbacks; vector arrays mirror `mline_t` scaling.

## Credit rationale

Credits default thin triangle, `AM_ParseArrow` coordinate scaling, fatal guard when no arrow, and `AMMNUM` mark lump names.
