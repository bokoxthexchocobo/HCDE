# Chunk 1444 — Consistency markers and the net status gate in d_net.cpp (6518-6726).

## Scope

Consistency markers and the net status gate in d_net.cpp (6518-6726).

## Native ranges

- `src/d_net.cpp` **MakeConsistencies** 6518–6726 (209 lines)

**Credited native LOC:** 209 (tally `nativeLoc` 209).

## Managed

`VmNet1444Slice.cs`
Managed LOC (tally): 229.
Conversion commit: `1cd6b35d2ad6e2477df2dfba63da4a11e34bd86f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Consistency marker and status gate (**6518–6726**) | yes |
| Band hull (**6518–6726**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Desync dump | lines 6228-6517 are already credited |
| NetUpdate | line 6727 starts the next function |

MakeConsistencies stamps a nonzero marker once per duplicated tic while a map is loaded. The status gate returns before the world runs when the level is waiting or a start delay remains. A long gap marks peers missing, and only a non-authority peer lets a waiting client freeze the world. Command skipping bleeds a client lead in batches and does not starve the authority.

| Managed (`VmNet1444Slice.cs`) | Native (`d_net.cpp`) |
|-------------------------------|----------------------|
| MakeConsistencies through UpdateStatus | **6518–6726** |
| Band hull (exclusive slice) | **6518–6726** |

### Chunk **1444** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **209** |
| **Net headline** | **+209** vs the prior chunk |


**Post-review ratio:** 229/209 managed/native ≈ 110% (src file LOC at HEAD; see tally).

## Tests

- `VmNet1444SliceTests.MarkersAndStatusGates`
- `VmNet1444SliceTests.LeadSkipThrottle`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1444 --to 1444` (0 hard / 0 soft).

## Seams

MakeConsistencies and Net_UpdateStatus. NetUpdate starts at line 6727.

## Credit rationale

UNCredited d_net.cpp 6518-6726 (+209). One contiguous run of whole functions. The span does not cross credited code.
