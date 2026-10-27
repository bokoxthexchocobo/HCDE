# Chunk 912 — `DLevelScript::RunScript` `PCD_GT`…`PCD_GE` and script/map/world/global var/array assign/push/add through `PCD_ADDGLOBALARRAY` from `p_acs.cpp` (**7710–7874**). Follows **911** (**≤7709**); `PCD_SUBSCRIPTVAR` at **7876** deferred.

## Scope

`DLevelScript::RunScript` `PCD_GT`…`PCD_GE` and script/map/world/global var/array assign/push/add through `PCD_ADDGLOBALARRAY` from `p_acs.cpp` (**7710–7874**). Follows **911** (**≤7709**); `PCD_SUBSCRIPTVAR` at **7876** deferred.

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::RunScript** 7710–7874 (165 lines)

**Credited native LOC:** 165 (tally `nativeLoc` 165).

## Managed

`VmPAcsVmOpcode912Slice.cs`
Managed LOC (tally): 232.
Conversion commit: `4395f2430c75452273281747fdf4640d2735b315`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native opcode region | Behavior ported |
|----------------------|-----------------|
| `PCD_GT` / `PCD_LE` / `PCD_GE` (**7710–7748**) | yes |
| `PCD_ASSIGNSCRIPTVAR` … `PCD_ASSIGNGLOBALARRAY` (**7750–7789**) | yes |
| `PCD_PUSHSCRIPTVAR` … `PCD_PUSHGLOBALARRAY` (**7791–7821**) | yes |
| `PCD_ADDSCRIPTVAR` … `PCD_ADDGLOBALARRAY` (**7823–7874**) | yes |

| Managed (`VmPAcsVmOpcode912Slice.cs`) | Native (`p_acs.cpp`) |
|----------------------------------------|---------------------|
| `TryApplyGt` | **7711–7722** |
| `TryApplyLe` | **7724–7735** |
| `TryApplyGe` | **7737–7748** |
| `AssignScriptVar` / `AssignMapVar` / `AssignWorldVar` / `AssignGlobalVar` | **7750–7769** |
| `AssignScriptArray` / `AssignMapArray` / `AssignWorldArray` / `AssignGlobalArray` | **7771–7789** |
| `PushScriptVar` … `PushGlobalArray` | **7791–7821** |
| `AddScriptVar` … `AddGlobalVar` | **7823–7841** |
| `AddScriptArray` / `AddMapArray` / `AddWorldArray` / `AddGlobalArray` | **7843–7874** |
| `AcsScriptArrayStore` / `AcsMapArrayStore` | array get/set backing **7772–7872** |

### Chunk **912** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **165** |
| **Net headline** | **+165** vs post-**911** |


**Post-review ratio:** 232/165 managed/native ≈ 141% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsVmOpcode912SliceTests.TryApplyGe_ComparesSecondToTop`
- `VmPAcsVmOpcode912SliceTests.TryApplyGt_MalformedWhenStackTooShallow`
- `VmPAcsVmOpcode912SliceTests.AssignScriptVar_PopsStackTopIntoLocal`
- `VmPAcsVmOpcode912SliceTests.PushWorldVar_PushesStoredValue`
- `VmPAcsVmOpcode912SliceTests.AssignScriptArray_WritesIndexedCell`
- `VmPAcsVmOpcode912SliceTests.PushMapArray_ResolvesArrayThroughMapVarCell`
- `VmPAcsVmOpcode912SliceTests.AddGlobalVar_IncrementsSlot`
- `VmPAcsVmOpcode912SliceTests.AddWorldArray_AddsToIndexedElement`
- `VmPAcsVmOpcode912SliceTests.AddMapArray_UsesMapVarAsArrayId`
- `VmPAcsVmOpcode912SliceTests.TryApplyLe_ReturnsOneWhenLessOrEqual`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 912 --to 912` (0 hard / 0 soft).

## Seams

`ACSLocalVariables`, `ACSLocalArrays`, `FBehavior::MapVars`, and full `RunScript` dispatch are not hosted; slice models stack pop/push and storage cell updates only.

## Credit rationale

Honest **165**-line UNCredited band; exclusive `VmPAcsVmOpcode912Slice.cs` without modifying **911** `VmPAcsVmOpcode911Slice.cs`.
