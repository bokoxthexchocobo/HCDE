# Chunk 410 — DGlow, DGlow2, DPhased thinkers

## Scope

DGlow, DGlow2, DPhased thinkers

## Native ranges

- `src/playsim/mapthinkers/a_lights.cpp` **DGlow** 343–385 (43 lines)
- `src/playsim/mapthinkers/a_lights.cpp` **DGlow2** 411–447 (37 lines)
- `src/playsim/mapthinkers/a_lights.cpp` **DPhased** 471–544 (74 lines)

**Credited native LOC:** 154 (tally `nativeLoc` 154).

## Managed

`VmALightsGlow.cs`, `VmALightsGlow2.cs`, `VmALightsPhased.cs`, `VmALightsThinkerTests.cs`
Managed LOC (tally): 99.
Conversion commit: `949a0d17b050e5d22aa521fd0892bf5bd363facf`.

## Tests

- `VmALightsThinkerTests.GlowTick_ReversesAtMinLight`
- `VmALightsThinkerTests.Glow2Interpolates_MidCycle`
- `VmALightsThinkerTests.PhasedTick_RampsDuringFirstHalf`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 410 --to 410` (0 hard / 0 soft).

## Seams

Sector mutation, thinker allocation, and `PhaseHelper` graph walks are not hosted.

## Credit rationale

Credits `DGlow`, `DGlow2`, and `DPhased` tick/construct logic from `a_lights.cpp` (bands avoid recognition `EV_StartLightFlickering`).
