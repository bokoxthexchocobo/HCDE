# Chunk 1452 — User-command record skipping in d_net.cpp (9508-9676).

## Scope

User-command record skipping in d_net.cpp (9508-9676).

## Native ranges

- `src/d_net.cpp` **Net_TrySkipUserCmdMessage** 9508–9676 (169 lines)

**Credited native LOC:** 169 (tally `nativeLoc` 169).

## Managed

`VmNet1452Slice.cs`
Managed LOC (tally): 172.
Conversion commit: `a54b0b274d744b4d5c50a90e3f63737c8970f6ea`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| User-command skip (**9508–9676**) | yes |
| Band hull (**9508–9676**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Net_TrySkipCommand | lines through 9506 are already credited |
| Invasion accessors | start at line 9679 |

A user command stops the scan after its button chain and flagged axes. An event-only tail is accepted only when the caller marks it as the final record. A failed opcode skip leaves the stream cursor where it was.

| Managed (`VmNet1452Slice.cs`) | Native (`d_net.cpp`) |
|-------------------------------|----------------------|
| TrySkipUserCmd through SkipCommand | **9508–9676** |
| Band hull (exclusive slice) | **9508–9676** |

### Chunk **1452** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **169** |
| **Net headline** | **+169** vs the prior chunk |


**Post-review ratio:** 172/169 managed/native ≈ 102% (src file LOC at HEAD; see tally).

## Tests

- `VmNet1452SliceTests.UserCmdAxesAndFailClosedSkip`
- `VmNet1452SliceTests.RemainingRecordsAndBoundary`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1452 --to 1452` (0 hard / 0 soft).

## Seams

Net_TrySkipUserCmdMessage through Net_SkipCommand. Net_TrySkipCommand ends at 9506. Net_GetLatency starts at 9679.

## Credit rationale

UNCredited d_net.cpp 9508-9676 (+169). One contiguous span of the user-command skippers and the fail-closed SkipCommand wrapper. The span does not cross credited code.
