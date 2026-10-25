# Chunk 739 — Pointer rearrange/transfer, action friendship, legacy sound ex/stop (`p_actionfunctions.cpp` 531–728).

## Scope

Pointer rearrange/transfer, action friendship, legacy sound ex/stop (`p_actionfunctions.cpp` 531–728).

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_RearrangePointers)** 531–592 (62 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_TransferPointer)** 611–639 (29 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_CopyFriendliness)** 654–670 (17 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_PlaySoundEx)** 681–716 (36 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_StopSoundEx)** 718–728 (11 lines)

**Credited native LOC:** 155 (tally `nativeLoc` 155).

## Managed

`VmPActionPointerSound.cs`
Managed LOC (tally): 195.
Conversion commit: `456201f10dd71f53ebc0b5032a0696420ea77b23`.

## Padding review (2026-10-21)

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `RearrangePointers` | `DEFINE_ACTION_FUNCTION(AActor, A_RearrangePointers)` 531–592 |
| `TransferPointer` | `DEFINE_ACTION_FUNCTION(AActor, A_TransferPointer)` 611–642 |
| `CopyFriendlinessFromPointer` | `DEFINE_ACTION_FUNCTION(AActor, A_CopyFriendliness)` 654–670 |
| `PlaySoundEx` | `DEFINE_ACTION_FUNCTION(AActor, A_PlaySoundEx)` 681–716 |
| `StopSoundEx` | `DEFINE_ACTION_FUNCTION(AActor, A_StopSoundEx)` 718–728 |

### Out-of-body uncredit (739)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **158** |
| **Out-of-body removed** | **3** |
| **Honest** | **155** |


**Post-review ratio:** 195/155 managed/native ≈ 126% (src file LOC at HEAD; see tally).

## Tests

- `VmPActionPointerSoundTests.RearrangePointers_TargetFromMaster`
- `VmPActionPointerSoundTests.RearrangePointers_NullClearsTarget`
- `VmPActionPointerSoundTests.TransferPointer_SelfNullWhenRecipientMatches`
- `VmPActionPointerSoundTests.TransferPointer_NullRecipient_NoOp`
- `VmPActionPointerSoundTests.CopyFriendlinessFromPointer_SkipsPlayers`
- `VmPActionPointerSoundTests.PlaySoundEx_AttenuationMapping`
- `VmPActionPointerSoundTests.PlaySoundEx_LoopingSkipsWhenAlreadyPlaying`
- `VmPActionPointerSoundTests.StopSoundEx_ValidChannelStops`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 739 --to 739` (0 hard / 0 soft).

## Seams

`COPY_AAPTR`, `VerifyTargetChain`, `VerifyMasterChain`, `CopyFriendliness`, and `S_Sound`/`S_StopSound` are injected.

## Credit rationale

DEFINE action bodies ported as managed methods; no extra delegate wrappers.
