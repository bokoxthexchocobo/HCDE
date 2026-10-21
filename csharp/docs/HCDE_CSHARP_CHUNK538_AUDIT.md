# Chunk 538 — FxRandom family resolve, FxIdentifier::Resolve/ResolveMember

## Scope

FxRandom family resolve, FxIdentifier::Resolve/ResolveMember

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxRandom::Resolve** 6203–6215 (13 lines)
- `src/common/scripting/backend/codegen.cpp` **FxRandomPick::Resolve** 6307–6317 (11 lines)
- `src/common/scripting/backend/codegen.cpp` **FxRandom2::Resolve** 6507–6512 (6 lines)
- `src/common/scripting/backend/codegen.cpp` **FxRandomSeed::Resolve** 6586–6591 (6 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::Resolve** 6647–6856 (210 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::ResolveMember** 6864–6983 (120 lines)

**Credited native LOC:** 366 (tally `nativeLoc` 366).

## Managed

`ZccIdentifierResolve.cs`, `ZccIdentifierResolveTests.cs`, `ZccRandomResolve.cs`, `ZccRandomResolveTests.cs`
Managed LOC (tally): 198.
Conversion commit: `f0d083720dd24d23ce2f7b381e2120a9d0c0b190`.

## Tests

- `ZccRandomResolveTests.RandomRange_ResolvesMinMax`
- `ZccRandomResolveTests.RandomPick_ResolvesAllChoices`
- `ZccRandomResolveTests.Random2_ResolvesMask`
- `ZccRandomResolveTests.RandomSeed_ResolvesSeedExpr`
- `ZccIdentifierResolveTests.LocalVariable_HighestPriority`
- `ZccIdentifierResolveTests.GlobalConstant_VersionGate`
- `ZccIdentifierResolveTests.RejectsUserinfoCvar`
- `ZccIdentifierResolveTests.MemberField_PrivateAccessDenied`
- `ZccIdentifierResolveTests.MemberFunction_RequiresCallParens`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 538 --to 538` (0 hard / 0 soft).

## Seams

`RESOLVE`, `FxConstant::MakeConstant`, `FindCVar`, `Emit`, and VM builtin calls are not hosted.

## Credit rationale

Credits random/pick/random2/seed resolve and identifier dispatch from `codegen.cpp` (6203–6215, 6307–6317, 6507–6512, 6586–6591, 6647–6983).
