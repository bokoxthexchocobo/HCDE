# Chunk 801 — `DElevator` construct/serialize/onDestroy and waggle thinkers plus `EV_StartWaggle` (`a_floor.cpp` 878–939, 1167–1353). Retired (2026-10-24): `--band` hull overlaps existing credit (p-floor-waggle-donut-elevator-chunk6 src/playsim/mapthinkers/a_floor.cpp 941-984).

## Scope

`DElevator` construct/serialize/onDestroy and waggle thinkers plus `EV_StartWaggle` (`a_floor.cpp` 878–939, 1167–1353). Retired (2026-10-24): `--band` hull overlaps existing credit (p-floor-waggle-donut-elevator-chunk6 src/playsim/mapthinkers/a_floor.cpp 941-984).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `f1dc926e45c4fd5d4a5ed0cc3dcd393de0ca22da`.

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

### Out-of-body uncredit (801)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **179** |
| **Out-of-body removed** | **0** |
| **Honest** | **179** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 801 --to 801` (0 hard / 0 soft).

## Seams

Interpolation refs, `P_ChangeSector`, midtex scroll, linked sectors, and tag iterators are injected.

## Credit rationale

Waggle whole functions (146 native) merged with adjacent elevator lifecycle (60) for honest 206 band per approved plan.
