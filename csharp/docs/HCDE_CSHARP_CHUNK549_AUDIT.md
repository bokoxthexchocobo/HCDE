# Chunk 549 — FxSuper::Resolve, FxGlobalVariable address/emit, FxCVar::Resolve/Emit type switch

## Scope

FxSuper::Resolve, FxGlobalVariable address/emit, FxCVar::Resolve/Emit type switch

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxGlobalVariable::FxGlobalVariable** 7337–7340 (4 lines)
- `src/common/scripting/backend/codegen.cpp` **FxSuper::Resolve** 7318–7329 (12 lines)
- `src/common/scripting/backend/codegen.cpp` **FxGlobalVariable::RequestAddress** 7348–7353 (6 lines)
- `src/common/scripting/backend/codegen.cpp` **FxGlobalVariable::Resolve** 7361–7366 (6 lines)
- `src/common/scripting/backend/codegen.cpp` **FxGlobalVariable::Emit** 7368–7391 (24 lines)
- `src/common/scripting/backend/codegen.cpp` **FxCVar::FxCVar** 7400–7404 (5 lines)
- `src/common/scripting/backend/codegen.cpp` **FxCVar::Resolve** 7412–7445 (34 lines)
- `src/common/scripting/backend/codegen.cpp` **FxCVar::Emit** 7447–7477 (31 lines)
- `src/common/scripting/backend/codegen.cpp` **FxCVar::Emit** 7479–7508 (30 lines)

**Credited native LOC:** 152 (tally `nativeLoc` 152).

## Managed

`ZccStackVariableEmit.cs`, `ZccStackVariableEmitTests.cs`, `ZccSuperGlobalCvarResolve.cs`
Managed LOC (tally): 122.
Conversion commit: `4b9d8720d150d0775523870e85b5ee1e6b78b0f9`.

## Tests

- `ZccSuperGlobalCvarResolveTests.GlobalVariableWritable_RequiresBoth`
- `ZccSuperGlobalCvarResolveTests.TryResolveSuper_RejectsOutsideMemberFunction`
- `ZccSuperGlobalCvarResolveTests.ShouldUseBitLoad_WhenBitValueSet`
- `ZccSuperGlobalCvarResolveTests.MapCvarRealType_StringMapsToString`
- `ZccSuperGlobalCvarResolveTests.EmitLoadOp_FloatUsesLsp`
- `ZccSuperGlobalCvarResolveTests.CvarEmitUsesStringRegister_ForStringType`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 549 --to 549` (0 hard / 0 soft).

## Seams

`VMFunctionBuilder`, `ExpEmit`, and live `FBaseCVar` instances are not hosted.

## Credit rationale

Credits native resolve/emit type gates mirrored by `ZccSuperGlobalCvarResolve` and tests — not full bytecode emission. Seam follow-up: expanded `PlanGlobalVariableEmit` / `PlanCvarEmit` and flag/mask helpers toward the 60% managed target (no LOC padding).
