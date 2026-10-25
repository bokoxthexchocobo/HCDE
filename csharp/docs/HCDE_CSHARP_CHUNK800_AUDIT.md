# Chunk 800 — Floor line specials: `EV_DoFloor`, crush/stop, `EV_BuildStairs`, `EV_DoDonut`, `EV_DoChange` (`a_floor.cpp` 539–552, 562–596, 607–788, 796–863, 1113–1154). Retired (2026-10-24): `--band` hull overlaps existing credit (p-floor-waggle-donut-elevator-chunk6 src/playsim/mapthinkers/a_floor.cpp 941-984).

## Scope

Floor line specials: `EV_DoFloor`, crush/stop, `EV_BuildStairs`, `EV_DoDonut`, `EV_DoChange` (`a_floor.cpp` 539–552, 562–596, 607–788, 796–863, 1113–1154). Retired (2026-10-24): `--band` hull overlaps existing credit (p-floor-waggle-donut-elevator-chunk6 src/playsim/mapthinkers/a_floor.cpp 941-984).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `e5a3a8e510ce4ea7fa66121989201e2d92c5e095`.

## Padding review (2026-10-21)

| Managed | Native (`a_floor.cpp`) |
|---------|----------------------|
| `EvDoFloor` | `FLevelLocals::EV_DoFloor` **539–552** |
| `EvFloorCrushStop` | `FLevelLocals::EV_FloorCrushStop` **562–579** |
| `EvStopFloor` | `FLevelLocals::EV_StopFloor` **582–596** |
| `EvBuildStairs` | `FLevelLocals::EV_BuildStairs` **607–788** |
| `EvDoDonut` | `FLevelLocals::EV_DoDonut` **796–863** |
| `EvDoChange` | `FLevelLocals::EV_DoChange` **1113–1154** |

### Out-of-body uncredit (800)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **339** |
| **Out-of-body removed** | **14** |
| **Honest** | **325** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 800 --to 800` (0 hard / 0 soft).

## Seams

Tag iterators, `CreateFloor`, height/neighbor finders, thinker spawn/destroy, and sector texture/special hooks are injected.

## Credit rationale

Single 339-native FLevelLocals floor-EV band after `CreateFloor` (799); avoids engineered 157-line five-function patch.
