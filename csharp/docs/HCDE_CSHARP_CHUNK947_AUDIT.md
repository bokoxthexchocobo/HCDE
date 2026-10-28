# Chunk 947 — `DLevelScript::RunScript` `PCD_CHECKACTORINVENTORY` tail through `PCD_CANCELFADE` from `p_acs.cpp` (**9750–9899**). Follows **946** (**≤9749**); avoids **8850–8999** credited corridor.

## Scope

`DLevelScript::RunScript` `PCD_CHECKACTORINVENTORY` tail through `PCD_CANCELFADE` from `p_acs.cpp` (**9750–9899**). Follows **946** (**≤9749**); avoids **8850–8999** credited corridor.

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::RunScript** 9750–9899 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsVmOpcode947Slice.cs`
Managed LOC (tally): 98.
Conversion commit: `6a4bf3bbb6b8b86d02c7653a6e4c81218ad14821`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native opcode region | Behavior ported |
|----------------------|-----------------|
| `PCD_CHECKACTORINVENTORY` tail / `CHECKINVENTORYDIRECT` (**9750–9754**) | yes |
| `PCD_USEINVENTORY` / `USEACTORINVENTORY` (**9757–9780**) | yes |
| `PCD_GETSIGILPIECES` (**9783–9795**) | yes |
| `PCD_GETAMMOCAPACITY` / `SETAMMOCAPACITY` (**9798–9849**) | yes |
| `PCD_SETMUSIC` / `DIRECT` / `LOCAL*` (**9852–9875**) | yes |
| `PCD_FADETO` / `FADERANGE` (**9878–9886**) | yes |
| `PCD_CANCELFADE` through cancel loop (**9889–9899**) | yes |

| Managed (`VmPAcsVmOpcode947Slice.cs`) | Native (`p_acs.cpp`) |
|----------------------------------------|---------------------|
| `CheckInventoryDirectPcAfterRead` | **9752–9754** |
| `UseActorInventoryUsesWorld` / `AccumulateUseInventoryResult` | **9765–9778** |
| `SigilPiecesWhenMissing` / `ActivatorHasSigil` | **9787–9793** |
| `TypeParentIsAmmo` / `AmmoMaxFromOwnedItem` | **9803–9813** |
| `SetAmmoCapacity*` / `GrantedAmmoStartingAmount` | **9826–9849** |
| `SetMusic*` / `ShouldApplyLocalSetMusic` | **9852–9875** |
| `ApplyFadeToStackPop` / `ApplyFadeRangeStackPop` | **9878–9886** |
| `CancelFadeForAllPlayers` / `ShouldCancelFader` | **9896–9899** |
| `PlayMovieDisabledResult` | **9904–9905** (deferred past hull) |
| `SetsMusic` | **9852–9875** |

### Chunk **947** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**946** |


**Post-review ratio:** 98/150 managed/native ≈ 65% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsVmOpcode947SliceTests.CheckInventoryDirectPcAfterRead_AdvancesOneWord`
- `VmPAcsVmOpcode947SliceTests.UseActorInventoryUsesWorld_WhenTidZero`
- `VmPAcsVmOpcode947SliceTests.AccumulateUseInventoryResult_SumsUses`
- `VmPAcsVmOpcode947SliceTests.ResolveAmmoCapacity_UsesDefaultWhenNotOwned`
- `VmPAcsVmOpcode947SliceTests.ResolveSigilPieces_ReturnsZeroWithoutSigil`
- `VmPAcsVmOpcode947SliceTests.InventoryAmountAfterGrant_ClearsPrevious`
- `VmPAcsVmOpcode947SliceTests.ApplySetMusicStackPop_DropsThreeSlots`
- `VmPAcsVmOpcode947SliceTests.ShouldCancelFader_WhenActivatorNull`
- `VmPAcsVmOpcode947SliceTests.ResolvePlayMovieStackValue_ForcesNegativeOne`
- `VmPAcsVmOpcode947SliceTests.SetsMusic_IncludesLocalDirectOpcode`
- `VmPAcsVmOpcode947SliceTests.ApplyFadeRangeStackPop_DropsNineSlots`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 947 --to 947` (0 hard / 0 soft).

## Seams

`CheckInventory`, `UseInventory`, `S_ChangeMusic`, `DoFadeTo`/`DoFadeRange`, and `DFlashFader` iteration remain host seams.

## Credit rationale

Honest **150**-line UNCredited band with zero tally overlap; exclusive `VmPAcsVmOpcode947Slice.cs` (`AcsVm` handlers are not matrix credit).
