# Chunk 829 — Hexen lightning thinker and level hooks from `mapthinkers/a_lightning.cpp` (46–316).

## Scope

Hexen lightning thinker and level hooks from `mapthinkers/a_lightning.cpp` (46–316).

## Native ranges

- `src/playsim/mapthinkers/a_lightning.cpp` **DLightningThinker::Construct** 46–55 (10 lines)
- `src/playsim/mapthinkers/a_lightning.cpp` **DLightningThinker::Serialize** 67–75 (9 lines)
- `src/playsim/mapthinkers/a_lightning.cpp` **DLightningThinker::Tick** 83–94 (12 lines)
- `src/playsim/mapthinkers/a_lightning.cpp` **DLightningThinker::LightningFlash** 102–210 (109 lines)
- `src/playsim/mapthinkers/a_lightning.cpp` **DLightningThinker::ForceLightning** 218–235 (18 lines)
- `src/playsim/mapthinkers/a_lightning.cpp` **FLevelLocals::StartLightning** 255–288 (34 lines)
- `src/playsim/mapthinkers/a_lightning.cpp` **FLevelLocals::ForceLightning** 296–307 (12 lines)
- `src/playsim/mapthinkers/a_lightning.cpp` **DEFINE_ACTION_FUNCTION(FLevelLocals, ForceLightning)** 309–316 (8 lines)

**Credited native LOC:** 212 (tally `nativeLoc` 212).

## Managed

`VmALightning.cs`
Managed LOC (tally): 210.
Conversion commit: `bd4c77407224d8ec5c0ca452727f3ade74792a80`.

## Padding review (2026-10-21)

| Managed | Native (`a_lightning.cpp`) |
|---------|---------------------------|
| `Construct` | **46–55** |
| `Serialize` | **67–75** |
| `Tick` | **83–94** |
| `LightningFlash` | **102–210** |
| `ForceLightning` (thinker) | **218–235** |
| `StartLightning` | **255–288** |
| `ForceLightning` (level) | **296–307** |
| VM wrapper | **309–316** |

**Post-review ratio:** 210/212 managed/native ≈ 99% (src file LOC at HEAD; see tally).

## Tests

- `VmALightningTests.Construct_SetsNextFlashFromRandom`
- `VmALightningTests.Tick_DecrementsCountdownWhenIdle`
- `VmALightningTests.LightningFlash_RestoreClearsSwapSkies`
- `VmALightningTests.LightningFlash_StartPhase_SetsFlashCountAndSwapSkies`
- `VmALightningTests.ForceLightningLevel_CreatesWhenMissing`
- `VmALightningTests.StartLightning_HexenNoSky_ClearsStartFlag`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 829 --to 829` (0 hard / 0 soft).

## Seams

Sector light levels, sky texture id, `S_Sound`, script/event hooks, and thinker factory are injected.

## Credit rationale

First tally credit for `DLightningThinker` flash cycle and level `StartLightning` / `ForceLightning`.
