# Chunk 801 — `DElevator` construct/serialize/onDestroy and waggle thinkers plus `EV_StartWaggle` (`a_floor.cpp` 878–939, 1167–1353).

## Scope

`DElevator` construct/serialize/onDestroy and waggle thinkers plus `EV_StartWaggle` (`a_floor.cpp` 878–939, 1167–1353).

## Native ranges

- `src/playsim/mapthinkers/a_floor.cpp` **DElevator::Construct** 878–885 (8 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DElevator::Serialize** 887–897 (11 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DElevator::OnDestroy** 911–924 (14 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DWaggleBase::Serialize** 1167–1178 (12 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DWaggleBase::Construct** 1190–1193 (4 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DWaggleBase::DoWaggle** 1201–1270 (70 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DFloorWaggle::Construct** 1278–1283 (6 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DFloorWaggle::Tick** 1285–1288 (4 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DCeilingWaggle::Construct** 1296–1301 (6 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DCeilingWaggle::Tick** 1303–1306 (4 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **FLevelLocals::EV_StartWaggle** 1314–1353 (40 lines)

**Credited native LOC:** 179 (tally `nativeLoc` 179).

## Managed

`VmAFloorWaggleElevator.cs`
Managed LOC (tally): 229.
Conversion commit: `6d3643b287434957016e2f41a2759e8e8c3d816f`.

## Padding review (2026-10-21)

| Managed | Native (`a_floor.cpp`) |
|---------|----------------------|
| `ElevatorConstruct` | `DElevator::Construct` **878–885** |
| `ElevatorSerialize` | `DElevator::Serialize` **887–897** |
| `ElevatorOnDestroy` | `DElevator::OnDestroy` **911–924** |
| `WaggleBaseSerialize` | `DWaggleBase::Serialize` **1167–1178** |
| `WaggleBaseConstruct` | `DWaggleBase::Construct` **1190–1193** |
| `DoWaggle` | `DWaggleBase::DoWaggle` **1201–1270** |
| `FloorWaggleConstruct` / `FloorWaggleTick` | `DFloorWaggle::Construct` **1278–1283**, `Tick` **1285–1288** |
| `CeilingWaggleConstruct` / `CeilingWaggleTick` | `DCeilingWaggle::Construct` **1296–1301**, `Tick` **1303–1306** |
| `EvStartWaggle` | `FLevelLocals::EV_StartWaggle` **1314–1353** |


**Post-review ratio:** 229/179 managed/native ≈ 128% (src file LOC at HEAD; see tally).

## Tests

- `VmAFloorWaggleElevatorTests.ElevatorOnDestroy_DropsInterpRefs`
- `VmAFloorWaggleElevatorTests.ElevatorSerialize_WritesFields`
- `VmAFloorWaggleElevatorTests.WaggleBaseSerialize_WritesFields`
- `VmAFloorWaggleElevatorTests.DoWaggle_ExpandReachesStable`
- `VmAFloorWaggleElevatorTests.DoWaggle_ReduceDestroysThinker`
- `VmAFloorWaggleElevatorTests.EvStartWaggle_Floor_SetsExpandState`
- `VmAFloorWaggleElevatorTests.EvStartWaggle_SkipsWhenPlaneMoving`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 801 --to 801` (0 hard / 0 soft).

## Seams

Interpolation refs, `P_ChangeSector`, midtex scroll, linked sectors, and tag iterators are injected.

## Credit rationale

Waggle whole functions (146 native) merged with adjacent elevator lifecycle (60) for honest 206 band per approved plan.
