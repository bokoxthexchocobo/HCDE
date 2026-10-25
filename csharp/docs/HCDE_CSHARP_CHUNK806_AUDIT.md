# Chunk 806 — Generic door/floor, door wait, texture floors, waggle/transfer/donut, pillars (`p_lnspec.cpp` 278–289, 305–334, 336–364, 415–419, 472–483, 502–573, 652–669). Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

Generic door/floor, door wait, texture floors, waggle/transfer/donut, pillars (`p_lnspec.cpp` 278–289, 305–334, 336–364, 415–419, 472–483, 502–573, 652–669). Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `c0933a71551aebc0007eb46193806aceb4b1c7b7`.

## Padding review (2026-10-21)

| Managed | Native (`p_lnspec.cpp`) |
|---------|----------------------|
| `DoorWaitRaise` | `LS_*` **278–283** |
| `DoorWaitClose` | `LS_*` **284–289** |
| `GenericDoor` | `LS_*` **305–334** |
| `FloorLowerByValue` | `LS_*` **336–340** |
| `FloorLowerToLowest` | `LS_*` **342–346** |
| `FloorLowerToHighest` | `LS_*` **348–352** |
| `FloorLowerToNearest` | `LS_*` **360–364** |
| `FloorCrushStop` | `LS_*` **415–419** |
| `FloorRaiseByTexture` | `LS_*` **472–477** |
| `FloorLowerByTexture` | `LS_*` **478–483** |
| `FloorWaggle` | `LS_*` **502–506** |
| `CeilingWaggle` | `LS_*` **508–512** |
| `FloorTransferTrigger` | `LS_*` **514–518** |
| `FloorTransferNumeric` | `LS_*` **520–524** |
| `FloorDonut` | `LS_*` **526–530** |
| `GenericFloor` | `LS_*` **532–567** |
| `FloorStop` | `LS_*` **569–573** |
| `PillarBuild` | `LS_*` **652–657** |
| `PillarBuildAndCrush` | `LS_*` **658–663** |
| `PillarOpen` | `LS_*` **664–669** |

### Out-of-body uncredit (806)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **163** |
| **Out-of-body removed** | **163** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 806 --to 806` (0 hard / 0 soft).

## Seams

`EV_DoDoor`, `EV_DoFloor`, `EV_DoPillar`, `EV_StartWaggle`, `EV_DoChange`, `EV_StopFloor`, and `EV_DoDonut` are injected.

## Credit rationale

Faithful whole-function line specials; retires slices on `line-specials-plat-generic-elevator`.
