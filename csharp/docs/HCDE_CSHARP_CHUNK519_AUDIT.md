# Chunk 519 — FxCompareEq::Resolve

## Scope

FxCompareEq::Resolve

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxCompareEq::Resolve** 4175–4391 (217 lines)

**Credited native LOC:** 217 (tally `nativeLoc` 217).

## Managed

(see conversion commit file list)
Managed LOC (tally): 111.
Conversion commit: `80d2e62788d5eaaf28ee6d0fcd64542c1e498fe2`.

## Tests

- `ZccCompareEqResolveTests.FoldStringEqual_ApproxIgnoresCase`
- `ZccCompareEqResolveTests.FoldFloatEqual_Neq`
- `ZccCompareEqResolveTests.ShouldUnaryNotRight_EqAndZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 519 --to 519` (0 hard / 0 soft).

## Seams

`FxTypeCast`, `StringConstToChar`, `AreCompatiblePointerTypes`, and `ScriptPosition.Message` are not hosted.

## Credit rationale

Credits equality comparison type coercion, constant folding, and zero-simplify paths from `codegen.cpp` (4175–4391).
