# Chunk 1327 — The prediction debug tick, the metrics row, and the soft warning from d_net_predict_impl.cpp (1-375).

## Scope

The prediction debug tick, the metrics row, and the soft warning from d_net_predict_impl.cpp (1-375).

## Native ranges

- `src/d_net_predict_impl.cpp` **Net_PredictionDebugTick** 1–375 (375 lines)

**Credited native LOC:** 375 (tally `nativeLoc` 375).

## Managed

`VmPredictDebug1327Slice.cs`
Managed LOC (tally): 543.
Conversion commit: `2756a73880857263925c1c8f6413729c333bbacd`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Level, net, and authority gates (**26–35**) | yes |
| Ping sample and extrema (**43–76**) | yes |
| Mirror walk and the one-second desync (**78–134**) | yes |
| Sample interval, CSV header, and the metrics row (**136–262**) | yes |
| Soft warning, trace dump, and the window roll (**264–375**) | yes |
| Band hull (**1–375**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Invasion and lag name strings | callees outside this file |
| Trace file body | DebugTrace::SaveToFile |

Level 0, a non-level game, demo playback, and the local authority return before the ping log. The ping log waits 1000 ms, then the console lead is `(ClientTic-gametic)/TicDup` and every other client lead is `sequence - gametic/TicDup`. Extrema update on every accepted frame. A mismatch stamp of 0 is treated as unset, so a clock of 0 never arms it. The sample returns before the CSV when `gametic - last < max(interval, 1)`, and that return does not roll the window. The CSV prints the window extrema, then the window resets to this frame. Soft warnings require level 2 and `EnterTic >= grace`. Ack lag, low available tic with backlog pressure, mirror drift, a stale attack, a passive storm, and two hard repairs share one bracketed trigger list. Only mirror, stale, passive, and hard repairs dump a trace, and only at level 3, at most once per 5000 ms. The soft-warn line itself is limited to once per 250 ms. A zero report stamp does not rate-limit.

| Managed (`VmPredictDebug1327Slice.cs`) | Native (`d_net_predict_impl.cpp`) |
|----------------------------------------|------------------------------|
| Debug tick, CSV, and soft warning | **1–375** |
| Band hull (exclusive slice) | **1–375** |

### Chunk **1327** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **375** |
| **Net headline** | **+375** vs post-**1326** |


**Post-review ratio:** 543/375 managed/native ≈ 145% (src file LOC at HEAD; see tally).

## Tests

- `VmPredictDebug1327SliceTests.Gates_LogPingAndHoldTheWindow`
- `VmPredictDebug1327SliceTests.Sample_WritesTheCsvAndRatesTheSoftWarn`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1327 --to 1327` (0 hard / 0 soft).

## Seams

HCDELiveReportIntervalElapsed treats a zero stamp as elapsed and then stores the clock. Net_InvasionStateName, Net_LagStateName, and DebugTrace::SaveToFile stay callees. The metrics path is the app-data directory plus the file name.

## Credit rationale

UNCredited d_net_predict_impl.cpp 1-375 (+375). A full tally line scan shows no overlap. The file ends at line 375.
