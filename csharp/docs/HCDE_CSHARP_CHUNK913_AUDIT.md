# Chunk 913 — `DLevelScript::RunScript` `PCD_SUB*` / `PCD_MUL*` / `PCD_DIVSCRIPTVAR`…`PCD_DIVSCRIPTARRAY` from `p_acs.cpp` (**7876–8041**). Follows **912** (**≤7874**); `PCD_DIVMAPARRAY` at **8043** deferred.

## Scope

`DLevelScript::RunScript` `PCD_SUB*` / `PCD_MUL*` / `PCD_DIVSCRIPTVAR`…`PCD_DIVSCRIPTARRAY` from `p_acs.cpp` (**7876–8041**). Follows **912** (**≤7874**); `PCD_DIVMAPARRAY` at **8043** deferred.

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::RunScript** 7876–8041 (166 lines)

**Credited native LOC:** 166 (tally `nativeLoc` 166).

## Managed

`VmPAcsVmOpcode913Slice.cs`
Managed LOC (tally): 205.
Conversion commit: `f032d63738ccf842eadd50cb35b43a08fba16f5e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native opcode region | Behavior ported |
|----------------------|-----------------|
| `PCD_SUBSCRIPTVAR` … `PCD_SUBGLOBALARRAY` (**7876–7927**) | yes |
| `PCD_MULSCRIPTVAR` … `PCD_MULGLOBALARRAY` (**7929–7980**) | yes |
| `PCD_DIVSCRIPTVAR` … `PCD_DIVGLOBALVAR` (**7982–8028**) | yes |
| `PCD_DIVSCRIPTARRAY` (**8030–8041**) | yes |

| Managed (`VmPAcsVmOpcode913Slice.cs`) | Native (`p_acs.cpp`) |
|----------------------------------------|---------------------|
| `SubScriptVar` … `SubGlobalVar` | **7876–7894** |
| `SubScriptArray` … `SubGlobalArray` | **7896–7927** |
| `MulScriptVar` … `MulGlobalVar` | **7929–7947** |
| `MulScriptArray` … `MulGlobalArray` | **7949–7980** |
| `DivScriptVar` … `DivGlobalVar` | **7982–8028** |
| `DivScriptArray` | **8030–8041** |
| `AcsScriptArrayStore` / `AcsMapArrayStore` | array backing **7899–8038** |

### Chunk **913** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **166** |
| **Net headline** | **+166** vs post-**912** |


**Post-review ratio:** 205/166 managed/native ≈ 123% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsVmOpcode913SliceTests.SubScriptVar_DecrementsLocal`
- `VmPAcsVmOpcode913SliceTests.SubGlobalArray_SubtractsIndexedElement`
- `VmPAcsVmOpcode913SliceTests.MulWorldVar_MultipliesSlot`
- `VmPAcsVmOpcode913SliceTests.MulMapArray_UsesMapVarAsArrayId`
- `VmPAcsVmOpcode913SliceTests.DivGlobalVar_ReturnsDivideByZero`
- `VmPAcsVmOpcode913SliceTests.DivWorldVar_DividesAndPops`
- `VmPAcsVmOpcode913SliceTests.DivScriptArray_DividesCell`
- `VmPAcsVmOpcode913SliceTests.SubMapVar_DecrementsMapSlot`
- `VmPAcsVmOpcode913SliceTests.MulScriptArray_MultipliesCell`
- `VmPAcsVmOpcode913SliceTests.DivMapVar_DetectsZeroDivisor`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 913 --to 913` (0 hard / 0 soft).

## Seams

`ACSLocalArrays`, `FBehavior::SetArrayVal`, and full `RunScript` dispatch are not hosted; slice models stack operands and `SCRIPT_DivideBy0` on zero divisors for DIV opcodes.

## Credit rationale

Honest **166**-line UNCredited band; exclusive `VmPAcsVmOpcode913Slice.cs` without modifying **911**/**912** slice files.
