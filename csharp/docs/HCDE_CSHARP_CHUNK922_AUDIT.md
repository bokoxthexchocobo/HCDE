# Chunk 922 — Waggle thinkers and `EV_StartWaggle` from `mapthinkers/a_floor.cpp` (**1163–1353**). Post–`EV_DoChange` (**1154**); avoids chunk **6** `DElevator::Tick` (**941–984**).

## Scope

Waggle thinkers and `EV_StartWaggle` from `mapthinkers/a_floor.cpp` (**1163–1353**). Post–`EV_DoChange` (**1154**); avoids chunk **6** `DElevator::Tick` (**941–984**).

## Native ranges

- `src/playsim/mapthinkers/a_floor.cpp` **DWaggleBase::Serialize** 1163–1178 (16 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DWaggleBase::DoWaggle** 1190–1270 (81 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DFloorWaggle** 1278–1306 (29 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **FLevelLocals::EV_StartWaggle** 1314–1353 (40 lines)

**Credited native LOC:** 166 (tally `nativeLoc` 166).

## Managed

`VmAFloorWaggleElevator.cs`
Managed LOC (tally): 229.
Conversion commit: `319749a6d5ef6e28bfb83434637795831a6e3b1a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `IMPLEMENT_CLASS` waggle types (**1163–1165**) | yes (manifest) |
| `DWaggleBase::Serialize` (**1167–1178**) | yes |
| `DWaggleBase::Construct` / `DoWaggle` (**1190–1270**) | yes |
| `DFloorWaggle` / `DCeilingWaggle` (**1278–1306**) | yes |
| `EV_StartWaggle` (**1314–1353**) | yes |

| Managed (`VmAFloorWaggleElevator.cs`) | Native (`a_floor.cpp`) |
|--------------------------------------|----------------------|
| `WaggleBaseSerialize` | **1167–1178** |
| `WaggleBaseConstruct` | **1190–1193** |
| `DoWaggle` | **1201–1270** |
| `FloorWaggleConstruct` / `FloorWaggleTick` | **1278–1288** |
| `CeilingWaggleConstruct` / `CeilingWaggleTick` | **1296–1306** |
| `EvStartWaggle` | **1314–1353** |
| Waggle state constants | **1186–1188** |

### Chunk **922** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **166** |
| **Net headline** | **+166** vs post-**921** |


**Post-review ratio:** 229/166 managed/native ≈ 138% (src file LOC at HEAD; see tally).

## Tests

- `VmAFloorWaggleElevatorTests.WaggleBaseSerialize_WritesFields`
- `VmAFloorWaggleElevatorTests.DoWaggle_ExpandReachesStable`
- `VmAFloorWaggleElevatorTests.DoWaggle_ReduceDestroysThinker`
- `VmAFloorWaggleElevatorTests.EvStartWaggle_Floor_SetsExpandState`
- `VmAFloorWaggleElevatorTests.EvStartWaggle_SkipsWhenPlaneMoving`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 922 --to 922` (0 hard / 0 soft).

## Seams

Tag iterators, thinker spawn, plane hooks, `BobSin`, scroll/linked-sector/sector-change, and interpolation are injected.

## Credit rationale

Tally orphan `VmAFloorWaggleElevator.cs` (retired **801**); **191** UNCredited lines in **1163–1353** band, **≥150** function-bounded.
