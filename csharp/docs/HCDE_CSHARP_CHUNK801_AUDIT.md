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

Whole-function elevator lifecycle + waggle thinkers + `EV_StartWaggle` in one TU (`nativeLoc` **179**).

## Portfolio delta vs approved **206** plan (why first land showed **+127**, not **+206**)

The approved **206** plan counted **878–939** as one elevator band; chunk **801** credits **whole functions only**. **`DEFINE_FIELD` macros (899–903)** and the **925–939** comment/`DElevator::Tick` header are **not** function bodies (**27** plan lines).

| Effect | `nativeLoc` |
|--------|------------|
| Gross chunk **801** (after `DWaggleBase::Serialize` fix) | **+179** |
| Legacy **`p-floor-waggle-donut-elevator-chunk6`** drops **`EV_StartWaggle` 1314–1353** only | **−40** (chunk 6 gains nothing; `managedLoc` unchanged) |
| **Net `convertedBehaviorNative`** | **+139** (was **+127** when gross was **167** and **`DWaggleBase::Serialize` was missing (−12)**) |

**79-line gap** vs plan headline on first land (**206 − 127**): **40** overlap retirement **+ 27** non-function plan lines **+ 12** omitted serialize (**79 = 40 + 27 + 12**). Not a checksum/tally bug.

**Checksum:** `behaviorNativeChecksum` / `baselineNative` remain **508 086** (re-bucketing only).
