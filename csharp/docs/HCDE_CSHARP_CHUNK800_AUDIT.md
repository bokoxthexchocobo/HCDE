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
- `VmAFloorEvTests.EvDoDonut_SkipsWhenFloorPlaneMoving`
- `VmAFloorEvTests.EvDoChange_TrigChange_CopiesFrontTexture`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 800 --to 800` (0 hard / 0 soft).

## Seams

Tag iterators, `CreateFloor`, height/neighbor finders, thinker spawn/destroy, and sector texture/special hooks are injected.

## Credit rationale

Single 339-native FLevelLocals floor-EV band after `CreateFloor` (799); avoids engineered 157-line five-function patch.

## Legacy overlap: `EV_DoDonut` moved off chunk 6

Chunk **800** credits **`FLevelLocals::EV_DoDonut`** (**796–863**, **68** native) on `VmAFloorEv.cs`. That band had already been credited on legacy row **`p-floor-waggle-donut-elevator-chunk6`** (early conversion).

| Row | Change at chunk 800 land | `nativeLoc` delta | `managedLoc` delta |
|-----|---------------------------|-------------------|---------------------|
| **`a-floor-ev-chunk800`** | New six-function EV band | **+339** | +379 (new port) |
| **`p-floor-waggle-donut-elevator-chunk6`** | Removed only `EV_DoDonut` **796–863**; retained `DElevator::Tick` **941–984** | **−68** | **0** (no new managed file) |

**Net `convertedBehaviorNative` at chunk 800:** **+339 − 68 = +271** (e.g. **92 586** after chunk 799 → **92 857** after chunk 800 tally sync). The chunk-800 headline **339** is gross new credit; portfolio growth is **271** after deduplicating donut.

**Checksum unchanged:** `behaviorNativeChecksum` / `baselineNative` remain **508 086** (`convertedBehaviorNative` + `recognitionBehaviorNative` + `remainingNative`; overlap fix moves lines between buckets, not out of the baseline).
