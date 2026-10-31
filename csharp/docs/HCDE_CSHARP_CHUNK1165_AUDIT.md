# Chunk 1165 — Corpse queue and unblock actions from `a_action.cpp` (**1–169**).

## Scope

Corpse queue and unblock actions from `a_action.cpp` (**1–169**).

## Native ranges

- `src/playsim/a_action.cpp` **Corpse queue** 1–169 (169 lines)

**Credited native LOC:** 169 (tally `nativeLoc` 169).

## Managed

`VmAActionCorpse1165Slice.cs`
Managed LOC (tally): 161.
Conversion commit: `8be0a0585b1a5713e3e08c3e16ea9cdc8fb47dde`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `HCDE_QueueCorpse` (**37–79**) | yes |
| `A_Unblock` (**87–136**) | yes |
| `A_QueueCorpse` (**146–155**) | yes |
| `A_DeQueueCorpse` (**158–169**) | yes |
| Band hull (**1–169**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| `P_DropItem` | the drop call is recorded and the class lookup stays host-supplied |
| `GC::WriteBarrier` | the push is the queue insert; the barrier has no extra state |

Bit **0** of the corpse filter keeps monster bodies. Any other value leaves the body alone. A missing corpse or a missing level returns before that test. A body already in the queue is removed first. A queue size of **0** or less destroys that body and returns. While the queue is at least that size, the head is destroyed when it exists and is not the body being queued, then the head is removed. The body is pushed after that.

`A_Unblock` sets alpha to **1** and visdir to **0** when stealth is set, then clears solid. A conversation with a drop type drops amount **-1** at probability **256**, clears the conversation, and returns. Otherwise the conversation is cleared. When drop is requested and the actor is not a player pawn, each drop whose name is not `None` is spawned when the class exists, using that link's amount and probability. A corpse with no player that is not a player pawn is then queued.

`A_QueueCorpse` queues only when the corpse flag is set and returns **0**. `A_DeQueueCorpse` removes the actor when it is in the queue and returns **0**.

| Managed (`VmAActionCorpse1165Slice.cs`) | Native (`a_action.cpp`) |
|------------------------------------------|--------------------------|
| Corpse queue | **1–169** |
| Band hull (exclusive slice) | **1–169** |

### Chunk **1165** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **169** |
| **Net headline** | **+169** vs post-**1164** |


**Post-review ratio:** 161/169 managed/native ≈ 95% (src file LOC at HEAD; see tally).

## Tests

- `VmAActionCorpse1165SliceTests.QueueCorpse_FiltersDedupesAndEvictsTheOldest`
- `VmAActionCorpse1165SliceTests.Unblock_DropsConversationAndSkipsTheQueue`
- `VmAActionCorpse1165SliceTests.Unblock_WalksDropsAndQueuesAMonsterCorpse`
- `VmAActionCorpse1165SliceTests.Actions_QueueOnlyCorpsesAndDequeueTheActor`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1165 --to 1165` (0 hard / 0 soft).

## Seams

Drop-item class lookup stays host-supplied. Destroy is a flag on the actor. The corpse list is the caller's queue.

## Credit rationale

UNCredited **`a_action.cpp`** **1–169** (+169). The file ends at **169**. A full tally line scan shows no overlap. An older sketch in `VmAAction.cs` is off the tally.
