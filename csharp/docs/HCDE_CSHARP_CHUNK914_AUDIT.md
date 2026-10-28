# Chunk 914 — `DLevelScript::RunScript` `PCD_DIVMAPARRAY`…`PCD_DIVGLOBALARRAY`, full `PCD_MOD*`, and `PCD_ANDSCRIPTVAR`…`PCD_ANDGLOBALVAR` from `p_acs.cpp` (**8043–8203**). Follows **913** (**≤8041**); `PCD_ANDSCRIPTARRAY` at **8205** deferred.

## Scope

`DLevelScript::RunScript` `PCD_DIVMAPARRAY`…`PCD_DIVGLOBALARRAY`, full `PCD_MOD*`, and `PCD_ANDSCRIPTVAR`…`PCD_ANDGLOBALVAR` from `p_acs.cpp` (**8043–8203**). Follows **913** (**≤8041**); `PCD_ANDSCRIPTARRAY` at **8205** deferred.

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::RunScript** 8043–8203 (161 lines)

**Credited native LOC:** 161 (tally `nativeLoc` 161).

## Managed

`VmPAcsVmOpcode914Slice.cs`
Managed LOC (tally): 188.
Conversion commit: `e56fdaab1546155e48b070b3cb3d8ea4d566203a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native opcode region | Behavior ported |
|----------------------|-----------------|
| `PCD_DIVMAPARRAY` … `PCD_DIVGLOBALARRAY` (**8043–8081**) | yes |
| `PCD_MODSCRIPTVAR` … `PCD_MODGLOBALARRAY` (**8083–8182**) | yes |
| `PCD_ANDSCRIPTVAR` … `PCD_ANDGLOBALVAR` (**8185–8203**) | yes |

| Managed (`VmPAcsVmOpcode914Slice.cs`) | Native (`p_acs.cpp`) |
|----------------------------------------|---------------------|
| `DivMapArray` | **8043–8055** |
| `DivWorldArray` | **8057–8068** |
| `DivGlobalArray` | **8070–8081** |
| `ModScriptVar` … `ModGlobalVar` | **8083–8129** |
| `ModScriptArray` … `ModGlobalArray` | **8131–8182** |
| `AndScriptVar` … `AndGlobalVar` | **8185–8203** |
| `AcsScriptArrayStore` / `AcsMapArrayStore` | array backing **8139–8179** |

### Chunk **914** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **161** |
| **Net headline** | **+161** vs post-**913** |


**Post-review ratio:** 188/161 managed/native ≈ 117% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsVmOpcode914SliceTests.DivWorldArray_DividesElement`
- `VmPAcsVmOpcode914SliceTests.DivMapArray_ReturnsDivideByZero`
- `VmPAcsVmOpcode914SliceTests.ModScriptVar_ReturnsModulusByZero`
- `VmPAcsVmOpcode914SliceTests.ModGlobalVar_AppliesModulo`
- `VmPAcsVmOpcode914SliceTests.ModScriptArray_UpdatesCell`
- `VmPAcsVmOpcode914SliceTests.ModMapArray_UsesMapVarCellAsArrayId`
- `VmPAcsVmOpcode914SliceTests.DivGlobalArray_DetectsZeroDivisor`
- `VmPAcsVmOpcode914SliceTests.AndWorldVar_BitwiseAndsStackTop`
- `VmPAcsVmOpcode914SliceTests.AndMapVar_ClearsBits`
- `VmPAcsVmOpcode914SliceTests.ModWorldArray_ModulusOnElement`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 914 --to 914` (0 hard / 0 soft).

## Seams

`FBehavior::SetArrayVal` / `GetArrayVal` and full `RunScript` loop are not hosted; slice models zero-divisor script states and stack operand layout only.

## Credit rationale

Honest **161**-line UNCredited band; exclusive `VmPAcsVmOpcode914Slice.cs` without modifying **911–913** slice files.
