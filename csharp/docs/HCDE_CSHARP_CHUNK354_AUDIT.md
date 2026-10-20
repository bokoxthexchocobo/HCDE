# Chunk 354 — Strife/Raven palettes and AM_initColors preset routing

## Scope

Strife/Raven palettes and AM_initColors preset routing

## Native ranges

- `src/am_map.cpp` **StrifeColors table** 628–655 (28 lines)
- `src/am_map.cpp` **RavenColors table** 657–684 (28 lines)
- `src/am_map.cpp` **AM_ClearColorsets** 693–697 (5 lines)
- `src/am_map.cpp` **AM_initColors** 705–759 (55 lines)

**Credited native LOC:** 116 (tally `nativeLoc` 116).

## Managed

`VmAutomapColorsetInit.cs`, `VmAutomapColorsetInitTests.cs`
Managed LOC (tally): 144.
Conversion commit: `a47fdff47fb17eefc961283a69d4549b4a049603`.

## Tests

- `VmAutomapColorsetInitTests.StrifePalette_MatchesNativeBackgroundTriplet`
- `VmAutomapColorsetInitTests.PredefinedUsesRavenGrid_OnlyForSetThree`
- `VmAutomapColorsetInitTests.ClearModColorsets_ClearsBothFlags`
- `VmAutomapColorsetInitTests.PlanInit_PicksStrifePalette`
- `VmAutomapColorsetInitTests.PlanInit_OverlayUsesCustomModWhenDefined`
- `VmAutomapColorsetInitTests.ResolvePresetColorset_StrifeWhenAuto`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 354 --to 354` (0 hard / 0 soft).

## Seams

CVAR `initFromCVars` and live `AMColors` globals are not hosted; managed code mirrors preset selection, mod overlay routing, and static palette bytes.

## Credit rationale

Credits `StrifeColors`/`RavenColors` tables, `AM_ClearColorsets`, and the `AM_initColors` overlay/custom/preset switch through Raven/Doom/Strife `initFromColors` arms.
