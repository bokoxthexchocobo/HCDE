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

**Credited native LOC:** 175 (tally `nativeLoc` 175 — sum of **whole-function** `nativeRanges` only).

## Uncredited-range check (pre-port)

```
uncredited_range_check.py --file src/playsim/p_secnodes.cpp --start 42 --end 283
  [UNCredited] cli src/playsim/p_secnodes.cpp 42-283 (242 lines)
exit 0
```

**242 vs 175:** The CLI band counts every line in **42–283**. Tally credits **175** because `nativeRanges` cover only the six hosted functions above. The **67** uncovered lines in that band are not separate conversion targets: explicit template instantiations (**130–131**, **183–184**), `P_DelSeclist` / portal `portnode_t` overloads (**200–204**), and comments/blank lines between function bodies (**57–65**, **71–84**, **129–143**, **182–193**, **199–215**). **Overlap trim:** **0** (no prior `p_secnodes.cpp` credit).

## Portfolio delta

| | Native LOC |
|---|---:|
| **Gross** (`p-secnodes-freelist-chunk810`, tally `nativeLoc`) | **+175** |
| **Overlap trim** | **0** |
| **Net headline Δ** | **+175** (93,378 → **93,553**) |

## Managed

`VmPSecnodes.cs`
Managed LOC (tally): **156** at HEAD (`f20fd1d9` tally sync; port tree `5fb77828`).
Conversion commit: `5fb778284b7b042c6930956eeb5f749be61bfc89`.

## Padding review (2026-10-21)

| Managed | Native (`p_secnodes.cpp`) |
|---------|---------------------------|
| `GetSecnode` / `PutSecnode` | **42–70** |
| `AddSecnode` | **85–128** |
| `DelSecnode` | **144–181** |
| `DelSeclist` | **194–198** |
| `CreateSecNodeList` | **216–283** |

**Post-review ratio:** 156/175 managed/native ≈ 89% (tally `managedLoc` at HEAD; see `fix_chunk_tally.py`).

## Land hygiene

Lands: port **`5fb77828`**, tally **`ee823d4e`**, then follow-up fixes **`4e644693`**, **`4fb66796`**, **`f20fd1d9`** (build/test/tally `managedLoc`) — each pushed as **new commits** (no amend on `origin`). Full gate green at **`f20fd1d9`**.

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
