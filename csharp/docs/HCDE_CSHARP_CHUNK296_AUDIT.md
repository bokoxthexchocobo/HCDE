# Chunk 296 — defaultenvironment ID pack and compat handler tail rows

## Scope

defaultenvironment ID pack and compat handler tail rows

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **teamdamage through hazardflash** 1567–1653 (87 lines)
- `src/gamedata/g_mapinfo.cpp` **intro and outro map options** 1905–1945 (41 lines)
- `src/gamedata/g_mapinfo.cpp` **MapFlagHandlers compat and CD tracks** 2096–2120 (25 lines)

**Credited native LOC:** 153 (tally `nativeLoc` 9).

## Managed

`VmMapInfoFlagHandlers.cs`, `VmMapInfoFlagHandlersCompat.cs`, `VmMapInfoFlagHandlersCompatTests.cs`
Managed LOC (tally): 28.
Conversion commit: `37b0385b15ffc2670453755078d40a2f8f5e0173`.

## Tests

- `VmMapInfoScalarOptionsBlockTests.PackDefaultEnvironment_CombinesBytes`
- `VmMapInfoFlagHandlersCompatTests.TryGetCompatHandler_FindsNoId24`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 296 --to 296` (0 hard / 0 soft).

## Seams

MAPINFO handlers omit live `FMapInfoParser` scanner state; managed code models option semantics only.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
