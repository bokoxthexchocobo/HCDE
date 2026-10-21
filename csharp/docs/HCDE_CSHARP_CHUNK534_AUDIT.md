# Chunk 534 — FxCompareRel::Resolve

## Scope

FxCompareRel::Resolve

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxCompareRel::Resolve** 3876–4046 (171 lines)

**Credited native LOC:** 171 (tally `nativeLoc` 171).

## Managed

`ZccCompareRelResolve.cs`, `ZccCompareRelResolveTests.cs`
Managed LOC (tally): 122.
Conversion commit: `d7283741658405d7afc612f5ca4160bd1b17bf1e`.

## Tests

- `ZccCompareRelResolveTests.UIntLeftSintRightWarning_OutOfRangeConstant`
- `ZccCompareRelResolveTests.FoldFloatCompare_Geq`
- `ZccCompareRelResolveTests.TryFoldConstantCompare_StringLess`
- `ZccCompareRelResolveTests.NeedsTranslationCast_Below412`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 534 --to 534` (0 hard / 0 soft).

## Seams

`StringConstToChar`, `FxStringCast`, `Promote`, and `FxConstant` emission are not hosted.

## Credit rationale

Credits relative comparison resolve (string/numeric paths, signed/unsigned warnings, constant folding) from `codegen.cpp` (3876–4046).
