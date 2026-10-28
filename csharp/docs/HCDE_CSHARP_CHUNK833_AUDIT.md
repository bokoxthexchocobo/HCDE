# Chunk 833 — `DEarthquake::Construct`, `Serialize`, `Tick`, and `StaticGetQuakeIntensities` from `mapthinkers/a_quake.cpp` (47–417).

## Scope

`DEarthquake::Construct`, `Serialize`, `Tick`, and `StaticGetQuakeIntensities` from `mapthinkers/a_quake.cpp` (47–417).

## Native ranges

- `src/playsim/mapthinkers/a_quake.cpp` **DEarthquake::Serialize** 78–98 (21 lines)
- `src/playsim/mapthinkers/a_quake.cpp` **DEarthquake::Tick** 109–164 (56 lines)
- `src/playsim/mapthinkers/a_quake.cpp` **DEarthquake::StaticGetQuakeIntensities** 335–417 (83 lines)

**Credited native LOC:** 160 (tally `nativeLoc` 160).

## Managed

`VmAQuakeLifecycle.cs`
Managed LOC (tally): 209.
Conversion commit: `0680962ea7e0b57f4aa0fd0e0064a109ecdf7c2d`.

## Padding review (2026-10-21)

| Managed | Native (`a_quake.cpp`) |
|---------|------------------------|
| `Construct` | **47–70** |
| `Serialize` | **78–98** |
| `Tick` | **109–164** |
| `StaticGetQuakeIntensities` | **335–417** |

### Out-of-body uncredit (833)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **184** |
| **Out-of-body removed** | **24** |
| **Honest** | **160** |


**Post-review ratio:** 209/160 managed/native ≈ 131% (src file LOC at HEAD; see tally).

## Tests

- `VmAQuakeTests.Construct_SetsCountdownAndSpot`
- `VmAQuakeTests.Tick_NullSpot_Destroys`
- `VmAQuakeTests.StaticGetQuakeIntensities_CountsInRadius`
- `VmAQuakeTests.GetFalloff_InsideMinRange_ReturnsOne`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 833 --to 833` (0 hard / 0 soft).

## Seams

Sound, player roster, thing iterator, and thinker enumeration are injected; completes deferred band from **832**.

## Credit rationale

Whole-function credit for earthquake thinker lifecycle and combined tremor intensity pass.
