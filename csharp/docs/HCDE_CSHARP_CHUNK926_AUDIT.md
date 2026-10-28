# Chunk 926 — `CharArrayParms` tail (**7066–7085**), `GetClassForIndex`, and `DLevelScript::RunScript` preamble through first `PCD_NOP`/`PCD_SUSPEND` from `p_acs.cpp` (**7066–7215**). Follows **925** (**≤7065**); `PCD_TAGSTRING` at **7228** deferred.

## Scope

`CharArrayParms` tail (**7066–7085**), `GetClassForIndex`, and `DLevelScript::RunScript` preamble through first `PCD_NOP`/`PCD_SUSPEND` from `p_acs.cpp` (**7066–7215**). Follows **925** (**≤7065**); `PCD_TAGSTRING` at **7228** deferred.

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::RunScript** 7066–7215 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsRunscript926Slice.cs`
Managed LOC (tally): 151.
Conversion commit: `78d4615ff730f4d2d9273739ee20e5b06330d257`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `CharArrayParms` ranged/flat tail (**7066–7085**) | yes |
| `GetClassForIndex` (**7087–7090**) | yes (lookup seam) |
| `RunScript` locals/module ptr (**7092–7111**) | yes |
| Hexen special-arg mask (**7113–7114**) | yes |
| Pre-run wait states (**7117–7167**) | yes |
| Stack/`RunScript` loop head (**7170–7208**) | yes |
| `default`/`PCD_TERMINATE`/`PCD_NOP`/`PCD_SUSPEND` (**7210–7226**) | yes |

| Managed (`VmPAcsRunscript926Slice.cs`) | Native (`p_acs.cpp`) |
|------------------------------------------|---------------------|
| `TryPopCharArrayParms` | **7066–7085** |
| `LookupActorClassName` | **7087–7090** |
| `ComputeSpecialArgMask` | **7113–7114** |
| `TrySelectModuleLocalArrays` | **7103–7111** |
| `TickPreRunWaitState` | **7117–7167** |
| `ShouldTerminateRunaway` | **7188–7192** |
| `ReadNextPcode` | **7195–7206** |
| `ApplyRunningOpcode` | **7210–7226** |
| `DefaultResultValue` / `RunawayInstructionLimit` | **7100**, **7188** |

### Chunk **926** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**925** |


**Post-review ratio:** 151/150 managed/native ≈ 101% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsRunscript926SliceTests.TryPopCharArrayParms_Ranged_RejectsInvalidCapacity`
- `VmPAcsRunscript926SliceTests.TryPopCharArrayParms_NonRanged_CombinesOffset`
- `VmPAcsRunscript926SliceTests.ComputeSpecialArgMask_HexenOld_TruncatesToByte`
- `VmPAcsRunscript926SliceTests.TickPreRunWaitState_DelayedBecomesRunningWhenZero`
- `VmPAcsRunscript926SliceTests.TickPreRunWaitState_TagWaitReturnsWhileSectorBusy`
- `VmPAcsRunscript926SliceTests.ReadNextPcode_LittleEnhanced_DecodesEscapedOpcode`
- `VmPAcsRunscript926SliceTests.ApplyRunningOpcode_SuspendSetsSuspended`
- `VmPAcsRunscript926SliceTests.ShouldTerminateRunaway_RespectsBusyFlag`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 926 --to 926` (0 hard / 0 soft).

## Seams

`ACSLocalVariables`, sector tag iterators, `PO_Busy`, `RunningScripts`, `Printf`, and full opcode dispatch remain host seams.

## Credit rationale

Honest **150**-line UNCredited band in **6614–7509** corridor; exclusive `VmPAcsRunscript926Slice.cs`.
