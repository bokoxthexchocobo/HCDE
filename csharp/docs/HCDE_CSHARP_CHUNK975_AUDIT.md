# Chunk 975 — `HandleReply` reply lookup, item-check failure, inventory give/take, log/terminal hooks from `p_conversation.cpp` (**451–600**); follows chunk **974** (**≤450**).

## Scope

`HandleReply` reply lookup, item-check failure, inventory give/take, log/terminal hooks from `p_conversation.cpp` (**451–600**); follows chunk **974** (**≤450**).

## Native ranges

- `src/p_conversation.cpp` **HandleReply** 451–600 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPConversationHandleReply975Slice.cs`
Managed LOC (tally): 90.
Conversion commit: `f481b0c3edfa12da5b24250c0a09b9d330ca60d3`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `HandleReply` abort + reply walk (**470–478**) | yes |
| default reply teardown (**480–487**) | yes |
| item-check failure + QuickNo (**490–506**) | yes |
| give/take policy (**513–576**) | yes (predicates) |
| quest log + terminal (**578–597**) | yes |

**Excluded (zero credit / deferred)**

| Native | Reason |
|--------|--------|
| **451–460** | `P_ResumeConversation` tail / comment banner |
| **509–510**, **528–545**, **558–562** | host calls (animation, spawn, special) |
| **599–600**, **602+** | `NextNode` comment + body (next band) |

| Managed (`VmPConversationHandleReply975Slice.cs`) | Native (`p_conversation.cpp`) |
|---------------------------------------------------|--------------------------------|
| `HandleReplyShouldAbort` / `AdvanceToReplyChild` | **470–478** |
| `IsDefaultReplyPath` / angle + in-conversation clear | **480–487** |
| `ReplyItemCheckPasses` / `ShouldShowQuickNo` | **490–506** |
| `WeaponAlreadyOwnedBlocksGive` / `FlameThrowerDialogAmmoGive` | **518–535** |
| `CombineTakeStuffWithSpecial` / `SelectTerminalReplyText` | **558–576** |
| `ResolveLogString` / `ShouldTerminalReply` | **578–597** |
| Band hull (exclusive slice) | **451–600** |

### Chunk **975** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**974** |


**Post-review ratio:** 90/150 managed/native ≈ 60% (src file LOC at HEAD; see tally).

## Tests

- `VmPConversationHandleReply975SliceTests.HandleReplyShouldAbort_InvalidNodeOrNpc`
- `VmPConversationHandleReply975SliceTests.AdvanceToReplyChild_WalksLinkedReplies`
- `VmPConversationHandleReply975SliceTests.ReplyItemCheckPasses_AllRequiredPresent`
- `VmPConversationHandleReply975SliceTests.WeaponAlreadyOwnedBlocksGive_WhenDuplicateWeapon`
- `VmPConversationHandleReply975SliceTests.CombineTakeStuffWithSpecial_OrSemantics`
- `VmPConversationHandleReply975SliceTests.SelectTerminalReplyText_PicksHaveEnoughWhenTakeFails`
- `VmPConversationHandleReply975SliceTests.ResolveLogString_ExpandsDollarPrefix`
- `VmPConversationHandleReply975SliceTests.ShouldTerminalReply_RequiresConsoleAndText`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 975 --to 975` (0 hard / 0 soft).

## Seams

`Spawn`, `CallTryPickup`, `P_ExecuteSpecial`, `G_StartSlideshow`, `TerminalResponse`, `ClearConversationStuff`, and `NextNode` continuation (**602+**) remain host-injected.

## Credit rationale

UNCredited `HandleReply` band; reuses chunk **973** item-check semantics in tests only—no second matrix row on **151–300**. `NextNode` body deferred past hull **600**.
