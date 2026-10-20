# Chunk 415 — EV strobe/light-on/glow, Strife teaser voices, DoomButtons table

## Scope

EV strobe/light-on/glow, Strife teaser voices, DoomButtons table

## Native ranges

- `src/playsim/mapthinkers/a_lights.cpp` **EV_StartLightStrobing** 570–596 (27 lines)
- `src/playsim/mapthinkers/a_lights.cpp` **EV_LightTurnOn** 635–669 (35 lines)
- `src/playsim/mapthinkers/a_lights.cpp` **EV_StartLightGlowing** 740–766 (27 lines)
- `src/d_main.cpp` **FindStrifeTeaserVoices** 3248–3281 (34 lines)
- `src/d_main.cpp` **DoomButtons** 3283–3316 (34 lines)

**Credited native LOC:** 157 (tally `nativeLoc` 157).

## Managed

`VmALightsEvAndRebornTests.cs`, `VmALightsEvLightTurnOn.cs`, `VmALightsEvStartLightGlowing.cs`, `VmALightsEvStrobe.cs`, `VmDMainDoomButtons.cs`, `VmDMainDoomButtonsTests.cs`, `VmDMainStrifeTeaserVoices.cs`, `VmDMainStrifeTeaserVoicesTests.cs`
Managed LOC (tally): 97.
Conversion commit: `d6ff4b42ba3a51aa566164a94e2a1e81d445a6dd`.

## Tests

- `VmALightsEvAndRebornTests.Strobe_SkipsLitSector`
- `VmALightsEvAndRebornTests.LightTurnOn_SearchesWhenBrightNegative`
- `VmALightsEvAndRebornTests.LightGlowing_SwapsBounds`
- `VmDMainStrifeTeaserVoicesTests.ShouldScanVoices_RequiresStrifeShareware`
- `VmDMainDoomButtonsTests.ButtonCount_MatchesNativeTable`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 415 --to 415` (0 hard / 0 soft).

## Seams

Sector iterators, filesystem namespaces, and bind registration are not hosted.

## Credit rationale

Credits `EV_StartLightStrobing`, `EV_LightTurnOn`, `EV_StartLightGlowing`, `FindStrifeTeaserVoices`, and the `DoomButtons` table from `a_lights.cpp` and `d_main.cpp`.
