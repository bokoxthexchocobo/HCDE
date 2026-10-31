# Chunk 1313 — Drawer threads, the debug-draw cursor, and the memcpy rows from r_thread.cpp (1-285).

## Scope

Drawer threads, the debug-draw cursor, and the memcpy rows from r_thread.cpp (1-285).

## Native ranges

- `src/common/rendering/r_thread.cpp` **DrawerThreads::Execute** 1–285 (285 lines)

**Credited native LOC:** 285 (tally `nativeLoc` 285).

## Managed

`VmDrawerThreads1313Slice.cs`
Managed LOC (tally): 398.
Conversion commit: `8ae609e5ba281ad5900ab7e1d461a1d61af0306b`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Execute, debug cursor, and the worker wait (**53–158**) | yes |
| Start, stop, and the NUMA layout (**161–240**) | yes |
| Allocation, the barrier, and memcpy rows (**244–285**) | yes |
| Band hull (**1–285**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Real worker threads | the slice records the setup and the test calls WorkerMain |
| Header push template | not in this cpp span |

An empty or null command list returns before the threads start. Execute starts the workers, locks the start mutex and then the end mutex, pushes the list, adds the thread count to the remaining tasks, unlocks end and then start, and notifies. The debug cursor compares each worker position plus `r_debug_draw * 120` with the end before it zeroes the position. If any worker is still short of the end, the end becomes 0. Otherwise the end grows by the debug cvar. Wait returns immediately when no tasks remain. A leftover count is fatal unless the workers finish inside the five-second wait, and the fatal path leaves the lists in place. Cleanup zeroes each worker queue, runs the explicit command destructor, clears each list, and then clears the active lists. WorkerMain returns while the shutdown flag is set and does that before taking work. Otherwise it takes the list at its cursor, advances the cursor, and sets the NUMA rows from the screen height with integer division. A nonzero debug cvar increments the cursor for every command and runs the command only while the cursor stays below the end. A zero cvar runs every command. The task count decrements with unsigned wrap, and the end condition notifies only when it hits 0. A zero NUMA total warns once, and only when the multithread cvar is 1, then uses one thread. A cvar of 0 forces one thread. Any other value replaces the NUMA total. An unchanged count does not stop or rebind. A matching NUMA count assigns the core index inside the node. Any other count uses a flat core index, one NUMA node, and affinity 0. Stop joins every worker, clears the list, and clears the shutdown flag. Allocation forwards the size as an int. The barrier increments, notifies, and waits while the count is below the core count. Memcpy uses the header skip and row count. The skip is the clipped first line plus the core remainder, and a negative remainder follows the toward-zero modulo. The copy steps by the core count times the pitch times the pixel size.

| Managed (`VmDrawerThreads1313Slice.cs`) | Native (`r_thread.cpp`) |
|------------------------------------------|-------------------------|
| Drawer threads, cursor, and memcpy | **1–285** |
| Band hull (exclusive slice) | **1–285** |

### Chunk **1313** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **285** |
| **Net headline** | **+285** vs post-**1312** |


**Post-review ratio:** 398/285 managed/native ≈ 140% (src file LOC at HEAD; see tally).

## Tests

- `VmDrawerThreads1313SliceTests.Start_DistributesNumaAndWarnsOnceWhenTheCountIsUnknown`
- `VmDrawerThreads1313SliceTests.Worker_StepsTheCursorAndCopiesRows`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1313 --to 1313` (0 hard / 0 soft).

## Seams

NUMA node counts and the thread affinity call are host queries. The screen height is a host read. Printf and I_FatalError are host messages. Frame memory allocation is a host buffer. Worker threads are recorded and the test calls WorkerMain.

## Credit rationale

UNCredited r_thread.cpp 1-285 (+285). A full tally line scan shows no overlap. The file ends at line 285.
