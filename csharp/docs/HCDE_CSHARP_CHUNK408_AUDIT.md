# Chunk 408 — DFireFlicker, DFlicker construct, DLightFlash, DStrobe thinkers

## Scope

DFireFlicker, DFlicker construct, DLightFlash, DStrobe thinkers

## Native ranges

- `src/playsim/mapthinkers/a_lights.cpp` **DFireFlicker** 74–135 (62 lines)
- `src/playsim/mapthinkers/a_lights.cpp` **DFlicker::Construct** 160–167 (8 lines)
- `src/playsim/mapthinkers/a_lights.cpp` **DLightFlash** 194–237 (44 lines)
- `src/playsim/mapthinkers/a_lights.cpp` **DStrobe** 264–316 (53 lines)

**Credited native LOC:** 167 (tally `nativeLoc` 167).

## Managed

`VmALightsConstruct.cs`, `VmALightsFireFlicker.cs`, `VmALightsFlicker.cs`, `VmALightsLightFlash.cs`, `VmALightsStrobe.cs`, `VmALightsThinkerTests.cs`
Managed LOC (tally): 97.
Conversion commit: `ab286cf3a2364da8c1c0e1fed958f80beb4e3689`.

## Tests

- `VmALightsThinkerTests.ResolveLightLevel_ClampsToMin`
- `VmALightsThinkerTests.FlickerInitialCount_UsesPrMask`
- `VmALightsThinkerTests.NextCountWhenDark_MasksMinTime`
- `VmALightsThinkerTests.ResolveMinLightWhenEqual_Zeroes`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 408 --to 408` (0 hard / 0 soft).

## Seams

`sector_t`, `pr_*` RNG, and thinker allocation are not hosted.

## Credit rationale

Credits `DFireFlicker`, `DFlicker`, `DLightFlash`, and `DStrobe` logic from `a_lights.cpp` (bands avoid recognition-only `DFlicker::Tick`).
