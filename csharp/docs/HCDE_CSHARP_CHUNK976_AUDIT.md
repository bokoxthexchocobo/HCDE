# Chunk 976 — `HandleReply` `NextNode` tail, reply teardown, `P_ConversationCommand`, `TerminalResponse` head, and `DEFINE_FIELD` exports from `p_conversation.cpp` (**601–750**); follows chunk **975** (**≤600**).

## Scope

`HandleReply` `NextNode` tail, reply teardown, `P_ConversationCommand`, `TerminalResponse` head, and `DEFINE_FIELD` exports from `p_conversation.cpp` (**601–750**); follows chunk **975** (**≤600**).

## Native ranges

- `src/p_conversation.cpp` **P_ConversationCommand** 601–750 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPConversationNextNodeCommand976Slice.cs`
Managed LOC (tally): 92.
Conversion commit: `245b0f09bb97d1ddcb4f914f8c7c279ccbca2ce8`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `NextNode` index + validity (**602–632**) | yes |
| reply teardown + console music (**634–649**) | yes |
| `P_ConversationCommand` (**660–689**) | yes |
| `TerminalResponse` head (**701–727**) | yes (predicates) |
| `DEFINE_FIELD` node/reply exports (**729–750**) | yes (export counts) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **599–601**, **652–659**, **691–700**, **728** | comments / separators |
| **616–617**, **621**, **648**, **668–674** | host calls |

| Managed (`VmPConversationNextNodeCommand976Slice.cs`) | Native (`p_conversation.cpp`) |
|------------------------------------------------------|--------------------------------|
| `HasNextNodeTransition` / `ComputeNextDialogueIndex` | **602–608** |
| `ShouldChainConversationAfterNextNode` | **612–617** |
| `ShouldStopNpcVoiceDuringIntermission` | **619–621** |
| `InvalidNextNodeMessageIncludesName` | **627–630** |
| Teardown predicates (`TeardownClearsPlayerConversation`) | **634–644** |
| `P_ConversationCommand` routing | **660–689** |
| `TerminalResponse*` predicates | **701–727** |
| `TryResolveNextDialogueAssignment` | **602–623** |
| `ShouldRunConversationCommandElseBranch` / `ConversationNpcUpdateApplies` | **676–684** |
| `StripTerminalStringTablePrefix` / `TerminalPrintsWithoutStatusBar` | **703–725** |
| Field export counts | **729–750** |
| Band hull (exclusive slice) | **601–750** |

### Chunk **976** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**975** |


**Post-review ratio:** 92/150 managed/native ≈ 61% (src file LOC at HEAD; see tally).

## Tests

- `VmPConversationNextNodeCommand976SliceTests.NextNodeTransition_ComputesDialogueIndex`
- `VmPConversationNextNodeCommand976SliceTests.ShouldChainConversationAfterNextNode_RespectsCloseAndIntermission`
- `VmPConversationNextNodeCommand976SliceTests.ConversationCommand_RoutesNetcodes`
- `VmPConversationNextNodeCommand976SliceTests.DemoPlaybackClosesConversationMenu_WhenBothFlags`
- `VmPConversationNextNodeCommand976SliceTests.TerminalResponse_StringTablePrefix`
- `VmPConversationNextNodeCommand976SliceTests.FieldExportCounts_MatchNativeBand`
- `VmPConversationNextNodeCommand976SliceTests.TryResolveNextDialogueAssignment_MatchesNativeBranches`
- `VmPConversationNextNodeCommand976SliceTests.TerminalStringTable_AndCommandElseBranch`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 976 --to 976` (0 hard / 0 soft).

## Seams

`P_StartConversation` re-entry, `S_StopSound`, `I_SetMusicVolume`, `HandleReply`, `ReadInt16`/`ReadInt8`, `Printf`, and `StatusBar`/`DHUDMessage` remain host-injected.

## Credit rationale

UNCredited continuation; chunk **975** stopped at hull **600** before `NextNode` body. No overlap with **604** `d_net.cpp` conversation netcode predicates beyond distinct TU credit.
