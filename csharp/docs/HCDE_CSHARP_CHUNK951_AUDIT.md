# Chunk 951 — `addDefered` tail, `P_StartScript`, suspend/terminate routing, deferred serialize, `scriptstat`, and `DumpScriptStatus` head from `p_acs.cpp` (**10950–11099**). Follows **950** (**≤10949**); avoids **10050–10199** credited corridor.

## Scope

`addDefered` tail, `P_StartScript`, suspend/terminate routing, deferred serialize, `scriptstat`, and `DumpScriptStatus` head from `p_acs.cpp` (**10950–11099**). Follows **950** (**≤10949**); avoids **10050–10199** credited corridor.

## Native ranges

- `src/playsim/p_acs.cpp` **P_StartScript** 10950–11099 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsStartScript951Slice.cs`
Managed LOC (tally): 142.
Conversion commit: `578afcf39d3e60de277c80aca9c89698b613efec`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `addDefered` tail (**10950–10953**) | yes |
| `allowsingleplayerscripts` / `sv_allowallscripts` CVAR lines (**10957–10958**) | seam (config host) |
| `P_StartScript` (**10960–11037**) | yes |
| `P_SuspendScript` / `P_TerminateScript` (**11039–11053**) | yes |
| `Serialize(acsdefered_t)` (**11055–11066**) | yes |
| `CCMD scriptstat` + `DumpScriptStatus` head (**11068–11099**) | yes |

| Managed (`VmPAcsStartScript951Slice.cs`) | Native (`p_acs.cpp`) |
|------------------------------------------------|---------------------|
| `MapNamesMatchForScript` / `ShouldDeferStartOnOtherMap` | **10962–10963**, **11029–11033** |
| `DenyNonNetScriptRequest` / denial logging gates | **10981–11005** |
| `StartScriptReturnsRunResult` / `StartScriptResultWhenRunning` | **11013–11018** |
| `ShouldPrintUnknownScript` | **11023–11026** |
| `DeferredExecuteTypeForStartFlags` | **11031–11033** |
| `SuspendOrTerminateOnOtherMap` / defer types / local states | **11041–11052** |
| `DeferredSerialize*` / arg slot padding | **11057–11063** |
| `LevelHasNoAcsThinker` / `DumpScriptStatusHasNext` | **11073–11099** |
| `ScriptStateName*` | **11086–11101** (names through band end) |
| `ResolveDeferredPlayerNumOnAdd` | **10944–10951** |

### Chunk **951** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**950** |


**Post-review ratio:** 142/150 managed/native ≈ 95% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsStartScript951SliceTests.MapNamesMatchForScript_IgnoresCaseWithinEightChars`
- `VmPAcsStartScript951SliceTests.ShouldDeferStartOnOtherMap_WhenTargetDiffers`
- `VmPAcsStartScript951SliceTests.DenyNonNetScriptRequest_BlocksNetPukeInNetgame`
- `VmPAcsStartScript951SliceTests.DenyNonNetScriptRequest_AllowsLocalConsoleRequester`
- `VmPAcsStartScript951SliceTests.DeferredExecuteTypeForStartFlags_UsesAlwaysDefer`
- `VmPAcsStartScript951SliceTests.StartScriptResultWhenRunning_ReturnsRunScriptWhenWantResult`
- `VmPAcsStartScript951SliceTests.SuspendOrTerminateOnOtherMap_WhenMapsDiffer`
- `VmPAcsStartScript951SliceTests.ScriptStateNameIndex_AlignsWithNativeStateNames`
- `VmPAcsStartScript951SliceTests.ResolveDeferredPlayerNumOnAdd_MissingWithoutPlayer`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 951 --to 951` (0 hard / 0 soft).

## Seams

`Behaviors.FindScript`, `P_GetScriptGoing`, `RunScript`, `FindLevelInfo`, `addDefered`, `SetScriptState`, `FSerializer`, and `AllLevels` iteration remain host seams.

## Credit rationale

Honest **150**-line UNCredited band with zero tally overlap; exclusive `VmPAcsStartScript951Slice.cs` (hosted `AcsVm` / ACS thinker dumps are not matrix credit).
