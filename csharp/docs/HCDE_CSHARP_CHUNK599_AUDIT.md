# Chunk 599 — FxVMFunctionCall::Resolve guards + Emit/CheckEmitCast helpers (9814–10326)

## Scope

FxVMFunctionCall::Resolve guards + Emit/CheckEmitCast helpers (9814–10326)

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxVMFunctionCall::Resolve** 9837–9841 (5 lines)
- `src/common/scripting/backend/codegen.cpp` **FxVMFunctionCall::Resolve** 9851–9855 (5 lines)
- `src/common/scripting/backend/codegen.cpp` **FxVMFunctionCall::Resolve** 9861–9866 (6 lines)
- `src/common/scripting/backend/codegen.cpp` **FxVMFunctionCall::Resolve** 9884–9888 (5 lines)
- `src/common/scripting/backend/codegen.cpp` **FxVMFunctionCall::Resolve** 9890–9935 (46 lines)
- `src/common/scripting/backend/codegen.cpp` **FxVMFunctionCall::Resolve** 10140–10153 (14 lines)
- `src/common/scripting/backend/codegen.cpp` **FxVMFunctionCall::Resolve** 10169–10181 (13 lines)
- `src/common/scripting/backend/codegen.cpp` **FxVMFunctionCall::Resolve** 10207–10228 (22 lines)
- `src/common/scripting/backend/codegen.cpp` **FxVMFunctionCall::Resolve** 10251–10266 (16 lines)
- `src/common/scripting/backend/codegen.cpp` **FxVMFunctionCall::Resolve** 10296–10325 (30 lines)

**Credited native LOC:** 162 (tally `nativeLoc` 162).

## Managed

`ZccVmFunctionCallResolve.cs`, `ZccVmFunctionCallEmit.cs`
Managed LOC (tally): 177.
Conversion commit: `81499489de6ab4197882117ba7d2cbcad1e7cab4`.

## Padding review (2026-10-21)

Former chunk 600 merged here (retired). Whole-function ports only; no LOC padding.

| Managed | Native (`codegen.cpp`) |
|---------|---------------------------|
| `RequiresSelfPointerNative` | non-static method guard 9837–9841 |
| `RejectsAbstractDirectCallNative` | abstract call guard 9851–9855 |
| `TooManyArgumentsNative` / `NamedArgsForbiddenInFnPtrNative` | arity + fn-ptr named args 9861–9888 |
| `RelaxedNamedArgsNative` / `FindArgIndexByNameNative` / `NamedArgOutOfOrderNative` | named-arg placement 9890–9935 |
| `ShouldSetReturnTypeFromProtoNative` / `ShouldAssignVoidReturnTypeNative` | return type 10140–10153 |
| `ShouldEarlyCastReturnNative` / `IsStaticCallNative` | `Emit` prologue 10169–10181 |
| `ShouldEmitScopeBarrierNative` / `ShouldPassActionStatePointersNative` | method self + action args 10190–10228 |
| `MatchesDefaultArgRegTypeNative` | default-arg `switch` cases 10251–10266 |
| `IsDecorateCastKludgeNative` / `ShouldEmitRetIntImmediateNative` | `CheckEmitCast` 10296–10325 |
| `NapConstantNative` | `static_assert(NAP == 3)` at 10217 (compile-time constant) |
| `ShouldNullCheckFnPtrSelfNative` | `FnPtrCall` self branch issues `OP_NULLCHECK` 10231–10236 |
| `ShouldEmitOpRetNative` | `CheckEmitCast` `returnit` fast-path 10302–10314 |
| `RunResolveGuardPipeline` / `RunEmitProloguePipeline` | resolve + emit orchestration **9837–10325** |

### Out-of-body uncredit (599)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **162** |
| **Out-of-body removed** | **0** |
| **Honest** | **162** |


**Post-review ratio:** 177/162 managed/native ≈ 109% (src file LOC at HEAD; see tally).

## Tests

- `ZccVmFunctionCallResolveTests.RelaxedNamedArgsNative_At413`
- `ZccVmFunctionCallResolveTests.RunResolveGuardPipeline_FlagsMissingSelf`
- `ZccVmFunctionCallEmitPipelineTests.RunEmitProloguePipeline_AllowsScopeBarrier`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 599 --to 599` (0 hard / 0 soft).

## Seams

`compileEnvironment.ResolveSpecialFunction`, `ScriptPosition.Message`, `FunctionCallEmitter`, and VM opcode emission are not hosted.

## Credit rationale

Faithful decision helpers for VM function call resolve guards, named arguments, return-type selection, emit static-call/scope/action-arg paths, and decorate cast kludge.
