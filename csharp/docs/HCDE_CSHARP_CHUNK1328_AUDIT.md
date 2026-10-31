# Chunk 1328 — Thinker link, the tick loop, client lights, destruction, and OnLoad from dthinker.cpp (1-373).

## Scope

Thinker link, the tick loop, client lights, destruction, and OnLoad from dthinker.cpp (1-373).

## Native ranges

- `src/playsim/dthinker.cpp` **FThinkerCollection::RunThinkers** 1–373 (373 lines)

**Credited native LOC:** 373 (tally `nativeLoc` 373).

## Managed

`VmThinkerRun1328Slice.cs`
Managed LOC (tally): 373.
Conversion commit: `f5c839a0341faea6d9d60ec5152af7e564b236dc`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Profile statics and the just-spawned link (**42–84**) | yes |
| Server tick loop and the profile sort (**92–200**) | yes |
| Dynamic-light recreate and the client tick (**210–307**) | yes |
| Destroy, traveller cleanup, and OnLoad (**315–366**) | yes |
| Band hull (**1–373**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| TickThinkers and ProfileThinkers bodies | lines 707–795, outside this band |
| DoDestroyThinkers body | line 581, outside this band |
| Commented client light profile | compiled out |

A just-spawned thinker at stat 32 or above is linked fresh and keeps the flag. Every other stat is linked on the main list, and the flag is cleared except for the travelling stat. The server loop resets the think counters, ticks stats 32 through 127, then repeats the fresh lists until they add nothing. A nonzero profile mode clears the map, sorts by name, call count, average, or total time, prints the yellow header with the Averg spelling, and then clears the mode. A zero profile limit prints every row. The client path clears the light-created flag, advances the local timer, and advances the world timer only when WorldPaused returns 0. Lights are enabled for hardware mode 4 with gl_lights, or any other mode with r_dynlights. Paused play skips thinker ticks and light ticks, and still recreates lights. The recreate flag is cleared before SetDynamicLights, and an effect runs when effects or fountain color is set, the actor renders locally, and the level is not frozen. Destroy skips travelling and static on both arrays, then destroys the extra main list. FullGC runs before a failure is reported. Full GC uses I_Error; a partial destroy uses I_FatalError. Traveller cleanup destroys stat 4, then removes travellers from fresh and main lists through stat 127. OnLoad walks every fresh list, then every main list including the extra slot.

| Managed (`VmThinkerRun1328Slice.cs`) | Native (`dthinker.cpp`) |
|--------------------------------------|-------------------------|
| Link, tick, client lights, destroy, and OnLoad | **1–373** |
| Band hull (exclusive slice) | **1–373** |

### Chunk **1328** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **373** |
| **Net headline** | **+373** vs post-**1327** |


**Post-review ratio:** 373/373 managed/native ≈ 100% (src file LOC at HEAD; see tally).

## Tests

- `VmThinkerRun1328SliceTests.Link_TicksFreshListsAndReportsDestroy`
- `VmThinkerRun1328SliceTests.Profile_SortsRowsAndRecreatesClientLights`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1328 --to 1328` (0 hard / 0 soft).

## Seams

TickThinkers, ProfileThinkers, AddTail, DoDestroyThinkers, DestroyThinkersInList, RemoveTravellers, and the list OnLoad stay outside this band. WorldPaused, SetDynamicLights, ShouldRenderLocally, P_RunEffect, isFrozen, the actor iterators, light Tick, FullGC, ClearGlobalVMStack, I_Error, and I_FatalError stay callees. The profile block inside the client light walk is compiled out.

## Credit rationale

UNCredited dthinker.cpp 1-373 (+373). A full tally line scan shows no overlap. SerializeThinkers at line 374 is already credited, so this band is not the end of the file.
