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

`ZccSuperGlobalCvarResolve.cs`
Managed LOC (tally): 77.
Conversion commit: `656a6cc4585201d331eca8d7634100df46ed2852`.

## Padding review (2026-10-21)

Deepen pass (2026-10-21) had inflated ratio via `*EmitPlan` record structs, string opcode sequences, and identity helpers (`MaskCvarShift`, `SelfEmitIncludesCheck`, etc.). Removed; see method map below.

| Managed | Native (`codegen.cpp`) |
|---------|------------------------|
| `TryResolveSuper` | `FxSuper::Resolve` 7318–7329 |
| `GlobalVariableRequestAddress`, `GlobalVariableWritable` | `FxGlobalVariable::RequestAddress` 7348–7353 |
| `MapCvarRealType` | `ECVarType` values via `FxCVar::Resolve` 7415–7437 |
| `ShouldUseBitLoad`, `GlobalVariableBitMask`, `GlobalVariableLoadOp` | `FxGlobalVariable::Emit` 7380–7388 |
| `MapCvarRealType`, `CvarResolveDeletesOnUnknown` | `FxCVar::Resolve` 7415–7443 |
| `EmitLoadOp`, `CvarEmitUsesStringRegister` | `FxCVar::Emit` 7447–7477 |
| `ApplyFlagCvarShift`, `ApplyFlagCvarMask` | `CVAR_Flag` emit 7487–7488 |
| `ApplyMaskCvar` | `CVAR_Mask` emit 7497–7498 |

**Post-review ratio:** 77/152 managed/native ≈ 51% (src file LOC at HEAD; see tally).

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

Credits native resolve/emit type gates mirrored by `ZccSuperGlobalCvarResolve` and tests — not full bytecode emission.
