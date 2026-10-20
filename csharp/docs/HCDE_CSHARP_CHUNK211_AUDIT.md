# Chunk 211 — DSDA OPTIONS defaults, option_list keys, parseOptions apply.

## Scope

DSDA OPTIONS defaults, option_list keys, parseOptions apply.

## Native ranges

- `src/gamedata/options.cpp` **dsda_options** 23–46 (24 lines)
- `src/gamedata/options.cpp` **option_list** 56–78 (23 lines)
- `src/gamedata/options.cpp` **parseOptions** 149–200 (52 lines)

**Credited native LOC:** 99 (tally `nativeLoc` 99).

## Managed

`VmDoomFontPlannerTests.cs`, `VmDsdaOptionsApplier.cs`, `VmDsdaOptionsApplierTests.cs`, `VmDsdaOptionsParser.cs`, `VmUMapInfoTokenParserTests.cs`, `VmVoxelSpinNameParser.cs`, `VmVoxelSpinNameParserTests.cs`, `VmXlatSimpleLineTranslate.cs`
Managed LOC (tally): 95.
Conversion commit: `bba9c7500564303331b84ee34f5daf85c999904a`.

## Tests

- `VmDsdaOptionsApplierTests.ApplyToLevel_SetsNoInfightingFlag`
- `VmDsdaOptionsParserTests.Parse_ClampedKeyValuePairs`
- `VmDsdaOptionsApplierTests.ApplyToLevel_SetsCompatMask`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 211 --to 211` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
