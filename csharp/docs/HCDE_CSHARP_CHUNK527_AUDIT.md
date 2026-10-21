# Chunk 527 — FxTypeCast::Resolve

## Scope

`FxTypeCast::convertRawFunctionToFunctionPointer`, `FxTypeCast::Resolve`

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **convertRawFunctionToFunctionPointer** 1775–1802 (28 lines)
- `src/common/scripting/backend/codegen.cpp` **FxTypeCast::Resolve** 1804–2017 (214 lines)

**Credited native LOC:** 242 (tally `nativeLoc`).

## Managed

`csharp/src/HCDE.Scripting/ZccTypeCastResolve.cs` (src-only credit).

## Tests

- `ZccTypeCastResolveTests.NullPtrToPointer_Allowed`
- `ZccTypeCastResolveTests.VersionAtLeast_4141`
- `ZccTypeCastResolveTests.NullPtrToIdTypes_RequiresVersion`
- `ZccTypeCastResolveTests.PlanRawFunction_RejectsVirtual`
- `ZccTypeCastResolveTests.CompatiblePointerTypes_RejectsConstStrip`
- `ZccTypeCastResolveTests.NativeStructToWritablePointer_RejectsReadonlyAt412`
- `ZccTypeCastResolveTests.ClassDescendantAllowsCast`

## Seams

`FxFloatCast`, hosted `ScriptPosition.Message`, and full `FxExpression` graph are not hosted.

## Credit rationale

Credits special-cast hook, raw-function pointer conversion, invalid-type guards, null-pointer/id promotions, class/struct/vector branches, and pointer-compatibility rules used by `FxTypeCast::Resolve` from `codegen.cpp` (1775–2017).

## Padding review (2026-10-21)

Reviewed deepen commit `080fffc9` against the padding checklist.

**Removed:** `CastResolveRoute` / `PlanRoute` (ordered switch mirror with no extra semantics); `TargetIs*` / `*Cast` identity predicates; always-true / always-false stubs (`ShouldDeleteSelfAfterDelegate`, `ReplaceBaseWithZeroConstant`, `ResolveFailed`, `EmitErrorOnInvalidCast`); thin property-compare aliases; `FontCastTarget` / `CompatiblePointerPassthrough`-style pass-throughs.

**Kept:** `TrySpecialTypeCast`; raw-function and VM-function constant paths; `InvalidCastTypes`; `NullPtrToPointer` / `NullPtrToIdTypes` with `MakeVersion`; `ClassDescendantAllowsCast`; `NativeStructToWritablePointer` (4.12 readonly guard); `CompatibleVectors`; integer sprite/texture promotion guards; `PlanRawFunctionPointer` / `MethodPointerClearsThisArg` from `convertRawFunctionToFunctionPointer`; `CompatiblePointerTypes` mirroring `AreCompatiblePointerTypes` (codegen.cpp 267–308) for const and descendant rules used at resolve time.

Remeasured surviving src LOC at HEAD after trim (see tally `managedLoc`).
