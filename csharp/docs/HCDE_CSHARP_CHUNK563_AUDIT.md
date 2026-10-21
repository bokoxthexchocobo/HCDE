# Chunk 563 — DEH SetShadow/ClearShadow and MBF21 flag opcodes (4213–4393); ACS master TID, SetActorProperty, GetActorSound, ACSStringPool::UnlockForLevel

## Scope

DEH SetShadow/ClearShadow and MBF21 flag opcodes (4213–4393); ACS master TID, SetActorProperty, GetActorSound, ACSStringPool::UnlockForLevel

## Native ranges

- `src/gamedata/d_dehacked.cpp` **SetShadow** 4213–4218 (6 lines)
- `src/gamedata/d_dehacked.cpp` **ClearShadow** 4220–4225 (6 lines)
- `src/gamedata/d_dehacked.cpp` **DEFINE_ACTION_FUNCTION(AActor, MBF21_JumpIfFlagsSet)** 4304–4329 (26 lines)
- `src/gamedata/d_dehacked.cpp` **DEFINE_ACTION_FUNCTION(AActor, MBF21_AddFlags)** 4335–4361 (27 lines)
- `src/gamedata/d_dehacked.cpp` **DEFINE_ACTION_FUNCTION(AActor, MBF21_RemoveFlags)** 4367–4393 (27 lines)
- `src/playsim/p_acs.cpp` **DoGetMasterTID** 4062–4071 (10 lines)
- `src/playsim/p_acs.cpp` **DLevelScript::SetActorProperty** 4148–4164 (17 lines)
- `src/playsim/p_acs.cpp` **GetActorSound** 4740–4757 (18 lines)
- `src/playsim/p_acs.cpp` **ACSStringPool::UnlockForLevel** 1408–1421 (14 lines)

**Credited native LOC:** 151 (tally `nativeLoc` 151).

## Managed

`VmDehackedShadowMbf21.cs`, `VmAcsActorPropertySound.cs`, `VmAcsStringPoolUnlockForLevel.cs`
Managed LOC (tally): 170.
Conversion commit: `c41e21c9e5f4f672fcdefc81647ec477c7e176d1`.

## Padding review (2026-10-21)

| Managed | Native |
|---------|--------|
| `SetShadow` / `ClearShadow` | `d_dehacked.cpp` 4213–4225 |
| `Mbf21JumpIfFlagsSet` | `MBF21_JumpIfFlagsSet` 4304–4329 |
| `Mbf21AddFlags` | `MBF21_AddFlags` 4335–4361 |
| `Mbf21RemoveFlags` | `MBF21_RemoveFlags` 4367–4393 |
| `DoGetMasterTidNative` | `DoGetMasterTID` 4062–4071 |
| `SetActorPropertyByTidNative` | `DLevelScript::SetActorProperty` 4148–4164 |
| `GetActorSoundNative` | `GetActorSound` 4740–4757 |
| `RemoveLevelLock` | `ACSStringPool::UnlockForLevel` 1408–1421 |

**Post-review ratio:** 170/151 managed/native ≈ 113% (src file LOC at HEAD; see tally).

## Tests

- `VmDehackedShadowMbf21Tests.SetShadow_UsesNativeMfShadowAndClearsZdoomTrans`
- `VmDehackedShadowMbf21Tests.ClearShadow_OpaqueUsesNormalLegacyStyle`
- `VmDehackedShadowMbf21Tests.Mbf21JumpIfFlagsSet_RequiresAllSpecifiedBits`
- `VmDehackedShadowMbf21Tests.Mbf21AddFlags_InvokesSettersForMaskedBits`
- `VmDehackedShadowMbf21Tests.Mbf21RemoveFlags_InvokesClearersForMaskedBits`
- `VmAcsActorPropertySoundTests.DoGetMasterTidNative_PrefersMaster`
- `VmAcsActorPropertySoundTests.SetActorPropertyByTidNative_IteratesActorsWhenTidNonZero`
- `VmAcsActorPropertySoundTests.GetActorSoundNative_MapsSoundSlots`
- `VmAcsStringPoolUnlockForLevelTests.RemoveLevelLock_FindsIndex`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 563 --to 563` (0 hard / 0 soft).

## Seams

`AActor::SetState`, `flag1handlers`/`flag2handlers` tables, `GetActorIterator`, and pool storage are not hosted.

## Credit rationale

Whole-function ports; DEH band alone is below the 150 native floor so ACS helpers from the next planned `p_acs.cpp` band are included in this chunk.
