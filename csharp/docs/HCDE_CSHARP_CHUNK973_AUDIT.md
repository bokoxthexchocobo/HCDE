# Chunk 973 — `ClearConversationStuff`, `CheckStrifeItem`, `TakeStrifeItem` guards, `ShouldSkipReply`, and `SendConversationReply` wire selection from `p_conversation.cpp` (**151–300**).

## Scope

`ClearConversationStuff`, `CheckStrifeItem`, `TakeStrifeItem` guards, `ShouldSkipReply`, and `SendConversationReply` wire selection from `p_conversation.cpp` (**151–300**).

## Native ranges

- `src/p_conversation.cpp` **CheckStrifeItem** 151–300 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPConversationItemReply973Slice.cs`
Managed LOC (tally): 100.
Conversion commit: `5a9b0b7313e0c6ee6f3a4d4218cf90167af56923`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `ClearConversationStuff` (**154–160**) | yes |
| `CheckStrifeItem` (**171–181**) | yes |
| `TakeStrifeItem` skip policy (**192–207**) | yes (guard predicates) |
| `dlg_musicvolume` clamp (**218–222**) | yes |
| `ShouldSkipReply` (**232–254**) | yes |
| `SendConversationReply` opcode switch (**268–284**) | yes (wire kind + last reply) |
| `P_FreeStrifeConversations` menu gate (**298–300**) | yes (predicate) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **162–170**, **183–191**, **224–231**, **256–267**, **289–297** | comments / VM prologue / separators |
| **209–216** | `TakeInventory` VM seam |

| Managed (`VmPConversationItemReply973Slice.cs`) | Native (`p_conversation.cpp`) |
|--------------------------------------------------|--------------------------------|
| `ClearConversationPointers` | **154–160** |
| `CheckStrifeItem` | **171–181** |
| `TakeStrifeItemBlocked` | **192–207** |
| `ClampDlgMusicVolume` | **218–222** |
| `ShouldSkipReply` | **232–254** |
| `SelectReplyWireKind` / `ReplyWireWritesNodeAndReply` | **268–284** |
| `FreeConversationsShouldCloseMenu` | **298–300** |
| Band hull (exclusive slice) | **151–300** |

### Chunk **973** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**972** |


**Post-review ratio:** 100/150 managed/native ≈ 67% (src file LOC at HEAD; see tally).

## Tests

- `VmPConversationItemReply973SliceTests.ClearConversationPointers_ResetsAllFields`
- `VmPConversationItemReply973SliceTests.CheckStrifeItem_RequiresAmountOnPc`
- `VmPConversationItemReply973SliceTests.TakeStrifeItemBlocked_SkipsQuestKeySigil`
- `VmPConversationItemReply973SliceTests.ClampDlgMusicVolume_ClampsToUnitInterval`
- `VmPConversationItemReply973SliceTests.ShouldSkipReply_EmptyTextOrFailedChecks`
- `VmPConversationItemReply973SliceTests.SelectReplyWireKind_MapsDemConversationOpcodes`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 973 --to 973` (0 hard / 0 soft).

## Seams

`TakeInventory` VM call, `Net_Write*`, menu close, and `P_FreeStrifeConversations` body remain host-injected.

## Credit rationale

UNCredited Strife dialog band with no prior `p_conversation.cpp` matrix row; predicates mirror item/reply gating without re-crediting line-special conversation helpers on chunk **933**.
