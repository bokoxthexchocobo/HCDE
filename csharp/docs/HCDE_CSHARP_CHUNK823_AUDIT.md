# Chunk 823 — `FDynamicLight` release, activate, tick, and location update from `a_dynlight.cpp` (196–417).

## Scope

`FDynamicLight` release, activate, tick, and location update from `a_dynlight.cpp` (196–417).

## Native ranges

- `src/playsim/a_dynlight.cpp` **FDynamicLight::ReleaseLight** 196–205 (10 lines)
- `src/playsim/a_dynlight.cpp` **FDynamicLight::Activate** 213–231 (19 lines)
- `src/playsim/a_dynlight.cpp` **FDynamicLight::Tick** 239–352 (114 lines)
- `src/playsim/a_dynlight.cpp` **FDynamicLight::UpdateLocation** 362–417 (56 lines)

**Credited native LOC:** 199 (tally `nativeLoc` 199).

## Managed

`VmADynLightTick.cs`
Managed LOC (tally): 158.
Conversion commit: `91b8f72bc7cc0ae58f63c8f308bbf700854399ca`.

## Padding review (2026-10-21)

| Managed | Native (`a_dynlight.cpp`) |
|---------|---------------------------|
| `ReleaseLight` | **196–205** |
| `Activate` | **213–231** |
| `Tick` | **239–352** |
| `UpdateLocation` | **362–417** |

**Post-review ratio:** 158/199 managed/native ≈ 79% (src file LOC at HEAD; see tally).

## Tests

- `VmADynLightTickTests.ReleaseLight_InvokesPoolReturn`
- `VmADynLightTickTests.Activate_PointLightSetsRadius`
- `VmADynLightTickTests.Tick_NoTarget_Releases`
- `VmADynLightTickTests.Tick_FlickerLight_PicksIntensity`
- `VmADynLightTickTests.UpdateLocation_RelinksWhenActive`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 823 --to 823` (0 hard / 0 soft).

## Seams

Level light lists, `FCycler`, sector light levels, and actor visibility are injected.

## Credit rationale

First tally credit for dynamic light lifetime and per-tic radius animation.
