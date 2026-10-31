# Chunk 1164 — Flicker, flash, strobe, glow, and phased light thinkers from `a_lights.cpp` (**159–551**).

## Scope

Flicker, flash, strobe, glow, and phased light thinkers from `a_lights.cpp` (**159–551**).

## Native ranges

- `src/playsim/mapthinkers/a_lights.cpp` **Light thinkers** 159–551 (393 lines)

**Credited native LOC:** 393 (tally `nativeLoc` 393).

## Managed

`VmALightsThinkers1164Slice.cs`
Managed LOC (tally): 348.
Conversion commit: `72f42a0087cc66e035017dae0fb54de7c369dc65`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `DFlicker::Construct` (**160–167**) | yes |
| `DLightFlash::Tick` and constructors (**194–237**) | yes |
| `DStrobe::Tick` and constructors (**264–316**) | yes |
| `DGlow::Tick` and constructor (**343–385**) | yes |
| `DGlow2::Tick` and constructor (**411–447**) | yes |
| `DPhased::Tick`, `PhaseHelper`, `Propagate`, and constructor (**471–544**) | yes |
| Band hull (**159–551**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **136–158** | `DFlicker::Tick` is already recognition credit |
| **552–560** | `EV_StartLightFlickering` is already recognition credit |
| `FindMinSurroundingLight` | the neighbor minimum stays host-supplied |
| `P_NextSpecialSector` | the next sequence sector stays host-supplied |

`ClampLight` limits a level to the signed **16**-bit range. `SetLightLevel` stores that clamped value. A flicker constructor writes the clamped maximum straight into the sector light. Its count is `(random & 64) + 1`.

A light flash built from the sector keeps that light and the supplied neighbor minimum, with max time **64** and min time **7**. A flash built from arguments clamps both arguments. The count starts as `(random & 64) + 1`. Each tick decrements first. At zero, a sector already at the maximum drops to the minimum and takes `(random & minTime) + 1`. Any other level rises to the maximum and takes `(random & maxTime) + 1`.

A Hexen strobe clamps the upper and lower lights, stores the bright and dark durations, and starts at count **1**. A Doom strobe keeps the sector light, uses the neighbor minimum, and forces that minimum to **0** when the two lights match. Sync starts at **1**. An unsynced start is `(random & 7) + 1`. At zero, a sector at the minimum becomes the maximum for the bright duration. Any other level becomes the minimum for the dark duration.

Glow speed is **8**. Down subtracts it and, on reaching the minimum, adds it back and turns up. Up adds it and, on reaching the maximum, subtracts it and turns down. Any other direction leaves the level and still stores it through `SetLightLevel`. A glow constructed from a sector uses the neighbor minimum, the sector light, and direction **-1**.

Glow2 clamps the ends, stores the duration, and starts the counter at **-1**. The comparison uses the current counter and then increments it. A counter that has reached the duration either sets the end and destroys a one-shot, or swaps the ends and subtracts the duration. The stored level is `((end - start) * tics) / maxTics + start`, with integer division toward zero.

A phased base and phase are stored in **8** bits. Below step **12** the level rises from the base. Below step **24** it falls. After that it stays at the base. Phase **0** becomes **63**. Any other phase decreases by one. `Propagate` increments the valid count and walks from the origin. A missing or already stamped sector returns the index. A zero light uses the incoming base. Index **0** updates the origin. Later indexes spawn a thinker whose phase starts at **0** and is then replaced. Special **3** searches for special **4**. Every other special searches for special **3**. The phase is `((steps - index - 1) * 64) / steps`, and the sector special becomes **0**.

| Managed (`VmALightsThinkers1164Slice.cs`) | Native (`a_lights.cpp`) |
|-------------------------------------------|--------------------------|
| Light thinkers | **159–551** |
| Band hull (exclusive slice) | **159–551** |

### Chunk **1164** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **393** |
| **Net headline** | **+393** vs post-**1163** |


**Post-review ratio:** 348/393 managed/native ≈ 89% (src file LOC at HEAD; see tally).

## Tests

- `VmALightsThinkers1164SliceTests.FlickerAndFlash_ClampArgsAndSwapWhenTheCountHitsZero`
- `VmALightsThinkers1164SliceTests.Strobe_StaysInSyncForHexenAndZerosAFlatDoomLight`
- `VmALightsThinkers1164SliceTests.Glow_TurnsAtTheEdgeAndInterpolatesAfterThePostIncrement`
- `VmALightsThinkers1164SliceTests.Phase_ChainsSpecialsAndWrapsFromZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1164 --to 1164` (0 hard / 0 soft).

## Seams

Neighbor minimum light and the next light-sequence sector stay host-supplied. This slice clamps, steps, and phases the thinkers.

## Credit rationale

UNCredited **`a_lights.cpp`** **159–551** (+393). Recognition credit ends at **158** and resumes at **552**. A full tally line scan shows no overlap.
