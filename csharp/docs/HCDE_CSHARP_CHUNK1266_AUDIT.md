# Chunk 1266 — Profile reset, the one-second render-time cache, and the bench command from hw_clock.cpp (1-199).

## Scope

Profile reset, the one-second render-time cache, and the bench command from hw_clock.cpp (1-199).

## Native ranges

- `src/common/rendering/hwrenderer/data/hw_clock.cpp` **ResetProfilingData** 1–199 (199 lines)

**Credited native LOC:** 199 (tally `nativeLoc` 199).

## Managed

`VmHwClock1266Slice.cs`
Managed LOC (tally): 158.
Conversion commit: `eeb1df07761f992fb4426ec184afbd5b86cfdc93`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| ResetProfilingData (**48–70**) | yes |
| AppendRenderTimes (**78–98**) | yes |
| AppendRenderStats (**100–106**) | yes |
| AppendLightStats (**108–112**) | yes |
| rendertimes (**114–126**) | yes |
| renderstats (**128–133**) | yes |
| lightstats (**135–140**) | yes |
| CheckBench (**147–174**) | yes |
| bench (**176–191**) | yes |
| checkBenchActive (**195–199**) | yes |
| Band hull (**1–199**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Cycle counter sample | host timer |
| benchmarks.txt | host file |

Reset clears the cycles named in that function and then clocks All only while profiling is active. Finish, the 2D pair, and the command-buffer count are left as they were. The BSP field subtracts the clip cycle from the BSP cycle. Worker wait subtracts the three setup cycles from the worker total. Render times stay cached until the clock moves by more than one second, so a later BSP value is not shown inside that second. The first sample at exactly one second still returns the empty cache. Wall, flat, sprite, and light lines are formatted on every call. Bench sets the print flag, turns the FPS counter on and records the wait when that counter was off, and leaves the counter alone while clearing the switch when it was already on. A console that is up refreshes the wait in that second case. The console is hidden either way. The save waits while the start is still inside five seconds. It appends three blank lines, prints the saved line even when the file does not open, turns FPS back off only when bench enabled it, and clears the print flag. The active flag is on when the render-time stat is present and active, or when a bench print is pending.

| Managed (`VmHwClock1266Slice.cs`) | Native (`hw_clock.cpp`) |
|-----------------------------------|--------------------------|
| Reset, cache, and bench | **1–199** |
| Band hull (exclusive slice) | **1–199** |

### Chunk **1266** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **199** |
| **Net headline** | **+199** vs post-**1265** |


**Post-review ratio:** 158/199 managed/native ≈ 79% (src file LOC at HEAD; see tally).

## Tests

- `VmHwClock1266SliceTests.Times_CacheForOneSecondAndLeaveFinishAlone`
- `VmHwClock1266SliceTests.Bench_WaitsFiveSecondsAndPrintsWhenTheFileFails`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1266 --to 1266` (0 hard / 0 soft).

## Seams

The cycle counter and the millisecond clock are host inputs. Opening benchmarks.txt and hiding the console are host steps. The slice keeps which cycles reset, the text formulas, the one-second cache, and the bench wait.

## Credit rationale

UNCredited hw_clock.cpp 1-199 (+199). A full tally line scan shows no overlap. The file ends at line 199.
