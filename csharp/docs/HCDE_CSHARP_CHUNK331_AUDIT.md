# Chunk 331 — ParseMapDefinition MapFlagHandlers switch MITYPE apply paths

## Scope

ParseMapDefinition MapFlagHandlers switch MITYPE apply paths

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **ParseMapDefinition flag switch** 2148–2285 (138 lines)

**Credited native LOC:** 138 (tally `nativeLoc` 138).

## Managed

`VmMapInfoFlagApply.cs`, `VmMapInfoFlagApplyTests.cs`
Managed LOC (tally): 55.
Conversion commit: `0465a296c165af1d871e8d49ec2223e87ad58f9f`.

## Tests

- `VmMapInfoFlagApplyTests.ApplyScFlags2_MergesTeamplayOn`
- `VmMapInfoFlagApplyTests.ApplySetFlag2_TogglesWithAssign`
- `VmMapInfoFlagApplyTests.ApplyClearCompat_SetsMask`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 331 --to 331` (0 hard / 0 soft).

## Seams

`FMapInfoParser` scanner, `AutoSegs::MapInfoOptions`, and unknown-property recovery are not hosted; switch arms are pure flag-word mutations on `VmMapInfoLevelFlags`.

## Credit rationale

Covers `g_mapinfo.cpp` 2148–2285: the `MITYPE_CLRFLAG` through `MITYPE_COMPATFLAG` arms inside `ParseMapDefinition`, including optional-assignment toggles for new-format compat flags and the early exit into custom `MapInfoOptions` dispatch when no handler matches.
