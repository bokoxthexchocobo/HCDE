# Chunk 647 — Blackbox CVars through record/save API (`d_net_blackbox.cpp` 35–337)

## Scope

Blackbox CVars through record/save API (`d_net_blackbox.cpp` 35–337)

## Native ranges

- `src/d_net_blackbox.cpp` **net_blackbox_record** 35–41 (7 lines)
- `src/d_net_blackbox.cpp` **net_blackbox_record** 45–51 (7 lines)
- `src/d_net_blackbox.cpp` **net_blackbox_record** 158–167 (10 lines)
- `src/d_net_blackbox.cpp` **net_blackbox_record** 169–177 (9 lines)
- `src/d_net_blackbox.cpp` **net_blackbox_record** 179–187 (9 lines)
- `src/d_net_blackbox.cpp` **net_blackbox_record** 214–228 (15 lines)
- `src/d_net_blackbox.cpp` **net_blackbox_record** 230–233 (4 lines)
- `src/d_net_blackbox.cpp` **net_blackbox_record** 234–276 (43 lines)
- `src/d_net_blackbox.cpp` **net_blackbox_record** 278–299 (22 lines)
- `src/d_net_blackbox.cpp` **net_blackbox_record** 301–308 (8 lines)
- `src/d_net_blackbox.cpp` **net_blackbox_record** 310–310 (1 lines)
- `src/d_net_blackbox.cpp` **net_blackbox_record** 311–319 (9 lines)
- `src/d_net_blackbox.cpp` **net_blackbox_record** 321–322 (2 lines)
- `src/d_net_blackbox.cpp` **net_blackbox_record** 323–327 (5 lines)
- `src/d_net_blackbox.cpp` **net_blackbox_record** 329–331 (3 lines)
- `src/d_net_blackbox.cpp` **net_blackbox_record** 332–337 (6 lines)

**Credited native LOC:** 160 (tally `nativeLoc` 160).

## Managed

`VmDNetBlackbox.cs`
Managed LOC (tally): 324.
Conversion commit: `163679a1c39577b96531088f68e384becc1e7d41`.

## Padding review (2026-10-21)

| Managed | Native (`d_net_blackbox.cpp`) |
|---------|-------------------------------|
| `ClampRecordCvarNative` | 35–41 |
| `ClampSizeMbCvarNative` | 45–51 |
| `BlackboxBudgetBytesNative` / `EntryByteCostNative` | 108–113 |
| `BlackboxEvictOldestNative` | 114–124 |
| `BlackboxStoreEntryNative` | 125–155 |
| `NetBlackboxInitStateNative` / `NetBlackboxShutdownStateNative` | 156–177 |
| `ShouldLazyInitNative` | 178–188 |
| `NetBlackboxRecordPacketNative` | 189–213 |
| `NetBlackboxRecordTicIndexNative` | 214–233 |
| `NetBlackboxSaveIterateNative` | 234–277 |
| `ShouldAutoSaveNative` / `NetBlackboxAutoSaveNative` | 278–300 |
| `NetBlackboxClearNative` | 301–310 |
| `RunBlackboxSaveCcmdNative` | 311–322 |
| `RunBlackboxClearCcmdNative` | 323–331 |
| `RunBlackboxRecordCcmdNative` | 332–337 |

### Out-of-body uncredit (647)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **244** |
| **Out-of-body removed** | **84** |
| **Honest** | **160** |


**Post-review ratio:** 324/160 managed/native ≈ 202% (src file LOC at HEAD; see tally).

## Tests

- `VmDNetBlackboxTests.ClampRecordCvarNative_EnforcesBounds`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 647 --to 647` (0 hard / 0 soft).

## Seams

`std::mutex`, `I_msTime`, and filesystem flush are not hosted.

## Credit rationale

CVAR bodies plus ring buffer through blackbox CCMDs (excludes unported struct-only preamble).
