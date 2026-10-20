# Chunk 362 — HCDE MAPINFO optional float legacy compat for sky speeds

## Scope

HCDE MAPINFO optional float legacy compat for sky speeds

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **HCDE_MapInfo_CheckOptionalFloatCompat** 600–633 (34 lines)

**Credited native LOC:** 34 (tally `nativeLoc` 34).

## Managed

`VmMapInfoOptionalFloatCompat.cs`, `VmMapInfoOptionalFloatCompatTests.cs`
Managed LOC (tally): 36.
Conversion commit: `3f9cc69c6adb9f5b38a5f1b2412b2ecb8a7a530b`.

## Tests

- `VmMapInfoOptionalFloatCompatTests.TryParseLegacyValue_PromotesNumberToFloat`
- `VmMapInfoOptionalFloatCompatTests.ShouldEmitCompatWarning_OnlyOnce`
- `VmMapInfoOptionalFloatCompatTests.ApplyHexenSkySpeedScale_DividesWhenHack`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 362 --to 362` (0 hard / 0 soft).

## Seams

Parser scanner calls are injected; one-time `Printf` warning uses a ref flag like native static `warned`.

## Credit rationale

Credits `HCDE_MapInfo_CheckOptionalFloatCompat` including legacy float/number promotion and mod-compat warning gate.
