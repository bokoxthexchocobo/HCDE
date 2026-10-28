# Chunk 973 — `ClearConversationStuff`, `CheckStrifeItem`, `TakeStrifeItem` guards, `ShouldSkipReply`, and `SendConversationReply` wire selection from `p_conversation.cpp` (**151–300**).

## Scope

Strife conversation item checks, reply filtering, music-volume clamp, and DEM conversation reply encoding from `p_conversation.cpp` (**151–300**).

## Native ranges

- `src/p_conversation.cpp` **CheckStrifeItem** hull 151–300 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPConversationItemReply973Slice.cs`

## Tests

- `VmPConversationItemReply973SliceTests.ClearConversationPointers_ResetsAllFields`
- `VmPConversationItemReply973SliceTests.CheckStrifeItem_RequiresAmountOnPc`
- `VmPConversationItemReply973SliceTests.TakeStrifeItemBlocked_SkipsQuestKeySigil`
- `VmPConversationItemReply973SliceTests.ClampDlgMusicVolume_ClampsToUnitInterval`
- `VmPConversationItemReply973SliceTests.ShouldSkipReply_EmptyTextOrFailedChecks`
- `VmPConversationItemReply973SliceTests.SelectReplyWireKind_MapsDemConversationOpcodes`

## Seams

`TakeInventory` VM call, `Net_Write*`, menu close, and `P_FreeStrifeConversations` body remain host-injected.

## Credit rationale

UNCredited Strife dialog band with no prior `p_conversation.cpp` matrix row; predicates mirror item/reply gating without re-crediting line-special conversation helpers on chunk **933**.
