# Chunk 948 — `DLevelScript::RunScript` `PCD_CANCELFADE` tail through `PCD_STARTTRANSLATION` from `p_acs.cpp` (**9900–10049**). Follows **947** (**≤9899**); avoids **8850–8999** credited corridor.

## Scope

`DLevelScript::RunScript` `PCD_CANCELFADE` tail through `PCD_STARTTRANSLATION` from `p_acs.cpp` (**9900–10049**). Follows **947** (**≤9899**); avoids **8850–8999** credited corridor.

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::RunScript** 9900–10049 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsVmOpcode948Slice.cs`
Managed LOC (tally): 84.
Conversion commit: `ebcf1ebd7907edc9fa8dc634ae52ded43ff60743`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native opcode region | Behavior ported |
|----------------------|-----------------|
| `PCD_CANCELFADE` tail (**9900–9902**) | yes |
| `PCD_PLAYMOVIE` (**9904–9905**) | yes |
| `PCD_SETACTORPOSITION` (**9908–9916**) | yes |
| `PCD_GETACTORX/Y/Z` (**9919–9936**) | yes |
| `PCD_GETACTORFLOORZ/CEILINGZ` / angle / pitch (**9939–9964**) | yes |
| `PCD_GETLINEROWOFFSET` (**9967–9975**) | yes |
| `PCD_GETSECTORFLOORZ/CEILINGZ` (**9978–10009**) | yes |
| `PCD_GETSECTORLIGHTLEVEL` (**10012–10022**) | yes |
| `PCD_SETFLOORTRIGGER` / `SETCEILINGTRIGGER` (**10025–10036**) | yes |
| `PCD_STARTTRANSLATION` head (**10039–10049**) | yes |

| Managed (`VmPAcsVmOpcode948Slice.cs`) | Native (`p_acs.cpp`) |
|----------------------------------------|---------------------|
| `ResolvePlayMovieStackValue` | **9904–9905** |
| `SetActorPosition*` / `AcsToDouble` | **9908–9916** |
| `ResolveActorAcsScalar` / `ResolveActorZWithBob` / `SelectActorAxisAcs` | **9919–9964** |
| `LineRowOffsetWhenActivationMissing` | **9967–9975** |
| `SectorTagIndex` / `SelectSectorPlaneHeight` | **9978–10009** |
| `ResolveSectorLightLevel` | **10012–10022** |
| `IsCeilingPlaneWatcher` / `ApplyPlaneWatcherStackPop` | **10025–10036** |
| `TranslationIndexInRange` / `TranslationTableSlot` | **10039–10049** |

### Chunk **948** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**947** |


**Post-review ratio:** 84/150 managed/native ≈ 56% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsVmOpcode948SliceTests.ResolvePlayMovieStackValue_ForcesNegativeOne`
- `VmPAcsVmOpcode948SliceTests.SetActorPositionInterpolate_NonZeroIsTrue`
- `VmPAcsVmOpcode948SliceTests.ResolveActorZWithBob_IncludesBobOffset`
- `VmPAcsVmOpcode948SliceTests.SelectActorAxisAcs_UsesYForGetActorY`
- `VmPAcsVmOpcode948SliceTests.SectorHeightUsesPointInSector_WhenTagZero`
- `VmPAcsVmOpcode948SliceTests.ResolveSectorLightLevel_ReturnsNegativeOneWhenMissing`
- `VmPAcsVmOpcode948SliceTests.IsCeilingPlaneWatcher_ForSetCeilingTrigger`
- `VmPAcsVmOpcode948SliceTests.TranslationTableSlot_IsZeroBased`
- `VmPAcsVmOpcode948SliceTests.ApplyPlaneWatcherStackPop_DropsEightSlots`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 948 --to 948` (0 hard / 0 soft).

## Seams

`P_MoveThing`, sector plane queries, `DPlaneWatcher`, `FRemapTable`, and linedef texture offsets remain host seams.

## Credit rationale

Honest **150**-line UNCredited band with zero tally overlap; exclusive `VmPAcsVmOpcode948Slice.cs` (`AcsVm` handlers are not matrix credit).
