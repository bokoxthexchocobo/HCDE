# Chunk 856 — FraggleScript variables from `fragglescript/t_variable.cpp` (1–424).

## Scope

FraggleScript variables from `fragglescript/t_variable.cpp` (1–424).

## Native ranges

- `src/playsim/fragglescript/t_variable.cpp` **intvalue** 45–50 (6 lines)
- `src/playsim/fragglescript/t_variable.cpp` **fixedvalue** 58–63 (6 lines)
- `src/playsim/fragglescript/t_variable.cpp` **floatvalue** 71–77 (7 lines)
- `src/playsim/fragglescript/t_variable.cpp` **stringvalue** 85–110 (26 lines)
- `src/playsim/fragglescript/t_variable.cpp` **actorvalue** 117–150 (34 lines)
- `src/playsim/fragglescript/t_variable.cpp` **DFsVariable::DFsVariable** 169–176 (8 lines)
- `src/playsim/fragglescript/t_variable.cpp` **DFsVariable::GetValue** 185–220 (36 lines)
- `src/playsim/fragglescript/t_variable.cpp` **DFsVariable::SetValue** 229–278 (50 lines)
- `src/playsim/fragglescript/t_variable.cpp` **DFsVariable::Serialize** 286–295 (10 lines)
- `src/playsim/fragglescript/t_variable.cpp` **DFsScript::NewVariable** 310–320 (11 lines)
- `src/playsim/fragglescript/t_variable.cpp` **DFsScript::NewFunction** 323–326 (4 lines)
- `src/playsim/fragglescript/t_variable.cpp` **DFsScript::VariableForName** 335–348 (14 lines)
- `src/playsim/fragglescript/t_variable.cpp` **DFsScript::FindVariable** 358–378 (21 lines)
- `src/playsim/fragglescript/t_variable.cpp` **DFsScript::ClearVariables** 387–412 (26 lines)
- `src/playsim/fragglescript/t_variable.cpp` **DFsScript::LabelValue** 420–424 (5 lines)

**Credited native LOC:** 264 (tally `nativeLoc` 264).

## Managed

`VmFScriptVariable.cs`
Managed LOC (tally): 221.
Conversion commit: `05582feae59ed50ce80f19d013d79829b5ae5c6c`.

## Padding review (2026-10-21)

**Behavior ported**

| Managed | Native |
|---------|--------|
| coercion + script variable API | **1–424** |

Gross **424** net +424 vs **854**.

### Out-of-body uncredit (856)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **424** |
| **Out-of-body removed** | **160** |
| **Honest** | **264** |


**Post-review ratio:** 221/264 managed/native ≈ 84% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptVariableTests.IntValue_FromString`
- `VmFScriptVariableTests.FixedValue_FromInt`
- `VmFScriptVariableTests.FloatValue_FromFixed`
- `VmFScriptVariableTests.StringValue_FromInt`
- `VmFScriptVariableTests.ActorValue_MapThingIndex`
- `VmFScriptVariableTests.NewVariable_ChainsInHash`
- `VmFScriptVariableTests.GetValue_CopiesString`
- `VmFScriptVariableTests.SetValue_AdaptsConst`
- `VmFScriptVariableTests.Serialize_RoundTripsName`
- `VmFScriptVariableTests.NewFunction_StoresHandler`
- `VmFScriptVariableTests.VariableForName_FindsChain`
- `VmFScriptVariableTests.FindVariable_WalksParentAndGlobal`
- `VmFScriptVariableTests.ClearVariables_StopsAtLabels`
- `VmFScriptVariableTests.LabelValue_ReturnsIndex`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 856 --to 856` (0 hard / 0 soft).

## Seams

`actorvalue` uses host `SpawnedThings` and `IsMapActor`; `SetValue` uses injected `ScriptError`.

## Credit rationale

Variable hash chains after preprocessor (**853**).
