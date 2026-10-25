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
Conversion commit: `664ead53c1dd63d87e565782d343fe4b5d2c89e7`.

## Padding review (2026-10-21)

| Managed | Native (`a_dynlight.cpp`) |
|---------|---------------------------|
| `ReleaseLight` | **196–205** |
| `Activate` | **213–231** |
| `Tick` | **239–352** |
| `UpdateLocation` | **362–417** |

### Out-of-body uncredit (823)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **199** |
| **Out-of-body removed** | **0** |
| **Honest** | **199** |


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
