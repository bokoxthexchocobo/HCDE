# Chunk 569 — FxTypeCast::Emit through FxMinusSign::Emit (2025–2210; unary-not stays chunks 56/61)

## Scope

FxTypeCast::Emit through FxMinusSign::Emit (2025–2210; unary-not stays chunks 56/61)

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxUnarySignCodegen** 2025–2210 (186 lines)

**Credited native LOC:** 186 (tally `nativeLoc` 186).

## Managed

`ZccUnarySignCodegen.cs`
Managed LOC (tally): 111.
Conversion commit: `346327f99be29c3f9a44a65e2b6e788a3e0be3d9`.

## Padding review (2026-10-21)

| Managed | Native (`codegen.cpp`) |
|---------|---------------------------|
| `TypeCastEmitIsUnreachable` | `FxTypeCast::Emit` 2025–2030 |
| `PlusSignResolveAllowed` / `PlusSignEmitForwardsOperand` | `FxPlusSign::Resolve` 2061–2078; `FxPlusSign::Emit` 2081–2084 |
| `ResolveMinusSign` | `FxMinusSign::Resolve` 2115–2146 |
| `EmitMinusSign` | `FxMinusSign::Emit` 2154–2210 (single method, native structure) |

**Post-review ratio:** 111/186 managed/native ≈ 60% (src file LOC at HEAD; see tally).

## Tests

- `ZccUnarySignCodegenTests.TypeCastEmitIsUnreachable`
- `ZccUnarySignCodegenTests.PlusSignResolveAllowed_AcceptsQuaternion`
- `ZccUnarySignCodegenTests.ResolveMinusSign_FoldsIntConstant`
- `ZccUnarySignCodegenTests.EmitMinusSign_KonstIntPoolBranch`
- `ZccUnarySignCodegenTests.EmitMinusSign_VectorNegV3WhenFixed`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 569 --to 569` (0 hard / 0 soft).

## Seams

`VMFunctionBuilder`, `ExpEmit`, `FxIntCast`, and `build->Emit` are not hosted.

## Credit rationale

Faithful managed mirrors for unary plus/minus and the unreachable type-cast emit path. One `EmitMinusSign` method follows native control flow.
