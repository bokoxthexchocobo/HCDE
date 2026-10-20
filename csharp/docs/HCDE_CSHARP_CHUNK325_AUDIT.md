# Chunk 325 — EMAPINFO string helpers value list bool int double flags levelnum registry

## Scope

EMAPINFO string helpers value list bool int double flags levelnum registry

## Native ranges

- `src/gamedata/hcde_emapinfo.cpp` **Trim through RememberEMapInfoMap** 55–122 (68 lines)
- `src/gamedata/hcde_emapinfo.cpp` **SplitValueList ParseBool ParseInt ParseDouble SetFlag** 174–290 (117 lines)

**Credited native LOC:** 185 (tally `nativeLoc` 185).

## Managed

`VmEMapInfoLineParserTests.cs`, `VmEMapInfoParsePrimitives.cs`, `VmEMapInfoParsePrimitivesTests.cs`, `VmEMapInfoRegistry.cs`, `VmEMapInfoRegistryTests.cs`
Managed LOC (tally): 130.
Conversion commit: `b087fb7a7966cafc27943be5e340a1b3bcce65ef`.

## Tests

- `VmEMapInfoLineParserTests.NormalizeResourceName_StripsLeadingSlashes`
- `VmEMapInfoRegistryTests.RememberMap_TracksNames`
- `VmEMapInfoLineParserTests.SplitValueList_SplitsOnComma`
- `VmEMapInfoLineParserTests.TryParseBool_TreatsEmptyAsTrue`
- `VmEMapInfoParsePrimitivesTests.TryParseIntStrict_RejectsTrailingGarbage`
- `VmEMapInfoParsePrimitivesTests.TryParseDoubleStrict_ParsesDecimal`
- `VmEMapInfoParsePrimitivesTests.SetFlag_TogglesSignedBits`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 325 --to 325` (0 hard / 0 soft).

## Seams

Anonymous-namespace stats tables and `wadlevelinfos` mutation are not hosted; `RememberEMapInfoMap` is a managed set and strict parsers reject trailing junk like `strtol`/`strtod` guards.

## Credit rationale

Credits the uncredited head of `hcde_emapinfo.cpp` before chunk 273's `StripLineComment` row: trim/quote/normalize helpers, map-name registry, `SplitValueList`, strict bool/int/double parsers, signed/unsigned `SetFlag`, Skips chunk 273's 124–167 rows; `GetDefaultLevelNum` body rows remain for a later chunk (283 already owns 294–301).
