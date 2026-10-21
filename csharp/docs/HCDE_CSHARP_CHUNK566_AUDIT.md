# Chunk 566 — ACSStringPool Dump/Unlock, P_MarkWorldVarStrings, DLevelScript list ops, DoGetMasterTID, SetActorProperty, GetActorSound (1395–1436, 3584–3662, 4062–4164, 4740–4757)

## Scope

ACSStringPool Dump/Unlock, P_MarkWorldVarStrings, DLevelScript list ops, DoGetMasterTID, SetActorProperty, GetActorSound (1395–1436, 3584–3662, 4062–4164, 4740–4757)

## Native ranges

- `src/playsim/p_acs.cpp` **ACSStringPool::Dump** 1395–1405 (11 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::UnlockForLevel** 1408–1421 (14 lines)
- `src/playsim/p_acs.cpp` **P_MarkWorldVarStrings** 1429–1436 (8 lines)
- `src/playsim/p_acs.cpp` **DLevelScript::GetController** 3584–3587 (4 lines)
- `src/playsim/p_acs.cpp` **DLevelScript::Unlink** 3589–3612 (24 lines)
- `src/playsim/p_acs.cpp` **DLevelScript::Link** 3614–3631 (18 lines)
- `src/playsim/p_acs.cpp` **DLevelScript::PutLast** 3633–3652 (20 lines)
- `src/playsim/p_acs.cpp` **DLevelScript::PutFirst** 3654–3662 (9 lines)
- `src/playsim/p_acs.cpp` **DoGetMasterTID** 4062–4071 (10 lines)
- `src/playsim/p_acs.cpp` **DLevelScript::SetActorProperty** 4148–4164 (17 lines)
- `src/playsim/p_acs.cpp` **GetActorSound** 4740–4757 (18 lines)

**Credited native LOC:** 153 (tally `nativeLoc` 153).

## Managed

`VmAcsStringPoolDump.cs`, `VmAcsStringPoolUnlockForLevel.cs`, `VmAcsMarkWorldVarStrings.cs`, `VmAcsLevelScriptList.cs`, `VmAcsActorPropertySound.cs`
Managed LOC (tally): 208.
Conversion commit: `0ed5ff6b515a426cad72fda2627a9e01bc7dc3b1`.

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

**Post-review ratio:** 208/153 managed/native ≈ 136% (src file LOC at HEAD; see tally).

## Tests

- `VmAcsStringPoolDumpTests.ShouldListEntry_SkipsFreeSlots`
- `VmAcsStringPoolUnlockForLevelTests.UnlockForLevelNative_RemovesMatchingLocks`
- `VmAcsMarkWorldVarStringsTests.MarkWorldVarStringsNative_MarksVarsAndEachArray`
- `VmAcsLevelScriptListTests.GetControllerNative_PicksClientSideThinker`
- `VmAcsLevelScriptListTests.PutFirstNative_MovesNodeToHead`
- `VmAcsLevelScriptListTests.PutLastNative_AppendsWhenListNonempty`
- `VmAcsActorPropertySoundTests.DoGetMasterTidNative_PrefersMaster`
- `VmAcsActorPropertySoundTests.SetActorPropertyByTidNative_UsesActivatorWhenTidZero`
- `VmAcsActorPropertySoundTests.SetActorPropertyByTidNative_IteratesActorsWhenTidNonZero`
- `VmAcsActorPropertySoundTests.GetActorSoundNative_MapsSoundSlots`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 566 --to 566` (0 hard / 0 soft).

## Seams

`GetActorIterator`, `DoSetActorProperty`, `Printf`, and GC write barriers are not hosted.

## Credit rationale

Whole-function ports for credited `p_acs.cpp` helpers only.
