# Chunk 942 — `SF_Wait` through `SF_ScriptRunning` scan from `fragglescript/t_func.cpp` (**3845–3994**). Follows **941** (**≤3844**); stops before **851** init (**4017+**). **3995–4016** gap deferred if sub-150.

## Scope

`SF_Wait` through `SF_ScriptRunning` scan from `fragglescript/t_func.cpp` (**3845–3994**). Follows **941** (**≤3844**); stops before **851** init (**4017+**). **3995–4016** gap deferred if sub-150.

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Wait** 3845–3994 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmFScriptParserScriptWait942Slice.cs`
Managed LOC (tally): 89.
Conversion commit: `1c93933203da0c085bdaa9d0ae4325c94778628e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`fragglescript/t_func.cpp`) |
|---------|-----------------------------------|
| chunk **942** hull | **3845–3994** |
| `UnaryWaitRequiresExactlyOneArg` | argc checks **3854**, **3878**, **3901**, **3924**, **3944** |
| `BuildDelayWait` / `WaitDelayTicksFromCentiseconds` | `SF_Wait` **3860–3865** |
| `BuildTagWait` | `SF_TagWait` **3884–3888** |
| `BuildScriptWait` | `SF_ScriptWait` **3907–3911** |
| `BuildScriptWaitPre` | `SF_ScriptWaitPre` **3930–3933** |
| `StartScriptNumberInRange` | range test **3952–3955** |
| `BuildStartScript` | `SF_StartScript` **3958–3971** |
| `ScriptRunningRequiresAtLeastOneArg` | **3986–3989** |
| `ScriptRunningEntryMatches` / `ScriptRunningReturn` | loop **3994–4007** (hull ends **3994**) |
| `ScriptRunningScanShouldContinue` | `current->next` walk **3994** |
| `StartScriptRequiresExactlyOneArg` | **3944** |
| `StartScriptChildDefined` | undefined child **3964–3966** |

### Chunk **942** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**941** |


**Post-review ratio:** 89/150 managed/native ≈ 59% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserScriptWait942SliceTests.WaitDelayTicksFromCentiseconds_ScalesWithTicRate`
- `VmFScriptParserScriptWait942SliceTests.BuildDelayWait_RejectsWrongArgCount`
- `VmFScriptParserScriptWait942SliceTests.BuildTagWait_SetsTagWaitKind`
- `VmFScriptParserScriptWait942SliceTests.StartScriptNumberInRange_RejectsNegative`
- `VmFScriptParserScriptWait942SliceTests.BuildStartScript_ReturnsNullWhenChildMissing`
- `VmFScriptParserScriptWait942SliceTests.ScriptRunningReturn_ReturnsOneWhenFound`
- `VmFScriptParserScriptWait942SliceTests.BuildScriptWaitPre_ThrowsTerminatorFlag`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 942 --to 942` (0 hard / 0 soft).

## Seams

`SaveCurrentScript`, `CFsTerminator`, `Create<DRunningScript>`, `LevelScript->children`, and running-script chain walks remain host seams.

## Credit rationale

Honest **150**-line UNCredited band; exclusive `VmFScriptParserScriptWait942Slice.cs` (live `VmFScriptParserSfWait` handlers are not matrix credit for these lines).
