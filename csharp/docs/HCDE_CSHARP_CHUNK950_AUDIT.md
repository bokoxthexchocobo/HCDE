# Chunk 950 — `P_GetScriptGoing` tail, `DLevelScript` constructor, `SetScriptState`, `DoDeferedScripts`, and `addDefered` head from `p_acs.cpp` (**10800–10949**). Follows **949** (**≤10799**); avoids **10050–10199** credited corridor.

## Scope

`P_GetScriptGoing` tail, `DLevelScript` constructor, `SetScriptState`, `DoDeferedScripts`, and `addDefered` head from `p_acs.cpp` (**10800–10949**). Follows **949** (**≤10799**); avoids **10050–10199** credited corridor.

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::DLevelScript** 10800–10949 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsDLevelScriptCtor950Slice.cs`
Managed LOC (tally): 75.
Conversion commit: `80f5a16212044129dfcea0db7b34446abaa0a505`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `P_GetScriptGoing` resume/abort tail (**10800–10809**) | yes |
| `DLevelScript::DLevelScript` (**10812–10866**) | yes |
| `SetScriptState` (**10869–10883**) | yes |
| `FLevelLocals::DoDeferedScripts` (**10885–10924**) | yes |
| `addDefered` head (**10927–10949**) | yes |

| Managed (`VmPAcsDLevelScriptCtor950Slice.cs`) | Native (`p_acs.cpp`) |
|------------------------------------------------|---------------------|
| `ResumeSuspendedToRunning` / `AlreadyRunningNonSuspended` | **10801–10807** |
| `ShouldCreate*Thinker` / `LocalVarInitCount` | **10817–10839** |
| `ActivationBackSide` / `ShouldTrackInRunningScripts` | **10844–10857** |
| `ShouldPutLastForHexenHack` | **10861–10864** |
| `TrySetScriptStateOnController` / defer states | **10869–10920** |
| `FlagsForDeferredExecute` / `CopyDeferredArgs` / `ResolveDeferredPlayerNum` | **10901–10949** |

### Chunk **950** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**949** |


**Post-review ratio:** 75/150 managed/native ≈ 50% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsDLevelScriptCtor950SliceTests.AlreadyRunningNonSuspended_WhenRunning`
- `VmPAcsDLevelScriptCtor950SliceTests.LocalVarInitCount_ClampsToArgCount`
- `VmPAcsDLevelScriptCtor950SliceTests.ShouldTrackInRunningScripts_WhenNotAlways`
- `VmPAcsDLevelScriptCtor950SliceTests.ActivationBackSide_ReadsFlag`
- `VmPAcsDLevelScriptCtor950SliceTests.FlagsForDeferredExecuteAlways_SetsAlwaysBit`
- `VmPAcsDLevelScriptCtor950SliceTests.CopyDeferredArgs_PadsWithZeros`
- `VmPAcsDLevelScriptCtor950SliceTests.ResolveDeferredPlayerNum_MissingWhenNoPlayer`
- `VmPAcsDLevelScriptCtor950SliceTests.ShouldPutLastForHexenHack_ServerMapOnly`
- `VmPAcsDLevelScriptCtor950SliceTests.IsDeferredExecuteType_IncludesAlways`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 950 --to 950` (0 hard / 0 soft).

## Seams

`Create<DLevelScript>`, thinker creation, `Behaviors.FindScript`, and map `deferred` storage remain host seams.

## Credit rationale

Honest **150**-line UNCredited band with zero tally overlap; exclusive `VmPAcsDLevelScriptCtor950Slice.cs` (hosted `AcsVm` fibers are not matrix credit).
