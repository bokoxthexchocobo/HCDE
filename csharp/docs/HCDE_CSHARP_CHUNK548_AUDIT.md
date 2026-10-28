# Chunk 548 — DetermineType, PatchThing flag handlers, SetShadow/ClearShadow, MBF21 flag opcodes (3969–4393)

## Scope

DetermineType, PatchThing flag handlers, SetShadow/ClearShadow, MBF21 flag opcodes (3969–4393)

## Native ranges

- `src/gamedata/d_dehacked.cpp` **DEFINE_ACTION_FUNCTION(ADehackedPickup, DetermineType)** 3969–3996 (28 lines)
- `src/gamedata/d_dehacked.cpp` **SetNoSector** 4018–4023 (6 lines)
- `src/gamedata/d_dehacked.cpp` **ClearNoSector** 4025–4030 (6 lines)
- `src/gamedata/d_dehacked.cpp` **SetNoBlockmap** 4032–4037 (6 lines)
- `src/gamedata/d_dehacked.cpp` **ClearNoBlockmap** 4039–4044 (6 lines)
- `src/gamedata/d_dehacked.cpp` **SetCountkill** 4046–4051 (6 lines)
- `src/gamedata/d_dehacked.cpp` **ClearCountkill** 4053–4058 (6 lines)
- `src/gamedata/d_dehacked.cpp` **SetBoss** 4060–4064 (5 lines)
- `src/gamedata/d_dehacked.cpp` **ClearBoss** 4066–4070 (5 lines)
- `src/gamedata/d_dehacked.cpp` **SetCountitem** 4072–4079 (8 lines)
- `src/gamedata/d_dehacked.cpp` **ClearCountitem** 4081–4088 (8 lines)
- `src/gamedata/d_dehacked.cpp` **SetFriendly** 4090–4096 (7 lines)
- `src/gamedata/d_dehacked.cpp` **ClearFriendly** 4098–4104 (7 lines)
- `src/gamedata/d_dehacked.cpp` **SetFullVol** 4106–4110 (5 lines)
- `src/gamedata/d_dehacked.cpp` **ClearFullVol** 4112–4116 (5 lines)
- `src/gamedata/d_dehacked.cpp` **SetTranslucent** 4118–4123 (6 lines)
- `src/gamedata/d_dehacked.cpp` **ClearTranslucent** 4125–4130 (6 lines)
- `src/gamedata/d_dehacked.cpp` **CheckTranslucent** 4132–4137 (6 lines)
- `src/gamedata/d_dehacked.cpp` **SetTranslation1** 4143–4147 (5 lines)
- `src/gamedata/d_dehacked.cpp` **ClearTranslation1** 4149–4153 (5 lines)
- `src/gamedata/d_dehacked.cpp` **CheckTranslation1** 4155–4158 (4 lines)
- `src/gamedata/d_dehacked.cpp` **SetTranslation2** 4160–4164 (5 lines)
- `src/gamedata/d_dehacked.cpp` **ClearTranslation2** 4166–4170 (5 lines)
- `src/gamedata/d_dehacked.cpp` **CheckTranslation2** 4172–4175 (4 lines)
- `src/gamedata/d_dehacked.cpp` **SetBounces** 4178–4188 (11 lines)
- `src/gamedata/d_dehacked.cpp` **ClearBounces** 4190–4196 (7 lines)
- `src/gamedata/d_dehacked.cpp` **SetMissile** 4199–4204 (6 lines)
- `src/gamedata/d_dehacked.cpp` **ClearMissile** 4206–4209 (4 lines)
- `src/gamedata/d_dehacked.cpp` **SetShadow** 4213–4218 (6 lines)
- `src/gamedata/d_dehacked.cpp` **ClearShadow** 4220–4225 (6 lines)
- `src/gamedata/d_dehacked.cpp` **DEFINE_ACTION_FUNCTION(AActor, MBF21_JumpIfFlagsSet)** 4304–4329 (26 lines)
- `src/gamedata/d_dehacked.cpp` **DEFINE_ACTION_FUNCTION(AActor, MBF21_AddFlags)** 4335–4361 (27 lines)
- `src/gamedata/d_dehacked.cpp` **DEFINE_ACTION_FUNCTION(AActor, MBF21_RemoveFlags)** 4367–4393 (27 lines)

**Credited native LOC:** 280 (tally `nativeLoc` 280).

## Managed

`VmDehackedDetermineTypeFlags.cs`, `VmDehackedShadowMbf21.cs`
Managed LOC (tally): 291.
Conversion commit: `ef9b9eae940c98dae1df443b115c3ef6da752274`.

## Padding review (2026-10-21)

