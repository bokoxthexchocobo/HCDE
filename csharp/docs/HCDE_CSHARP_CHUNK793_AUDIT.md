# Chunk 793 — `DDoor::Tick` and `DDoor::DoorSound` (`mapthinkers/a_doors.cpp` 80–343).

## Scope

`DDoor::Tick` and `DDoor::DoorSound` (`mapthinkers/a_doors.cpp` 80–343).

## Native ranges

- `src/playsim/mapthinkers/a_doors.cpp` **DDoor::Tick** 80–234 (155 lines)
- `src/playsim/mapthinkers/a_doors.cpp` **DDoor::DoorSound** 246–342 (97 lines)

**Credited native LOC:** 252 (tally `nativeLoc` 252).

## Managed

`VmADoorTickSound.cs`
Managed LOC (tally): 272.
Conversion commit: `58ac70d2aaedaaa34195b6eaeeeba898e2b065ae`.

## Padding review (2026-10-21)

| Managed | Native (`a_doors.cpp`) |
|---------|-------------------------|
| `Tick` | `DDoor::Tick` **80–234** |
| `DoorSound` | `DDoor::DoorSound` **246–342** |

### Out-of-body uncredit (793)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **252** |
| **Out-of-body removed** | **0** |
| **Honest** | **252** |


**Post-review ratio:** 272/252 managed/native ≈ 108% (src file LOC at HEAD; see tally).

## Tests

- `VmADoorTickSoundTests.Tick_WaitingDoorRaise_CountsDownThenCloses`
- `VmADoorTickSoundTests.Tick_OpenDirectionPastDest_DestroyThinker`
- `VmADoorTickSoundTests.DoorSound_StrifeDoorTexture_PicksStone`
- `VmADoorTickSoundTests.DoorSound_FastDoor_AddsChoiceOffset`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 793 --to 793` (0 hard / 0 soft).

## Seams

`MoveCeiling`, sound sequence helpers, lift `DPlat` detection, and `Net_DiagTraceDoorMove` are injected on `DoorTickContext`.

## Credit rationale

Faithful vertical-door wait/move/sound band before `Construct`/`EV_DoDoor` chunks.
