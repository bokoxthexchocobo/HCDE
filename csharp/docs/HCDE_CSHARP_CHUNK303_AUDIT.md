# Chunk 303 — HCDE_ParseCMPGNINF invasion/duel limits

## Scope

HCDE_ParseCMPGNINF invasion/duel limits

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **HCDE_ParseCMPGNINF** 2869–2943 (75 lines)

**Credited native LOC:** 75 (tally `nativeLoc` 75).

## Managed

`VmCmpgninfParser.cs`, `VmCmpgninfParserTests.cs`, `VmDehackedDehSuppAmmoNames.cs`, `VmDehackedDehSuppAmmoNamesTests.cs`, `VmDehackedIncludeDirectiveTests.cs`, `VmDehackedMbfParamEmittersTests.cs`, `VmDehackedPatchSpriteTableTests.cs`, `VmDehackedPatchTextTests.cs`, `VmEMapInfoPropertyTailTests.cs`
Managed LOC (tally): 58.
Conversion commit: `3bc60909bddc164448cb40ef18f757f90d5e384d`.

## Tests

- `VmCmpgninfParserTests.TryApplyProperty_ParsesWaveAndDuelLimits`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 303 --to 303` (0 hard / 0 soft).

## Seams

MAPINFO handlers omit live `FMapInfoParser` scanner state; managed code models option semantics only.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
