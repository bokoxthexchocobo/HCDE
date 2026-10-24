# Chunk 355 — FMapInfoParser ParseAMColors MAPINFO automap color blocks

## Scope

FMapInfoParser ParseAMColors MAPINFO automap color blocks

## Native ranges

- `src/am_map.cpp` **ParseAMColors** 767–838 (72 lines)

**Credited native LOC:** 72 (tally `nativeLoc` 72).

## Managed

`VmAutomapMapinfoColorsParser.cs`, `VmAutomapMapinfoColorsParserTests.cs`
Managed LOC (tally): 99.
Conversion commit: `c0a30a10c324a343d2acc23dc05d4948993a78c9`.

## Tests

- `VmAutomapMapinfoColorsParserTests.TryParseBasePreset_AcceptsDoom`
- `VmAutomapMapinfoColorsParserTests.ValidateBaseKeyOrder_RejectsLateBase`
- `VmAutomapMapinfoColorsParserTests.TryResolveColorNameIndex_FindsWallColor`
- `VmAutomapMapinfoColorsParserTests.TryParseShowLocks_FalseLiteralHidesLocks`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 355 --to 355` (0 hard / 0 soft).

## Seams

Scanner tokens and `V_GetColorFromString` are injected; `AMColorset` storage stays native.

## Credit rationale

Credits the full `ParseAMColors` loop: base preset selection, `showlocks`, and per-slot color name dispatch.
