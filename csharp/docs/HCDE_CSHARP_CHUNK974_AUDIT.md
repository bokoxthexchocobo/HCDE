# Chunk 974 — `P_StartConversation`, console menu open policy, item-check node jump, and `P_ResumeConversation` head from `p_conversation.cpp` (**301–450**); follows chunk **973** (**≤300**).

## Scope

Strife conversation start/resume gates and menu-open policy from `p_conversation.cpp` (**301–450**).

## Native ranges

- `src/p_conversation.cpp` **P_StartConversation** hull 301–450 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPConversationStart974Slice.cs`

## Tests

- `VmPConversationStart974SliceTests.StartConversationActorsValid_RejectsNullOrNonPrimary`
- `VmPConversationStart974SliceTests.NpcBusyWithOtherPlayer_MatchesReference`
- `VmPConversationStart974SliceTests.TryResolveItemCheckDialogueIndex_JumpsWhenAllItemsPresent`
- `VmPConversationStart974SliceTests.ShouldFaceTalker_RespectsDontFaceTalkerFlag`
- `VmPConversationStart974SliceTests.ResolveMenuClassName_FallsBackToConversationMenu`
- `VmPConversationStart974SliceTests.ShouldResetStaticLastReply_WhenNodeChanges`
- `VmPConversationStart974SliceTests.ResumeConversationShouldRestart_RequiresBothPointers`

## Seams

`PlayIdle`, `S_Sound`, `A_FaceTarget`, `PClass`/`VMCall` menu init, `M_*` menu APIs, and resume re-entry remain host-injected.

## Credit rationale

UNCredited continuation on `p_conversation.cpp`; does not duplicate chunk **933** (`p_lnspec`) or chunk **973** item/reply band.