| Managed | Native (`d_dehacked.cpp`) |
|---------|---------------------------|
| `VmDehackedDetermineTypeFlags.*` | DetermineType + PatchThing handlers 3969–4209 |
| `SetShadow` / `ClearShadow` | 4213–4225 |
| `Mbf21JumpIfFlagsSet` | MBF21_JumpIfFlagsSet 4304–4329 |
| `Mbf21AddFlags` | MBF21_AddFlags 4335–4361 |
| `Mbf21RemoveFlags` | MBF21_RemoveFlags 4367–4393 |

### Out-of-body uncredit (548)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **280** |
| **Out-of-body removed** | **0** |
| **Honest** | **280** |


**Post-review ratio:** 291/280 managed/native ≈ 104% (src file LOC at HEAD; see tally).

## Tests

- `VmDehackedDetermineTypeFlagsTests.DetermineTypeBinarySearch_FindsMatch`
- `VmDehackedDetermineTypeFlagsTests.SetNoSector_SetsFlag`
- `VmDehackedDetermineTypeFlagsTests.ClearNoSector_ClearsFlag`
- `VmDehackedDetermineTypeFlagsTests.SetNoBlockmap_SetsFlag`
- `VmDehackedDetermineTypeFlagsTests.ClearNoBlockmap_ClearsFlag`
- `VmDehackedDetermineTypeFlagsTests.SetCountkill_AdjustsTotal`
- `VmDehackedDetermineTypeFlagsTests.ClearCountkill_AdjustsTotal`
- `VmDehackedDetermineTypeFlagsTests.SetBoss_SetsFlags`
- `VmDehackedDetermineTypeFlagsTests.ClearBoss_ClearsFlags`
- `VmDehackedDetermineTypeFlagsTests.SetCountitem_IncrementsOnce`
- `VmDehackedDetermineTypeFlagsTests.ClearCountitem_DecrementsOnce`
- `VmDehackedDetermineTypeFlagsTests.SetFriendly_AdjustsTotal`
- `VmDehackedDetermineTypeFlagsTests.ClearFriendly_AdjustsTotal`
- `VmDehackedDetermineTypeFlagsTests.SetFullVol_SetsFlags`
- `VmDehackedDetermineTypeFlagsTests.ClearFullVol_ClearsFlags`
- `VmDehackedDetermineTypeFlagsTests.SetTranslucent_UsesHalfAlpha`
- `VmDehackedDetermineTypeFlagsTests.ClearTranslucent_ResetsAlpha`
- `VmDehackedDetermineTypeFlagsTests.CheckTranslucent_IgnoresZdoomTrans`
- `VmDehackedDetermineTypeFlagsTests.SetTranslation1_PromotesToT3`
- `VmDehackedDetermineTypeFlagsTests.ClearTranslation1_DemotesThroughT2`
- `VmDehackedDetermineTypeFlagsTests.CheckTranslation1_MatchesNative`
- `VmDehackedDetermineTypeFlagsTests.SetTranslation2_PromotesToT3`
- `VmDehackedDetermineTypeFlagsTests.ClearTranslation2_DemotesThroughT1`
- `VmDehackedDetermineTypeFlagsTests.CheckTranslation2_MatchesNative`
- `VmDehackedDetermineTypeFlagsTests.SetBounces_MissileUsesClassic`
- `VmDehackedDetermineTypeFlagsTests.ClearBounces_ZerosBounceFlags`
- `VmDehackedDetermineTypeFlagsTests.SetMissile_UpgradesDehBounce`
- `VmDehackedDetermineTypeFlagsTests.ClearMissile_ClearsMissileBits`
- `VmDehackedShadowMbf21Tests.SetShadow_UsesNativeMfShadowAndClearsZdoomTrans`
- `VmDehackedShadowMbf21Tests.ClearShadow_OpaqueUsesNormalLegacyStyle`
- `VmDehackedShadowMbf21Tests.Mbf21JumpIfFlagsSet_RequiresAllSpecifiedBits`
- `VmDehackedShadowMbf21Tests.Mbf21AddFlags_InvokesSettersForMaskedBits`
- `VmDehackedShadowMbf21Tests.Mbf21RemoveFlags_InvokesClearersForMaskedBits`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 548 --to 548` (0 hard / 0 soft).

## Seams

`UnlinkFromWorld`/`LinkToWorld`, `PClass::FindActor`, and live `AActor` mutation are not hosted; managed code mirrors each handler body.

## Credit rationale

Credits only whole native functions with matching methods in `VmDehackedDetermineTypeFlags`, `VmDehackedShadowMbf21`, and per-function tests.
