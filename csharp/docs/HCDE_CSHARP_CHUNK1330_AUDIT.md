# Chunk 1330 — Thinker list link, destroy, save, traveller removal, OnLoad, tick, profile, and the empty-link destructor from dthinker.cpp (444-808).

## Scope

Thinker list link, destroy, save, traveller removal, OnLoad, tick, profile, and the empty-link destructor from dthinker.cpp (444-808).

## Native ranges

- `src/playsim/dthinker.cpp` **FThinkerList::AddTail** 444–808 (365 lines)

**Credited native LOC:** 365 (tally `nativeLoc` 365).

## Managed

`VmThinkerList1330Slice.cs`
Managed LOC (tally): 434.
Conversion commit: `12b93a6c2cdf956e8e28cbdee33b2b076538506e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| AddTail ring insert (**452–474**) | yes |
| First thinker and root marks (**483–516**) | yes |
| Head, tail, and empty (**525–558**) | yes |
| Destroy copy, sentinel, and both catches (**567–628**) | yes |
| Save, travellers, and OnLoad (**637–698**) | yes |
| Tick and profile (**707–794**) | yes |
| Empty-link destructor (**804–807**) | yes |
| Band hull (**444–808**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| SerializeThinkers | ends at line 443, already credited |
| OnDestroy | starts at line 809, already credited |
| Remove, CallTick, and CallPostBeginPlay | callees |

AddTail rejects a linked thinker and a thinker scheduled for destruction before it creates the sentinel. A missing sentinel is a self-ring, then the thinker is inserted ahead of that sentinel. One-argument and two-argument write barriers follow the native order. FirstThinker clamps with an unsigned compare, prefers the live head, and then reads the fresh head. MarkRoots marks live and fresh sentinels through 127 and the extra live slot 128. GetHead treats a self-ring as no head and requires the head to point back at the sentinel. GetTail returns the previous thinker unless that previous thinker is the sentinel. IsEmpty is true only when the sentinel is missing or its next link is null, so a self-ring is not empty. Destroy copies the ring, clears every link, destroys the sentinel, then destroys the copies. A VM abort prints the header, the red message when the message pointer is set, and the stack trace, then forces deletion and continues. A recoverable error prints one bold line, forces deletion, and continues. Any other exception escapes before the rest of the copies are destroyed. DestroyThinkers reports `DestroyThinkers failed` only when the copy pass returns an error. Save walks until the sentinel flag and refuses a null or euthanize next link. RemoveTravellers saves the next thinker first, clears the travelling bit when saving, and destroys the thinker otherwise. Euthanize travellers stay. OnLoad saves the next thinker and calls the override only when the thinker is not scheduled for destruction. Tick and profile count every node. A just-spawned thinker can move to the destination before post-begin. A destination plus a thinker that has already ticked raises `There is a thinker in the fresh list that has already ticked.` A destroyed thinker is not ticked, and its just-spawned bit stays. Profile counts the call, clocks, ticks, then unclocks, and a tick exception skips the unclock and leaves the just-spawned bit. The destructor requires both links to be null.

| Managed (`VmThinkerList1330Slice.cs`) | Native (`dthinker.cpp`) |
|---------------------------------------|-------------------------|
| List link, destroy, tick, and profile | **444–808** |
| Band hull (exclusive slice) | **444–808** |

### Chunk **1330** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **365** |
| **Net headline** | **+365** vs post-**1329** |


**Post-review ratio:** 434/365 managed/native ≈ 119% (src file LOC at HEAD; see tally).

## Tests

- `VmThinkerList1330SliceTests.Link_BuildsTheRingAndDestroysIt`
- `VmThinkerList1330SliceTests.Tick_SavesTravellersAndProfiles`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1330 --to 1330` (0 hard / 0 soft).

## Seams

Remove, CallTick, CallPostBeginPlay, Serialize, and the GC stay callees. SerializeThinkers ends at line 443. OnDestroy at line 809 stays on the serialize-instance chunk.

## Credit rationale

UNCredited dthinker.cpp 444-808 (+365). A full tally line scan shows no overlap. The save-archive chunk ends at line 443 and OnDestroy starts at line 809. This is the largest remaining hole of at most 400 lines, ahead of whole-file jit_flow.cpp 1-362.
