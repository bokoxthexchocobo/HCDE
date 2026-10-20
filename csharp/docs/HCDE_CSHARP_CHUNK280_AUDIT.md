# Chunk 280 — MAPINFO registered option dispatch (re-audit 265-286: tested ranges only)

## Scope

MAPINFO registered option dispatch (re-audit 265-286: tested ranges only)

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **ParseMapDefinition options** 2271–2420 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 3).

## Managed

`VmMapInfoOptionDispatch.cs`, `VmMapInfoOptionDispatchTests.cs`
Managed LOC (tally): 42.
Conversion commit: `d39999445b3aef31f5ecbb216392d96247b7562e`.

## Tests

- `VmMapInfoOptionDispatchTests.TryDispatchOption_MatchesName`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 280 --to 280` (0 hard / 0 soft).

## Seams

MAPINFO handlers omit live `FMapInfoParser` scanner state; managed code models option semantics only.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
