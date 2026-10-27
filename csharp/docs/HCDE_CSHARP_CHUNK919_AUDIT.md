# Chunk 919 — `EV_DoFloor`, `EV_FloorCrushStop`, `EV_StopFloor`, and `EV_BuildStairs` from `mapthinkers/a_floor.cpp` (**539–788**). Stops before `EV_DoDonut` **796+** and chunk **6** waggle **941+**.

## Scope

`EV_DoFloor`, `EV_FloorCrushStop`, `EV_StopFloor`, and `EV_BuildStairs` from `mapthinkers/a_floor.cpp` (**539–788**). Stops before `EV_DoDonut` **796+** and chunk **6** waggle **941+**.

## Native ranges

- `src/playsim/mapthinkers/a_floor.cpp` **FLevelLocals::EV_DoFloor** 539–552 (14 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **FLevelLocals::EV_FloorCrushStop** 562–579 (18 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **FLevelLocals::EV_StopFloor** 582–596 (15 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **FLevelLocals::EV_BuildStairs** 607–788 (182 lines)

**Credited native LOC:** 229 (tally `nativeLoc` 229).

## Managed

`VmAFloorEvLineFloor919Slice.cs`
Managed LOC (tally): 233.
Conversion commit: `1d594c975e8e690c1545137113206f05894a2de1`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `FLevelLocals::EV_DoFloor` (**539–552**) | yes |
| `FLevelLocals::EV_FloorCrushStop` (**562–579**) | yes |
| `FLevelLocals::EV_StopFloor` (**582–596**) | yes |
| `FLevelLocals::EV_BuildStairs` (**607–788**) | yes |

| Managed (`VmAFloorEvLineFloor919Slice.cs`) | Native (`a_floor.cpp`) |
|---------------------------------------------|----------------------|
| `EvDoFloor` | **539–552** |
| `EvFloorCrushStop` | **562–579** |
| `EvStopFloor` | **582–596** |
| `EvBuildStairs` | **607–788** |

### Chunk **919** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **229** |
| **Net headline** | **+229** vs post-**918** |


**Post-review ratio:** 233/229 managed/native ≈ 102% (src file LOC at HEAD; see tally).

## Tests

- `VmAFloorEvTests.EvDoFloor_CallsCreateFloorPerTag`
- `VmAFloorEvTests.EvFloorCrushStop_OnlyRaiseAndCrush`
- `VmAFloorEvTests.EvStopFloor_DestroyActiveThinker`
- `VmAFloorEvTests.EvBuildStairs_ZeroSpeed_ReturnsFalse`
- `VmAFloorEvTests.EvBuildStairs_TwoSidedChain_SpawnsStairThinker`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 919 --to 919` (0 hard / 0 soft).

## Seams

Tag iterators, `CreateFloor`, height/neighbor finders, thinker spawn, `SN_StopSequence`, and sector graph links are injected.

## Credit rationale

First honest ≥150 UNCredited band after chunk **918** `CreateFloor`; exclusive `VmAFloorEvLineFloor919Slice.cs` (types remain on `VmAFloorEv` for deferred donut/change). Retired **800** hull not re-tallied.
