# Chunk 534 — FxCompareRel::Resolve prologue, FxSpaceship::Resolve

## Scope

FxCompareRel::Resolve prologue, FxSpaceship::Resolve

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxCompareRel::Resolve** 3876–3933 (58 lines)
- `src/common/scripting/backend/codegen.cpp` **FxSpaceship::Resolve** 4704–4752 (49 lines)

**Credited native LOC:** 107 (tally `nativeLoc` 107).

## Managed

`ZccCompareRelResolve.cs`, `ZccCompareRelResolveTests.cs`, `ZccSpaceshipResolve.cs`, `ZccSpaceshipResolveTests.cs`
Managed LOC (tally): 126.
Conversion commit: `184ce29cb2961520e23ac81bde0e7c4d9a152ea7`.

## Tests

- `ZccCompareRelResolveTests.NeedsTranslationCast_Below412`
- `ZccCompareRelResolveTests.StringComparisonPath_EitherSideString`
- `ZccSpaceshipResolveTests.OperatorVersionPolicy_Requires4151`
- `ZccSpaceshipResolveTests.TryFoldConstantSpaceship_Ordering`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 534 --to 534` (0 hard / 0 soft).

## Seams

`StringConstToChar`, `FxStringCast`, `Promote`, and `Emit` are not hosted.

## Credit rationale

Credits compare-rel coercion prologue (3876–3933) and spaceship resolve (4704–4752); numeric compare fold remains chunk 55.
