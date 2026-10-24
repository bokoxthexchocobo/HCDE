# Chunk 322 — MAPINFO inter exit enter picture options

## Scope

MAPINFO inter exit enter picture options

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **MAPINFO inter exit enter pics** 1375–1420 (46 lines)

**Credited native LOC:** 46 (tally `nativeLoc` 46).

## Managed

`VmMapInfoMapOptionsPics.cs`, `VmMapInfoMapOptionsPicsTests.cs`
Managed LOC (tally): 18.
Conversion commit: `7aaf320d20a8c826a93c8f41d3c1b51ed05ba4cd`.

## Tests

- `VmMapInfoMapOptionsPicsTests.ApplyInterPic_SetsLabel`
- `VmMapInfoMapOptionsPicsTests.ApplyExitPic_SetsNextMap`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 322 --to 322` (0 hard / 0 soft).

## Seams

MAPINFO handlers omit live `FMapInfoParser` scanner state; managed code models option semantics only.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
