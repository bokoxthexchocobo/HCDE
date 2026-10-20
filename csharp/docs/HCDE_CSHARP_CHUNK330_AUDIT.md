# Chunk 330 — MapFlagHandlers falling damage through compat_nofriendlyspawn rows

## Scope

MapFlagHandlers falling damage through compat_nofriendlyspawn rows

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **MapFlagHandlers mid table** 1980–2095 (116 lines)

**Credited native LOC:** 116 (tally `nativeLoc` 116).

## Managed

`VmMapInfoFlagHandlers.cs`, `VmMapInfoFlagHandlersMidTests.cs`
Managed LOC (tally): 35.
Conversion commit: `9cd1c34f4eef953957cb97227a3ea12ea4b27c0b`.

## Tests

- `VmMapInfoFlagHandlersMidTests.TryGetHandler_FindsStrifeFallingDamage`
- `VmMapInfoFlagHandlersMidTests.TryGetHandler_FindsCompatNoid24`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 330 --to 330` (0 hard / 0 soft).

## Seams

Handler rows keep placeholder `LEVEL_*`/`COMPATF_*` bit values; only catalog lookup and flag-apply helpers are modeled.

## Credit rationale

Credits the uncredited `MapFlagHandlers[]` continuation 1980–2095 between chunk 329's `nojump` row and the CD-track `MITYPE_EATNEXT` rows on chunk 296: falling-damage variants, inventory/teamplay/checkswitch flags, LEVEL3 rendering toggles, and the compat-flag tail through `compat_nofriendlyspawn`.
