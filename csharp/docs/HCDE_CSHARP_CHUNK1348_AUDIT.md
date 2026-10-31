# Chunk 1348 — Monster director sweep, regroup hints, and ai_status from d_net_aidirector.cpp (1-284), through end of file.

## Scope

Monster director sweep, regroup hints, and ai_status from d_net_aidirector.cpp (1-284), through end of file.

## Native ranges

- `src/d_net_aidirector.cpp` **HCDEAIDirectorTick** 1–284 (284 lines)

**Credited native LOC:** 284 (tally `nativeLoc` 284).

## Managed

`VmAiDirector1348Slice.cs`
Managed LOC (tally): 341.
Conversion commit: `9e02bf528504550f92e47638d21f8f7dfb316648`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Cvars, state, and authority (**64–91**) | yes |
| Regroup hint read and consume (**93–132**) | yes |
| Monster sweep and clusters (**134–225**) | yes |
| Tick cadence and the named RNG touch (**227–252**) | yes |
| ai_status text (**254–284**) | yes |
| Band hull (**1–284**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Actor mutation and replication | this file never writes actor state |
| pr_aidirector draws | the tick names the stream and does not draw |

A disabled director returns before the authority check and leaves the tick counter alone. The sweep cvar clamps to 1 through 70. Tick 1 always sweeps, and later sweeps follow the modulo of the clamped period. A null level still counts the sweep and clears the stores, and it does not refresh the wallclock. Living monsters join the first cluster whose center is within 512, inclusive, and the new center is the reciprocal-weighted average. Corpses and non-positive health are counted and not clustered. A dormant living monster is counted as both dormant and alive and is clustered. Regroup hints are stored only for clusters of at least 3, and only while the hint cvar is on. Reading a hint does not remove it. Consuming a hint increments the total and also leaves it stored. The target consume reads the current cluster center, skips a null out pointer, and does not increment when the group is negative or out of range. A null actor misses before a null hint entry. The status text is the ai_status printf, including the phase line. Wallclock values are diagnostic and do not choose a cluster.

| Managed (VmAiDirector1348Slice.cs) | Native (d_net_aidirector.cpp) |
|------------------------------------|-------------------------------|
| Sweep, hints, tick, and status | **1–284** |
| Band hull (exclusive slice) | **1–284** |

### Chunk **1348** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **284** |
| **Net headline** | **+284** vs the prior chunk |


**Post-review ratio:** 341/284 managed/native ≈ 120% (src file LOC at HEAD; see tally).

## Tests

- `VmAiDirector1348SliceTests.DriveSweepAndClusters`
- `VmAiDirector1348SliceTests.HintsStatusAndConsume`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1348 --to 1348` (0 hard / 0 soft).

## Seams

The level supplies the thinker list and the tic. The authority predicate and the millisecond clock are inputs. Clustering uses the actor position, monster bit, dormant bit, corpse bit, and health.

## Credit rationale

UNCredited d_net_aidirector.cpp 1-284 (+284). A full tally line scan shows no overlap. This is the largest remaining hole of at most 400 lines, ahead of RegistersScopeStateNode.cpp 1-283.
