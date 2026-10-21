# Chunk 569 — FxTypeCast::Emit stub, FxPlusSign/FxMinusSign, FxUnaryNotBoolean (2018–2345)

## Scope

FxTypeCast::Emit stub, FxPlusSign/FxMinusSign, FxUnaryNotBoolean (2018–2345)

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxTypeCast::Emit** 2025–2030 (6 lines)
- `src/common/scripting/backend/codegen.cpp` **FxPlusSign::Resolve** 2061–2078 (18 lines)
- `src/common/scripting/backend/codegen.cpp` **FxPlusSign::Emit** 2081–2084 (4 lines)
- `src/common/scripting/backend/codegen.cpp` **FxMinusSign::Resolve** 2115–2146 (32 lines)
- `src/common/scripting/backend/codegen.cpp` **FxMinusSign::Emit** 2154–2210 (57 lines)
- `src/common/scripting/backend/codegen.cpp` **FxUnaryNotBoolean::Resolve** 2335–2355 (21 lines)
- `src/common/scripting/backend/codegen.cpp` **FxUnaryNotBoolean::Emit** 2364–2385 (22 lines)

**Credited native LOC:** 160 (tally `nativeLoc` 160).

## Managed

`ZccUnarySignCodegen.cs`
Managed LOC (tally): 86.
Conversion commit: `3cda497494d78d5b0e8c0c910e6fafe90b0de468`.

## Padding review (2026-10-21)

| Managed | Native (`codegen.cpp`) |
|---------|---------------------------|
| `ZccUnarySignCodegen.PlusSignOperandAllowed` | `FxPlusSign::Resolve` 2061–2078 |
| `ZccUnarySignCodegen` negate helpers | `FxMinusSign::Resolve` 2115–2146; `FxMinusSign::Emit` 2154–2210 |
| `FxTypeCast::Emit` | assert-only stub 2025–2030 (seam: unreachable in hosted VM) |

**Post-review ratio:** 86/160 managed/native ≈ 54% (src file LOC at HEAD; see tally).

## Tests

- `ZccUnarySignCodegenTests.PlusSignOperandAllowed_AcceptsVectors`
- `ZccUnarySignCodegenTests.MinusSignShouldCastBoolToInt_WhenNonConstantBool`
- `ZccUnarySignCodegenTests.NegateEmitOpcode_PicksVectorOpcode`
- `ZccUnarySignCodegenTests.UnaryNotBoolean_FoldsConstants`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 569 --to 569` (0 hard / 0 soft).

## Seams

`VMFunctionBuilder`, `ExpEmit`, `FxIntCast`, and `build->Emit` are not hosted.

## Credit rationale

Whole-function ports for unary plus/minus ZScript codegen paths.
