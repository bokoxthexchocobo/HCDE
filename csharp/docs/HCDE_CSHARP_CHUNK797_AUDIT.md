# Chunk 797 — `DCeiling::PlayCeilingSound`, `Tick`, and ceiling EV helpers (`mapthinkers/a_ceiling.cpp` 97–117, 125–215, 506–621).

## Scope

`DCeiling::PlayCeilingSound`, `Tick`, and ceiling EV helpers (`mapthinkers/a_ceiling.cpp` 97–117, 125–215, 506–621).

## Native ranges

- `src/playsim/mapthinkers/a_ceiling.cpp` **DCeiling::PlayCeilingSound** 97–117 (21 lines)
- `src/playsim/mapthinkers/a_ceiling.cpp` **DCeiling::Tick** 125–215 (91 lines)
- `src/playsim/mapthinkers/a_ceiling.cpp` **FLevelLocals::ActivateInStasisCeiling** 552–568 (17 lines)
- `src/playsim/mapthinkers/a_ceiling.cpp` **FLevelLocals::EV_CeilingCrushStop** 578–605 (28 lines)
- `src/playsim/mapthinkers/a_ceiling.cpp` **FLevelLocals::EV_StopCeiling** 607–621 (15 lines)

**Credited native LOC:** 172 (tally `nativeLoc` 172).

## Managed

`VmACeilingTickEv.cs`
Managed LOC (tally): 265.
Conversion commit: `eb2ae43c02e250b612e576bd31f6606a1402c07c`.

## Padding review (2026-10-21)

| Managed | Native (`a_ceiling.cpp`) |
|---------|-------------------------|
| `PlayCeilingSound` / `PlayCeilingSoundSector` | `DCeiling::PlayCeilingSound` **97–117** |
| `Tick` | `DCeiling::Tick` **125–215** |
| `EvDoCeiling` | `FLevelLocals::EV_DoCeiling` **506–551** |
| `ActivateInStasisCeiling` | `FLevelLocals::ActivateInStasisCeiling` **552–577** |
| `EvCeilingCrushStop` | `FLevelLocals::EV_CeilingCrushStop` **578–606** |
| `EvStopCeiling` | `FLevelLocals::EV_StopCeiling` **607–621** |

### Out-of-body uncredit (797)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **228** |
| **Out-of-body removed** | **56** |
| **Honest** | **172** |


**Post-review ratio:** 265/172 managed/native ≈ 154% (src file LOC at HEAD; see tally).

## Tests

- `VmACeilingTickEvTests.PlayCeilingSound_DefaultPicksCeilingNormal`
- `VmACeilingTickEvTests.Tick_UpPastDest_DestroyThinker`
- `VmACeilingTickEvTests.Tick_DownCrushSlowdown_ReducesSpeed`
- `VmACeilingTickEvTests.ActivateInStasisCeiling_RestoresDirection`
- `VmACeilingTickEvTests.EvCeilingCrushStop_PutsCrusherInStasis`
- `VmACeilingTickEvTests.EvStopCeiling_DestroyThinkerOnTaggedSector`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 797 --to 797` (0 hard / 0 soft).

## Seams

`MoveCeiling`, sequence helpers, thinker iterators, and tag iterators are injected on tick/EV contexts.

## Credit rationale

Ceiling runtime band paired with `CreateCeiling` chunk796.
