# Chunk 566 — ACSStringPool Dump/Unlock, P_MarkWorldVarStrings, DLevelScript list ops, DoGetMasterTID, SetActorProperty, GetActorSound (1395–1436, 3584–3662, 4062–4164, 4740–4757) Merged: [565]. Retired (2026-10-24): `--band` hull overlaps existing credit (acs-random-getcvar-polywait src/playsim/p_acs.cpp 3664-3672).

## Scope

ACSStringPool Dump/Unlock, P_MarkWorldVarStrings, DLevelScript list ops, DoGetMasterTID, SetActorProperty, GetActorSound (1395–1436, 3584–3662, 4062–4164, 4740–4757) Merged: [565]. Retired (2026-10-24): `--band` hull overlaps existing credit (acs-random-getcvar-polywait src/playsim/p_acs.cpp 3664-3672).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `50674c640645aa595c7d36600b26a22a85ee38bb`.

## Padding review (2026-10-21)

| Managed | Native (`p_acs.cpp`) |
|---------|-------------------|
| `VmAcsStringPoolDump` | `ACSStringPool::Dump` 1395–1405 |
| `VmAcsStringPoolUnlockForLevel` | `ACSStringPool::UnlockForLevel` 1408–1421 |
| `VmAcsMarkWorldVarStrings` | `P_MarkWorldVarStrings` 1429–1436 |
| `VmAcsLevelScriptList` | `GetController` / `Unlink` / `Link` / `PutLast` / `PutFirst` 3584–3662 |
| `DoGetMasterTidNative` | `DoGetMasterTID` 4062–4071 |
| `SetActorPropertyByTidNative` | `DLevelScript::SetActorProperty` 4148–4164 |
| `GetActorSoundNative` | `GetActorSound` 4740–4757 |

<!-- merged from chunk565 -->
| Managed | Native (`d_net.cpp`) |
|---------|-------------------|
| `GetNetBufferSizeNative` | `GetNetBufferSize` 5010–5140 |
| `HSendPacketShouldSkip` | `HSendPacket` demo guard 5147–5149 |
| `HGetPacketFromLocalBuffer` | `HGetPacket` local buffer branch 5176–5182 |

### Out-of-body uncredit (566)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **153** |
| **Out-of-body removed** | **0** |
| **Honest** | **153** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 566 --to 566` (0 hard / 0 soft).

## Seams

`GetActorIterator`, `DoSetActorProperty`, `Printf`, and GC write barriers are not hosted.

## Credit rationale

Whole-function ports for credited `p_acs.cpp` helpers only.
