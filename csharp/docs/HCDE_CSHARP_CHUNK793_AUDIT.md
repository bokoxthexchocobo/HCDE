# Chunk 793 — `DDoor::Tick` and `DDoor::DoorSound` (`mapthinkers/a_doors.cpp` 80–343).

## Scope

`DDoor::Tick` and `DDoor::DoorSound` (`mapthinkers/a_doors.cpp` 80–343).

## Native ranges

- `src/playsim/mapthinkers/a_doors.cpp` **DDoor::Tick** 80–234 (155 lines)
- `src/playsim/mapthinkers/a_doors.cpp` **DDoor::DoorSound** 246–342 (97 lines)

**Credited native LOC:** 252 (tally `nativeLoc` 252).

## Managed

`VmADoorTickSound.cs`
Managed LOC (tally): 271.
Conversion commit: `4c7205e5ffbe676c1219dba30b3cf026066f81f6`.

## Padding review (2026-10-21)

| Managed | Native (`a_doors.cpp`) |
|---------|-------------------------|
| `Tick` | `DDoor::Tick` **80–234** |
| `DoorSound` | `DDoor::DoorSound` **246–342** |


**Post-review ratio:** 271/252 managed/native ≈ 108% (src file LOC at HEAD; see tally).

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
