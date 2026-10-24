# Chunk 647 — Blackbox CVars through record/save API (`d_net_blackbox.cpp` 35–337)

## Scope

Blackbox CVars through record/save API (`d_net_blackbox.cpp` 35–337)

## Native ranges

- `src/d_net_blackbox.cpp` **BlackboxStoreEntry** 35–337 (303 lines)

**Credited native LOC:** 303 (tally `nativeLoc` 303).

## Managed

`VmDNetBlackbox.cs`
Managed LOC (tally): 153.
Conversion commit: `31c4766166ac7776910b29286701789a77c5f16a`.

## Padding review (2026-10-21)

| Managed | Native (`d_net_blackbox.cpp`) |
|---------|-------------------------------|
| `ClampRecordCvarNative` / `ClampSizeMbCvarNative` | 35–51 |
| `BlackboxBudgetBytesNative` / `ShouldEvictOldestNative` | 108–155 |
| `ShouldLazyInitNative` | 183–187 |
| `ShouldSkipRecordPacketNative` / `FillPacketHeaderNative` | 189–212 |
| `ShouldAutoSaveNative` | 278–299 |

**Post-review ratio:** 153/303 managed/native ≈ 50% (src file LOC at HEAD; see tally).

## Tests

- `VmDNetBlackboxTests.ClampSizeMbCvarNative_EnforcesBounds`
- `VmDNetBlackboxTests.ShouldAutoSaveNative_RespectsCooldown`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 647 --to 647` (0 hard / 0 soft).

## Seams

`std::mutex`, `I_msTime`, and filesystem flush are not hosted.

## Credit rationale

Whole translation unit body from CVARs through `Net_BlackboxClear`.
