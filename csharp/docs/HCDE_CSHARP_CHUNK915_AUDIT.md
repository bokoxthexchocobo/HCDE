# Chunk 915 — `DLevelScript::RunScript` `PCD_ANDSCRIPTARRAY`…`ANDGLOBALARRAY`, full `PCD_EOR*` / `PCD_OR*`, and `PCD_LSSCRIPTVAR`…`LSGLOBALVAR` from `p_acs.cpp` (**8205–8363**). Follows **914** (**≤8203**); `PCD_LSSCRIPTARRAY` at **8365** deferred.

## Scope

`DLevelScript::RunScript` `PCD_ANDSCRIPTARRAY`…`ANDGLOBALARRAY`, full `PCD_EOR*` / `PCD_OR*`, and `PCD_LSSCRIPTVAR`…`LSGLOBALVAR` from `p_acs.cpp` (**8205–8363**). Follows **914** (**≤8203**); `PCD_LSSCRIPTARRAY` at **8365** deferred.

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::RunScript** 8205–8363 (159 lines)

**Credited native LOC:** 159 (tally `nativeLoc` 159).

## Managed

`VmPAcsVmOpcode915Slice.cs`
Managed LOC (tally): 208.
Conversion commit: `2b66dc22196cd792b25e2ac65f3d925cc57c3e86`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native opcode region | Behavior ported |
|----------------------|-----------------|
| `PCD_ANDSCRIPTARRAY` … `PCD_ANDGLOBALARRAY` (**8205–8236**) | yes |
| `PCD_EORSCRIPTVAR` … `PCD_EORGLOBALARRAY` (**8238–8289**) | yes |
| `PCD_ORSCRIPTVAR` … `PCD_ORGLOBALARRAY` (**8291–8343**) | yes |
| `PCD_LSSCRIPTVAR` … `PCD_LSGLOBALVAR` (**8345–8363**) | yes |

| Managed (`VmPAcsVmOpcode915Slice.cs`) | Native (`p_acs.cpp`) |
|----------------------------------------|---------------------|
| `AndScriptArray` … `AndGlobalArray` | **8205–8236** |
| `EorScriptVar` … `EorGlobalArray` | **8238–8289** |
| `OrScriptVar` … `OrGlobalArray` | **8291–8343** |
| `LsScriptVar` … `LsGlobalVar` | **8345–8363** |
| `AcsScriptArrayStore` / `AcsMapArrayStore` | array backing **8208–8340** |

### Chunk **915** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **159** |
| **Net headline** | **+159** vs post-**914** |


**Post-review ratio:** 208/159 managed/native ≈ 131% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsVmOpcode915SliceTests.AndScriptArray_BitwiseAndsCell`
- `VmPAcsVmOpcode915SliceTests.EorGlobalVar_TogglesBits`
- `VmPAcsVmOpcode915SliceTests.OrWorldArray_OrCombinesElement`
- `VmPAcsVmOpcode915SliceTests.OrMapArray_ResolvesArrayThroughMapVar`
- `VmPAcsVmOpcode915SliceTests.LsScriptVar_ShiftsLocalLeft`
- `VmPAcsVmOpcode915SliceTests.LsGlobalVar_ShiftsByStackAmount`
- `VmPAcsVmOpcode915SliceTests.AndGlobalArray_AndsInPlace`
- `VmPAcsVmOpcode915SliceTests.EorScriptArray_UpdatesStoredValue`
- `VmPAcsVmOpcode915SliceTests.OrScriptVar_MergesBits`
- `VmPAcsVmOpcode915SliceTests.LsMapVar_LeftShiftsMapSlot`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 915 --to 915` (0 hard / 0 soft).

## Seams

`FBehavior::SetArrayVal` / `GetArrayVal` and full `RunScript` dispatch are not hosted; slice models stack operand layout and in-place bitwise/shift updates only.

## Credit rationale

Honest **159**-line UNCredited band; exclusive `VmPAcsVmOpcode915Slice.cs` without modifying **911–914** slice files.
