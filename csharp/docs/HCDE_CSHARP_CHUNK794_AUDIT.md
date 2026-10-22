# Chunk 794 — `DDoor::Construct` / `FLevelLocals::EV_DoDoor` (`a_doors.cpp` 355–538)

## Scope

Whole-function overload `DDoor::Construct(sector_t*, EVlDoor, …)` and `FLevelLocals::EV_DoDoor` only (not the single-arg `Construct` at 344–347).

**Credited native LOC:** 176 (75 + 101).

## Managed

`VmADoorConstructEv.cs` — tally managedLoc at HEAD (see `loc_tally.json`).

## Padding review — method-to-native map

| Managed | Native (`a_doors.cpp`) |
|---------|-------------------------|
| `Construct` — compat light clear | **368–371** |
| `Construct` — `doorClose` branch + `DoorSound(false)` | **375–380** |
| `Construct` — `doorOpen` / `doorRaise` + conditional sound | **382–389** |
| `Construct` — `doorCloseWaitOpen` | **391–395** |
| `Construct` — `doorWaitRaise` | **397–401** |
| `Construct` — `doorWaitClose` | **403–410** |
| `Construct` — bot dist (plat vs floor/ceiling point) | **414–425** |
| `Construct` — `OldFloorDist` + diag trace | **425–428** |
| `EvDoDoor` — key check | **445–446** |
| `EvDoDoor` — manual door / usefail / sector | **448–462** |
| `EvDoDoor` — existing `DDoor` raise reversal / close | **465–507** |
| `EvDoDoor` — `CreateThinker` manual | **509–510** |
| `EvDoDoor` — tagged iterator | **512–525** |
| `EvDoDoor` — `Net_DiagTraceDoorEvent` | **528–536** |

No credited managed symbols outside this table. `DoorSound` calls delegate to **chunk 793** (`VmADoorTickSound.DoorSound`).

## Manual review (2026-10-22)

- **Structure:** `Construct` and `EvDoDoor` follow native switch/if layout; no added branches for ratio tuning.
- **Constants:** `DDoor::EVlDoor` values live on `VmADoorTickSound` (0=`doorClose` … 5=`doorWaitClose`) and are shared with chunk **793** (fixed enum drift that had mis-ordered types).
- **Seams (allowed):** `DoorConstructContext` / `EvDoDoorContext` inject `Find*Surrounding`, `P_CheckKeys`, tag iteration, `CreateThinker`, diag hooks — native calls are not duplicated elsewhere in managed.
- **Not wrappers:** `EvLine` / `EvSide` / `EvThing` are minimal field bags for `line_t` / `AActor` seams, not forwarding facades.
- **Bugfix:** `EvDoDoor` activator player index uses `GetActivatorPlayerIndex` instead of hard-coded `0`.
- **No** `Plan*`, empty stubs, or duplicate door tick/sound implementations in this file.
- **Ratio ~147%:** Extra LOC is DTO properties and explicit `VmADoorTickSound` qualification; no comment blocks or helper top-ups.

## Tests

`VmADoorConstructEvTests` (6 cases) — see chunk794.json.

## Verified

`reaudit_chunks.py audit --from 794 --to 794` 0/0; padding-scan 0/0 for credited symbols.
