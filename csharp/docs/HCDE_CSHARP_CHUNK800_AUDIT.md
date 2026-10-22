# Chunk 800 — Floor line specials: `EV_DoFloor`, crush/stop, `EV_BuildStairs`, `EV_DoDonut`, `EV_DoChange` (`a_floor.cpp` 539–552, 562–596, 607–788, 796–863, 1113–1154).

## Scope

Floor line specials: `EV_DoFloor`, crush/stop, `EV_BuildStairs`, `EV_DoDonut`, `EV_DoChange` (`a_floor.cpp` 539–552, 562–596, 607–788, 796–863, 1113–1154).

## Native ranges

- `src/playsim/mapthinkers/a_floor.cpp` **FLevelLocals::EV_DoFloor** 539–552 (14 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **FLevelLocals::EV_FloorCrushStop** 562–579 (18 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **FLevelLocals::EV_StopFloor** 582–596 (15 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **FLevelLocals::EV_BuildStairs** 607–788 (182 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **FLevelLocals::EV_DoDonut** 796–863 (68 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **FLevelLocals::EV_DoChange** 1113–1154 (42 lines)

**Credited native LOC:** 339 (tally `nativeLoc` 339).

## Managed

`VmAFloorEv.cs`
Managed LOC (tally): 379.
Conversion commit: `0735d972a0a059d5a00e3939a998ed3beae9d013`.

## Padding review (2026-10-21)

| Managed | Native (`a_floor.cpp`) |
|---------|----------------------|
| `EvDoFloor` | `FLevelLocals::EV_DoFloor` **539–552** |
| `EvFloorCrushStop` | `FLevelLocals::EV_FloorCrushStop` **562–579** |
| `EvStopFloor` | `FLevelLocals::EV_StopFloor` **582–596** |
| `EvBuildStairs` | `FLevelLocals::EV_BuildStairs` **607–788** |
| `EvDoDonut` | `FLevelLocals::EV_DoDonut` **796–863** |
| `EvDoChange` | `FLevelLocals::EV_DoChange` **1113–1154** |


**Post-review ratio:** 379/339 managed/native ≈ 112% (src file LOC at HEAD; see tally).

## Tests

- `VmAFloorEvTests.EvDoFloor_CallsCreateFloorPerTag`
- `VmAFloorEvTests.EvStopFloor_DestroyActiveThinker`
- `VmAFloorEvTests.EvBuildStairs_ZeroSpeed_ReturnsFalse`
- `VmAFloorEvTests.EvBuildStairs_TwoSidedChain_SpawnsStairThinker`
- `VmAFloorEvTests.EvDoDonut_CreatesSlimeAndPillarThinkers`
- `VmAFloorEvTests.EvDoChange_TrigChange_CopiesFrontTexture`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 800 --to 800` (0 hard / 0 soft).

## Seams

Tag iterators, `CreateFloor`, height/neighbor finders, thinker spawn/destroy, and sector texture/special hooks are injected.

## Credit rationale

Single 339-native FLevelLocals floor-EV band after `CreateFloor` (799); avoids engineered 157-line five-function patch.
