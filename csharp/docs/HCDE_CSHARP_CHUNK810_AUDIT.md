# Chunk 810 — Freelist `P_GetSecnode` / `P_PutSecnode`, `P_AddSecnode`, `P_DelSecnode`, `P_DelSeclist`, and `P_CreateSecNodeList` from `p_secnodes.cpp` (42–283).

## Scope

Freelist `P_GetSecnode` / `P_PutSecnode`, `P_AddSecnode`, `P_DelSecnode`, `P_DelSeclist`, and `P_CreateSecNodeList` from `p_secnodes.cpp` (42–283).

## Native ranges

- `src/playsim/p_secnodes.cpp` **P_GetSecnode** 42–56 (15 lines)
- `src/playsim/p_secnodes.cpp` **P_PutSecnode** 66–70 (5 lines)
- `src/playsim/p_secnodes.cpp` **P_AddSecnode** 85–128 (44 lines)
- `src/playsim/p_secnodes.cpp` **P_DelSecnode** 144–181 (38 lines)
- `src/playsim/p_secnodes.cpp` **P_DelSeclist** 194–198 (5 lines)
- `src/playsim/p_secnodes.cpp` **P_CreateSecNodeList** 216–283 (68 lines)

**Credited native LOC:** 175 (tally `nativeLoc` 175).

## Portfolio delta

**Gross CLI band:** **242** lines in the UNCredited-range probe; tally credits **175** whole-function native LOC only.
**67** uncovered lines in that band are inter-function gaps, comments, or non-function lines (not credited).

| | Native LOC |
|---|---:|
| **Gross** (CLI UNCredited band) | **242** |
| **Credited** (whole functions, this row) | **175** |
| **Overlap trim** | **0** |
| **Net headline Δ** | **+175** (see `convertedBehaviorNative` in tally at land commit) |

## Managed

`VmPSecnodes.cs`
Managed LOC (tally): 180.
Conversion commit: `fbf4f57220a8638ea29d97b6186d80703abad11a`.

## Padding review (2026-10-21)

| Managed | Native (`p_secnodes.cpp`) |
|---------|---------------------------|
| `GetSecnode` / `PutSecnode` | **42–70** |
| `AddSecnode` | **85–128** |
| `DelSecnode` | **144–181** |
| `DelSeclist` | **194–198** |
| `CreateSecNodeList` | **216–283** |

### Out-of-body uncredit (810)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **175** |
| **Out-of-body removed** | **0** |
| **Honest** | **175** |


**Post-review ratio:** 180/175 managed/native ≈ 103% (src file LOC at HEAD; see tally).

## Tests

- `VmPSecnodesTests.GetSecnode_ReusesFreelist`
- `VmPSecnodesTests.AddSecnode_LinksSectorHead`
- `VmPSecnodesTests.AddSecnode_ReusesExistingSectorNode`
- `VmPSecnodesTests.DelSecnode_UnlinksThreads`
- `VmPSecnodesTests.DelSeclist_ClearsThingList`
- `VmPSecnodesTests.CreateSecNodeList_PrunesUnusedNodes`
- `VmPSecnodesTests.CreateSecNodeList_AddsLineCrossingSectors`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 810 --to 810` (0 hard / 0 soft).

## Seams

Arena allocation and `FBlockLinesIterator` line sweep are injected via `CreateListContext.LineCrossings` and `SectorHeadMap`; portal `portnode_t` templates omitted.

## Credit rationale

First tally credit for Boom secnode list maintenance used by sector touch tracking.
