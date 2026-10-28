# Chunk 949 — `DLevelScript::RunScript` `STRCPYTO*CHRANGE` tail, `PCD_CONSOLECOMMAND*`, divide/modulus epilogue, and `P_GetScriptGoing` head from `p_acs.cpp` (**10650–10799**). Follows **948** (**≤10049**); does not hull **10050–10199** (credited).

## Scope

`DLevelScript::RunScript` `STRCPYTO*CHRANGE` tail, `PCD_CONSOLECOMMAND*`, divide/modulus epilogue, and `P_GetScriptGoing` head from `p_acs.cpp` (**10650–10799**). Follows **948** (**≤10049**); does not hull **10050–10199** (credited).

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::RunScript** 10650–10799 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsRunscriptStrcpyEpilogue949Slice.cs`
Managed LOC (tally): 80.
Conversion commit: `1819ec002d0c9d22bd1b8a8d1a82f3bfba727dc1`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `STRCPYTO*CHRANGE` lookup/copy loop tail (**10650–10723**) | yes |
| `PCD_CONSOLECOMMAND` / `DIRECT` (**10726–10734**) | yes |
| `RunScript` divide/modulus + `PleaseRemove` (**10738–10776**) | yes |
| `RunScript` return + `P_GetScriptGoing` head (**10777–10799**) | yes |

| Managed (`VmPAcsRunscriptStrcpyEpilogue949Slice.cs`) | Native (`p_acs.cpp`) |
|------------------------------------------------------|---------------------|
| `StrCpyDestinationIndex` / `StrCpyRangeArgsInvalid` | **10649–10647** |
| `StrCpyCompletesWhenLookupEmpty` / `AdvanceStringOffset` | **10651–10668** |
| `StrCpySuccessWhenOnlyTerminatorRemains` / opcode predicates | **10670–10720** |
| `ApplyStrCpyRangeStackPop` | **10722–10723** |
| `ConsoleCommand*` stack/PC helpers | **10726–10734** |
| `ShouldRestoreSavedBehavior` / `StateAfter*` | **10740–10766** |
| `ShouldUnlinkRunningScript` / `RunScriptEndsWithEmptyStack` | **10767–10780** |
| `ShouldIgnoreClientSideScriptStart` / `ResumeSuspendedRunningScript` | **10792–10805** |

### Chunk **949** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**948** |


**Post-review ratio:** 80/150 managed/native ≈ 53% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsRunscriptStrcpyEpilogue949SliceTests.StrCpyDestinationIndex_AddsOffset`
- `VmPAcsRunscriptStrcpyEpilogue949SliceTests.StrCpyRangeArgsInvalid_WhenSourceOffsetNegative`
- `VmPAcsRunscriptStrcpyEpilogue949SliceTests.MapCharRangeArrayWritable_RequiresPositiveIdAndMapVar`
- `VmPAcsRunscriptStrcpyEpilogue949SliceTests.IsStrCpyToScriptCharRange_MatchesOpcode`
- `VmPAcsRunscriptStrcpyEpilogue949SliceTests.ApplyConsoleCommandStackPop_DropsThreeSlots`
- `VmPAcsRunscriptStrcpyEpilogue949SliceTests.ShouldRestoreSavedBehavior_OnDivideByZero`
- `VmPAcsRunscriptStrcpyEpilogue949SliceTests.ResumeSuspendedRunningScript_WhenSuspended`
- `VmPAcsRunscriptStrcpyEpilogue949SliceTests.SkipNewScriptWhenAlreadyRunning_WithoutAlwaysFlag`
- `VmPAcsRunscriptStrcpyEpilogue949SliceTests.AdvanceStringOffset_ClampsPastEnd`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 949 --to 949` (0 hard / 0 soft).

## Seams

`localarrays->Set`, `CopyStringToArray`, `ACS_WorldArrays`/`ACS_GlobalArrays`, `ACS_RunConsoleCommandString`, and `Create<DLevelScript>` remain host seams.

## Credit rationale

Honest **150**-line UNCredited band with zero tally overlap; exclusive `VmPAcsRunscriptStrcpyEpilogue949Slice.cs` (`AcsVm` / **925** console helpers are not matrix credit for this hull).
