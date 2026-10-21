# Chunk 551 — FxStackVariable ctor/emit and FxStructMember RequestAddress scope gates

## Scope

FxStackVariable ctor/emit and FxStructMember RequestAddress scope gates

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxStackVariable::FxStackVariable** 7517–7520 (4 lines)
- `src/common/scripting/backend/codegen.cpp` **FxStackVariable::~FxStackVariable** 7529–7533 (5 lines)
- `src/common/scripting/backend/codegen.cpp` **FxStackVariable::RequestAddress** 7541–7546 (6 lines)
- `src/common/scripting/backend/codegen.cpp` **FxStackVariable::Resolve** 7554–7559 (6 lines)
- `src/common/scripting/backend/codegen.cpp` **FxStackVariable::Emit** 7561–7598 (38 lines)
- `src/common/scripting/backend/codegen.cpp` **FxStructMember::RequestAddress** 7635–7666 (32 lines)
- `src/common/scripting/backend/codegen.cpp` **FxStructMember::Resolve** 7674–7716 (43 lines)
- `src/common/scripting/backend/codegen.cpp` **FxStructMember::Resolve** 7718–7743 (26 lines)

**Credited native LOC:** 160 (tally `nativeLoc` 160).

## Managed

`ZccStackVariableEmit.cs`, `ZccStackVariableEmitTests.cs`, `ZccSuperGlobalCvarResolve.cs`
Managed LOC (tally): 129.
Conversion commit: `4b9d8720d150d0775523870e85b5ee1e6b78b0f9`.

## Tests

- `ZccStackVariableEmitTests.StackOffsetRegister_ZeroMeansDefault`
- `ZccStackVariableEmitTests.StackVariableWritable_RequiresCtx`
- `ZccStackVariableEmitTests.StackUseBitLoad_WhenBitSet`
- `ZccStackVariableEmitTests.StackLoadOpForMember_RewritesLoToLp`
- `ZccStackVariableEmitTests.StructMemberWritable_RequiresScopeBarrier`
- `ZccStackVariableEmitTests.StructMemberResolve_RejectsUnreadable`
- `ZccStackVariableEmitTests.StructMemberResolve_StaticReadonlyPromotesGlobal`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 551 --to 551` (0 hard / 0 soft).

## Seams

`VMFunctionBuilder`, `ExpEmit`, and `FScopeBarrier` live instances are not hosted.

## Credit rationale

Credits native bands mirrored by `ZccStackVariableEmit` and tests — not full bytecode emission. Seam follow-up: `PlanStackVariableEmit` and struct-member writable gates ported toward the 60% target (no LOC padding).
