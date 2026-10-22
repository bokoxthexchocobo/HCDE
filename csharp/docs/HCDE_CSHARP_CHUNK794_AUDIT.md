# Chunk 794 — `DDoor::Construct` overload and `FLevelLocals::EV_DoDoor` (`mapthinkers/a_doors.cpp` 355–538).

## Scope

`DDoor::Construct` overload and `FLevelLocals::EV_DoDoor` (`mapthinkers/a_doors.cpp` 355–538).

## Native ranges

- `src/playsim/mapthinkers/a_doors.cpp` **DDoor::Construct** 355–429 (75 lines)
- `src/playsim/mapthinkers/a_doors.cpp` **FLevelLocals::EV_DoDoor** 438–538 (101 lines)

**Credited native LOC:** 176 (tally `nativeLoc` 176).

## Managed

`VmADoorConstructEv.cs`
Managed LOC (tally): 259.
Conversion commit: `c19b4e7dfc179cc25b8a391dcf9a829f6f41be3d`.

## Padding review (2026-10-21)

| Managed | Native (`a_doors.cpp`) |
|---------|-------------------------|
| `Construct` | `DDoor::Construct` **355–429** |
| `EvDoDoor` | `FLevelLocals::EV_DoDoor` **438–538** |


**Post-review ratio:** 259/176 managed/native ≈ 147% (src file LOC at HEAD; see tally).

## Tests

- `VmADoorConstructEvTests.Construct_DoorRaise_SetsDirectionAndTopDist`
- `VmADoorConstructEvTests.Construct_DoorClose_ClearsLightTagWithCompat`
- `VmADoorConstructEvTests.Construct_DoorOpen_SkipsSoundWhenAlreadyAtTop`
- `VmADoorConstructEvTests.EvDoDoor_ManualTag_CreatesThinker`
- `VmADoorConstructEvTests.EvDoDoor_RaiseDoorReversal_ChangesDirection`
- `VmADoorConstructEvTests.EvDoDoor_LockFailsWithoutKeys`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 794 --to 794` (0 hard / 0 soft).

## Seams

`P_CheckKeys`, sector tag iteration, `CreateThinker`, height finders, and diag hooks are injected on construct/EV contexts; `DoorSound` reuses `VmADoorTickSound`.

## Credit rationale

Vertical-door spawn and EV band after tick/sound chunk793.
