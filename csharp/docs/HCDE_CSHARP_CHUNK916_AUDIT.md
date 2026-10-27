# Chunk 916 — `DLevelScript::RunScript` `PCD_LSSCRIPTARRAY`…`LSGLOBALARRAY`, full `PCD_RS*`, `PCD_INC*` through `INCGLOBALARRAY`, and `PCD_DECSCRIPTVAR`…`DECGLOBALVAR` from `p_acs.cpp` (**8365–8514**). Follows **915** (**≤8363**); `PCD_DECSCRIPTARRAY` at **8517** deferred.

## Scope

`DLevelScript::RunScript` `PCD_LSSCRIPTARRAY`…`LSGLOBALARRAY`, full `PCD_RS*`, `PCD_INC*` through `INCGLOBALARRAY`, and `PCD_DECSCRIPTVAR`…`DECGLOBALVAR` from `p_acs.cpp` (**8365–8514**). Follows **915** (**≤8363**); `PCD_DECSCRIPTARRAY` at **8517** deferred.

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::RunScript** 8365–8514 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsVmOpcode916Slice.cs`
Managed LOC (tally): 174.
Conversion commit: `8248a0fe45aa45afd23f37fe920fb2e8bab398ea`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native opcode region | Behavior ported |
|----------------------|-----------------|
| `PCD_LSSCRIPTARRAY` … `PCD_LSGLOBALARRAY` (**8365–8396**) | yes |
| `PCD_RSSCRIPTVAR` … `PCD_RSGLOBALARRAY` (**8398–8449**) | yes |
| `PCD_INCSCRIPTVAR` … `PCD_INCGLOBALARRAY` (**8452–8499**) | yes |
| `PCD_DECSCRIPTVAR` … `PCD_DECGLOBALVAR` (**8501–8514**) | yes |

| Managed (`VmPAcsVmOpcode916Slice.cs`) | Native (`p_acs.cpp`) |
|----------------------------------------|---------------------|
| `LsScriptArray` … `LsGlobalArray` | **8365–8396** |
| `RsScriptVar` … `RsGlobalArray` | **8398–8449** |
| `IncScriptVar` … `IncGlobalArray` | **8452–8499** |
| `DecScriptVar` … `DecGlobalVar` | **8501–8514** |
| `AcsScriptArrayStore` / `AcsMapArrayStore` | array backing **8368–8496** |

### Chunk **916** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**915** |


**Post-review ratio:** 174/150 managed/native ≈ 116% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsVmOpcode916SliceTests.LsScriptArray_ShiftsCellLeft`
- `VmPAcsVmOpcode916SliceTests.RsGlobalVar_RightShiftsLocal`
- `VmPAcsVmOpcode916SliceTests.RsWorldArray_ShiftsElement`
- `VmPAcsVmOpcode916SliceTests.IncScriptVar_Increments`
- `VmPAcsVmOpcode916SliceTests.IncMapArray_ResolvesArrayId`
- `VmPAcsVmOpcode916SliceTests.DecWorldVar_Decrements`
- `VmPAcsVmOpcode916SliceTests.LsMapArray_ShiftsThroughMapVar`
- `VmPAcsVmOpcode916SliceTests.RsMapArray_RightShiftsCell`
- `VmPAcsVmOpcode916SliceTests.IncGlobalArray_IncrementsElement`
- `VmPAcsVmOpcode916SliceTests.DecGlobalVar_DecrementsSlot`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 916 --to 916` (0 hard / 0 soft).

## Seams

`FBehavior::SetArrayVal` / `GetArrayVal` and full `RunScript` dispatch are not hosted; slice models stack pops and in-place shift/inc/dec only.

## Credit rationale

Honest **150**-line UNCredited band (exact case-edge trim, no LOC top-up); exclusive `VmPAcsVmOpcode916Slice.cs` without modifying **911–915** slice files.
