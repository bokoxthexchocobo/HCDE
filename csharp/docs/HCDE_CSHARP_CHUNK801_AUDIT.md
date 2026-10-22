# Chunk 801 — `DElevator` construct/serialize/onDestroy and waggle thinkers plus `EV_StartWaggle` (`a_floor.cpp` 878–939, 1167–1353).

## Scope

`DElevator` construct/serialize/onDestroy and waggle thinkers plus `EV_StartWaggle` (`a_floor.cpp` 878–939, 1167–1353).

## Native ranges

- `src/playsim/mapthinkers/a_floor.cpp` **DElevator::Construct** 878–885 (8 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DElevator::Serialize** 887–897 (11 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DElevator::OnDestroy** 911–924 (14 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DWaggleBase::Construct** 1190–1193 (4 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DWaggleBase::DoWaggle** 1201–1270 (70 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DFloorWaggle::Construct** 1278–1283 (6 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DFloorWaggle::Tick** 1285–1288 (4 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DCeilingWaggle::Construct** 1296–1301 (6 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DCeilingWaggle::Tick** 1303–1306 (4 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **FLevelLocals::EV_StartWaggle** 1314–1353 (40 lines)

**Credited native LOC:** 167 (tally `nativeLoc` 167).

## Managed

`VmAFloorWaggleElevator.cs`
Managed LOC (tally): 217.
Conversion commit: `6d3643b287434957016e2f41a2759e8e8c3d816f`.

## Padding review (2026-10-21)

| Managed | Native (`a_floor.cpp`) |
|---------|----------------------|
| `ElevatorConstruct` | `DElevator::Construct` **878–885** |
| `ElevatorOnDestroy` | `DElevator::OnDestroy` **911–924** |
| `WaggleBaseConstruct` | `DWaggleBase::Construct` **1190–1193** |
| `DoWaggle` | `DWaggleBase::DoWaggle` **1201–1270** |
| `FloorWaggleConstruct` / `FloorWaggleTick` | `DFloorWaggle::Construct` **1278–1283**, `Tick` **1285–1288** |
| `CeilingWaggleConstruct` / `CeilingWaggleTick` | `DCeilingWaggle::Construct` **1296–1301**, `Tick` **1303–1306** |
| `EvStartWaggle` | `FLevelLocals::EV_StartWaggle` **1314–1353** |


**Post-review ratio:** 217/167 managed/native ≈ 130% (src file LOC at HEAD; see tally).

## Tests

- `VmAFloorWaggleElevatorTests.ElevatorOnDestroy_DropsInterpRefs`
- `VmAFloorWaggleElevatorTests.ElevatorSerialize_WritesFields`
- `VmAFloorWaggleElevatorTests.DoWaggle_ExpandReachesStable`
- `VmAFloorWaggleElevatorTests.DoWaggle_ReduceDestroysThinker`
- `VmAFloorWaggleElevatorTests.EvStartWaggle_Floor_SetsExpandState`
- `VmAFloorWaggleElevatorTests.EvStartWaggle_SkipsWhenPlaneMoving`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 801 --to 801` (0 hard / 0 soft).

## Seams

Interpolation refs, `P_ChangeSector`, midtex scroll, linked sectors, and tag iterators are injected.

## Credit rationale

Waggle whole functions plus adjacent elevator lifecycle in one TU; see portfolio delta below.

## Portfolio delta vs approved **206** plan (why **+127**, not **+206**)

Approved plan **206** mixed whole-function spans with adjacent non-function lines (`DEFINE_FIELD` **899–903**, comment/header tail **925–939**). Chunk **801** credits **whole functions only** (standing rules).

| Component | Plan LOC | Credited LOC (whole functions) | Notes |
|-----------|---------|-------------------------------|--------|
| `DElevator` construct / serialize / onDestroy | **60** (878–939) | **33** (878–885, 887–897, 911–924) | **27** plan lines are `DEFINE_FIELD` macros and post-`OnDestroy` headers, not credited bodies |
| Waggle + `EV_StartWaggle` | **146** | **146** (1167–1178, 1190–1193, 1201–1270, …, 1314–1353) | `DWaggleBase::Serialize` **1167–1178** added in follow-up fix (was omitted on first land) |
| **Gross chunk 801** | **206** | **179** after serialize fix | **27** plan inflation; not a tally miscount |

**Overlap (old row loses credit only):** legacy **`p-floor-waggle-donut-elevator-chunk6`** dropped **`EV_StartWaggle` 1314–1353** (**40** native) when chunk **801** took that whole function. Chunk 6 **`managedLoc` unchanged**; **`nativeLoc` 84 → 44**.

**Net `convertedBehaviorNative` at 801:** **+179 − 40 = +139** after serialize fix (was **+167 − 40 = +127** on first land). The **79** gap vs plan headline (**206 − 127**) is **40** overlap retirement **+ 27** non-function plan lines **+ 12** missing `DWaggleBase::Serialize` on first land (**79 = 40 + 27 + 12**).

**Checksum:** `behaviorNativeChecksum` / `baselineNative` stay **508 086** (lines move between buckets; total native budget unchanged).
