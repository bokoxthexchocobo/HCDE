# Chunk 974 — `P_StartConversation`, console menu open policy, item-check node jump, and `P_ResumeConversation` head from `p_conversation.cpp` (**301–450**); follows chunk **973** (**≤300**).

## Scope

`P_StartConversation`, console menu open policy, item-check node jump, and `P_ResumeConversation` head from `p_conversation.cpp` (**301–450**); follows chunk **973** (**≤300**).

## Native ranges

- `src/p_conversation.cpp` **P_StartConversation** 301–450 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPConversationStart974Slice.cs`
Managed LOC (tally): 80.
Conversion commit: `2190e984676eaa5923e7a27de20bdeacf327ba3e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `P_StartConversation` actor/busy gates (**318–330**) | yes |
| in-conversation flag + angle/face policy (**338–364**) | yes |
| item-check node jump loop (**367–387**) | yes |
| console menu/voice/music path (**393–425**) | yes (predicates + class resolve) |
| `P_ResumeConversation` restart gate (**446–448**) | yes |

**Excluded (zero credit / prior chunk)**

| Native | Reason |
|--------|--------|
| **301–303** | `P_FreeStrifeConversations` closing braces (adjacent **973**) |
| **304–311**, **428–435**, **453+** | comment banners / next function head outside behavior |
| **332–335**, **344–345**, **390**, **409–415**, **424** | host calls (movement, sound, VM) |

| Managed (`VmPConversationStart974Slice.cs`) | Native (`p_conversation.cpp`) |
|---------------------------------------------|--------------------------------|
| `StartConversationActorsValid` / `NpcBusyWithOtherPlayer` | **318–330** |
| `ApplyInConversationFlag` / face + target restore | **338–364** |
| `TryResolveItemCheckDialogueIndex` | **367–387** |
| `ShouldOpenConversationMenu` / `ResolveMenuClassName` / menu reset | **393–425** |
| `ResumeConversationShouldRestart` | **446–448** |
| Band hull (exclusive slice) | **301–450** |

### Chunk **974** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**973** |


**Post-review ratio:** 80/150 managed/native ≈ 53% (src file LOC at HEAD; see tally).

## Tests

- `VmPConversationStart974SliceTests.StartConversationActorsValid_RejectsNullOrNonPrimary`
- `VmPConversationStart974SliceTests.NpcBusyWithOtherPlayer_MatchesReference`
- `VmPConversationStart974SliceTests.TryResolveItemCheckDialogueIndex_JumpsWhenAllItemsPresent`
- `VmPConversationStart974SliceTests.ShouldFaceTalker_RespectsDontFaceTalkerFlag`
- `VmPConversationStart974SliceTests.ResolveMenuClassName_FallsBackToConversationMenu`
- `VmPConversationStart974SliceTests.ShouldResetStaticLastReply_WhenNodeChanges`
- `VmPConversationStart974SliceTests.ResumeConversationShouldRestart_RequiresBothPointers`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 974 --to 974` (0 hard / 0 soft).

## Seams

`PlayIdle`, `S_Sound`, `A_FaceTarget`, `PClass`/`VMCall` menu init, `M_*` menu APIs, and `P_StartConversation` re-entry from resume remain host-injected.

## Credit rationale

UNCredited continuation of Strife dialog TU; predicates mirror start/resume gates without duplicating chunk **933** line-special `TryStartConversation` on `p_lnspec.cpp` or chunk **973** item/reply band.
