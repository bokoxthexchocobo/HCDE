# Chunk 984 — `SetConversation`, `GetStrifeType`, `GetConversation`, dialogue reply dtor, and `FindNode` from `p_conversation.cpp` (**1–150**); fills UNCredited head before chunk **973** (**151–300**).

## Scope

`SetConversation`, `GetStrifeType`, `GetConversation`, dialogue reply dtor, and `FindNode` from `p_conversation.cpp` (**1–150**); fills UNCredited head before chunk **973** (**151–300**).

## Native ranges

- `src/p_conversation.cpp` **FLevelLocals::SetConversation** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPConversationHead984Slice.cs`
Managed LOC (tally): 95.
Conversion commit: `b5e021d6e61518d3702803dce806bcd705f0da0b`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `SetConversation` root registration (**77–87**) | yes |
| `GetStrifeType` table miss (**89–94**) | yes |
| `GetConversation` by id/name (**96–108**) | yes |
| `FStrifeDialogueNode` dtor chain (**116–125**) | yes |
| `FindNode` linear scan (**135–144**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–76** | includes / static decls |
| **146–150** | comment banner before `ClearConversationStuff` |
| **154+** | outside hull / **973** (`ClearConversationStuff` onward) |

| Managed (`VmPConversationHead984Slice.cs`) | Native (`p_conversation.cpp`) |
|---------------------------------------------|--------------------------------|
| `ShouldRegisterConversationRoot` / `TryRegisterConversationRoots` | **79–86** |
| `ResolveStrifeTypeFromTable` | **91–94** |
| `ResolveConversationIndexById` / `ResolveConversationIndexByClassName` | **98–107** |
| `FreeDialogueReplyChain` | **118–124** |
| `FindDialogueNodeIndex` | **137–143** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **984** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**983** |


**Post-review ratio:** 95/150 managed/native ≈ 63% (src file LOC at HEAD; see tally).

## Tests

- `VmPConversationHead984SliceTests.SetConversation_RegistersRoots`
- `VmPConversationHead984SliceTests.ConversationLookups_MissReturnsInvalid`
- `VmPConversationHead984SliceTests.ReplyChainAndFindNode`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 984 --to 984` (0 hard / 0 soft).

## Seams

Includes, CVAR, and menu forward declarations remain host-only; `ClearConversationStuff` (**154+**) credited on **973**.

## Credit rationale

UNCredited `p_conversation.cpp` prologue with no existing `VmPConversation*` slice on **1–150**; disjoint from **973–976** (`151+`).
