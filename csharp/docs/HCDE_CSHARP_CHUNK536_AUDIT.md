# Chunk 536 — FxDotCross, FxTypeCheck, FxDynamicCast, FxConditional::Resolve; DPlaneWatcher Construct/Tick

## Scope

FxDotCross, FxTypeCheck, FxDynamicCast, FxConditional::Resolve; DPlaneWatcher Construct/Tick

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxDotCross::Resolve** 5142–5157 (16 lines)
- `src/common/scripting/backend/codegen.cpp` **FxTypeCheck::Resolve** 5209–5234 (26 lines)
- `src/common/scripting/backend/codegen.cpp` **FxDynamicCast::Resolve** 5301–5314 (14 lines)
- `src/common/scripting/backend/codegen.cpp` **FxConditional::Resolve** 5365–5442 (78 lines)
- `src/playsim/p_acs.cpp` **DPlaneWatcher::Construct** 1899–1928 (30 lines)
- `src/playsim/p_acs.cpp` **DPlaneWatcher::Tick** 1945–1971 (27 lines)

**Credited native LOC:** 191 (tally `nativeLoc` 191).

## Managed

`VmAcsPlaneWatcher.cs`, `VmAcsPlaneWatcherTests.cs`, `ZccConditionalAndTypeResolve.cs`, `ZccConditionalAndTypeResolveTests.cs`
Managed LOC (tally): 122.
Conversion commit: `5b2e345d4228641860dfd8bceeff4e230cec4a5f`.

## Tests

- `ZccConditionalAndTypeResolveTests.DotCross_ValidatesVectorOperands`
- `ZccConditionalAndTypeResolveTests.DotCross_ResultType`
- `ZccConditionalAndTypeResolveTests.TypeCheck_UsesClassPointerPath`
- `ZccConditionalAndTypeResolveTests.DynamicCast_PreservesConstPointer`
- `ZccConditionalAndTypeResolveTests.Conditional_MergesIntegerBranches`
- `ZccConditionalAndTypeResolveTests.Conditional_FoldConstantCondition`
- `VmAcsPlaneWatcherTests.Construct_RecordsWatchPlane`
- `VmAcsPlaneWatcherTests.Tick_FiresWhenPlaneCrossesWatchD`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 536 --to 536` (0 hard / 0 soft).

## Seams

`Promote`, `FxBoolCast`, `FxFloatCast`, `FxTranslationCast`, `P_ExecuteSpecial`, and `Emit` are not hosted.

## Credit rationale

Credits vector dot/cross typing, `is` type-check casts, dynamic cast pointer rewrite, ternary resolve, and ACS plane watcher setup/crossing from `codegen.cpp` (5142–5442) and `p_acs.cpp` (1899–1971).
