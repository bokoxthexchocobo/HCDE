# Chunk 795 — `DAnimatedDoor` thinker and `FLevelLocals::EV_SlidingDoor` (`mapthinkers/a_doors.cpp` 548–858).

## Scope

`DAnimatedDoor` thinker and `FLevelLocals::EV_SlidingDoor` (`mapthinkers/a_doors.cpp` 548–858).

## Native ranges

- `src/playsim/mapthinkers/a_doors.cpp` **DAnimatedDoor::Construct** 548–551 (4 lines)
- `src/playsim/mapthinkers/a_doors.cpp` **DAnimatedDoor::Serialize** 553–569 (17 lines)
- `src/playsim/mapthinkers/a_doors.cpp` **DAnimatedDoor::StartClosing** 577–603 (27 lines)
- `src/playsim/mapthinkers/a_doors.cpp` **DAnimatedDoor::Tick** 611–705 (95 lines)
- `src/playsim/mapthinkers/a_doors.cpp` **DAnimatedDoor::Construct** 713–770 (58 lines)
- `src/playsim/mapthinkers/a_doors.cpp` **FLevelLocals::EV_SlidingDoor** 779–858 (80 lines)

**Credited native LOC:** 281 (tally `nativeLoc` 281).

## Managed

`VmAAnimatedDoor.cs`
Managed LOC (tally): 368.
Conversion commit: `6cdbfac2c2bf28c5e81693d9ff016e9da86ad3f2`.

## Padding review (2026-10-21)

| Managed | Native (`a_doors.cpp`) |
|---------|-------------------------|
| `ConstructSectorOnly` | `DAnimatedDoor::Construct` **548–551** |
| `Serialize` | `DAnimatedDoor::Serialize` **553–569** |
| `StartClosing` | `DAnimatedDoor::StartClosing` **577–603** |
| `Tick` | `DAnimatedDoor::Tick` **611–705** |
| `Construct` | `DAnimatedDoor::Construct` **713–770** |
| `EvSlidingDoor` | `FLevelLocals::EV_SlidingDoor` **779–858** |

### Out-of-body uncredit (795)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **281** |
| **Out-of-body removed** | **0** |
| **Honest** | **281** |


**Post-review ratio:** 368/281 managed/native ≈ 131% (src file LOC at HEAD; see tally).

## Tests

- `VmAAnimatedDoorTests.Tick_OpeningFinishesWaitingWhenDelaySet`
- `VmAAnimatedDoorTests.Serialize_CopiesThinkerFields`
- `VmAAnimatedDoorTests.StartClosing_BlockedWhenSectorTouching`
- `VmAAnimatedDoorTests.Tick_ClosingFinishesClearsBlocking`
- `VmAAnimatedDoorTests.EvSlidingDoor_Manual_CreatesWhenAnimFound`
- `VmAAnimatedDoorTests.EvSlidingDoor_Tag_IteratesSectors`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 795 --to 795` (0 hard / 0 soft).

## Seams

`TexAnim::FindAnimatedDoor`, `CreateThinker`, sector tag iteration, and `MoveCeiling` are injected; texture init uses display-height seam.

## Credit rationale

Sliding-door animation band after vertical-door EV chunk794.
