# Chunk 218 — ParseStandardProperty subset for UMAPINFO.

## Scope

ParseStandardProperty subset for UMAPINFO.

## Native ranges

- `src/gamedata/umapinfo.cpp` **ParseStandardProperty** 165–222 (58 lines)
- `src/gamedata/umapinfo.cpp` **ParseStandardProperty partime** 266–269 (4 lines)

**Credited native LOC:** 62 (tally `nativeLoc` 62).

## Managed

`VmUMapInfoStandardPropertyParser.cs`, `VmUMapInfoStandardPropertyParserTests.cs`
Managed LOC (tally): 62.
Conversion commit: `28880ce49ed36f1e722148c2c5706801d035ea5b`.

## Tests

- `VmUMapInfoStandardPropertyParserTests.TryApplyProperty_SetsLevelName`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 218 --to 218` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
