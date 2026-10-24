# Chunk 527 — FxTypeCast::Resolve

## Scope

FxTypeCast::Resolve

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxTypeCast::convertRawFunctionToFunctionPointer** 1775–1802 (28 lines)
- `src/common/scripting/backend/codegen.cpp` **FxTypeCast::Resolve** 1804–2017 (214 lines)

**Credited native LOC:** 242 (tally `nativeLoc` 242).

## Managed

`csharp/src/HCDE.Scripting/ZccTypeCastResolve.cs` (src-only credit).

Managed LOC (tally): 154.

## Tests

- `ZccTypeCastResolveTests.NullPtrToPointer_Allowed`
- `ZccTypeCastResolveTests.VersionAtLeast_4141`
- `ZccTypeCastResolveTests.NullPtrToIdTypes_RequiresVersion`
- `ZccTypeCastResolveTests.PlanRawFunction_RejectsVirtual`
- `ZccTypeCastResolveTests.CompatiblePointerTypes_RejectsConstStrip`
- `ZccTypeCastResolveTests.ClassifyResolveCast_NullPtrToId_Requires4141`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 527 --to 527` (0 hard / 0 soft).

## Seams

`FxFloatCast`, `convertRawFunctionToFunctionPointer`, and `ScriptPosition.Message` are not hosted.

## Credit rationale

Credits ZScript type-cast resolve dispatch (null pointer, ID types, numeric/string casts) and raw-function pointer conversion from `codegen.cpp` (1775–2017).

## Padding review (2026-10-21)

| Managed | Native |
|---------|--------|
| `PlanRawFunctionPointer` | `convertRawFunctionToFunctionPointer` 1775–1802 |
| `ClassifyResolveCast` | `Resolve` early branches 1831–2000 (before `Fx*Cast` nodes) |
| `CompatiblePointerTypes` | call at 1982 → `AreCompatiblePointerTypes` 267–308 (hosted mirror, not separate credit) |
| `NullPtrToIdTypes`, `NativeStructToWritablePointer`, `ClassDescendantAllowsCast`, `CompatibleVectors` | matching `Resolve` branches |

**Removed:** `SameExpressionTypeCast`, `ClassMetaTypeCast`, `VoidBaseTypeCast`, `CompatibleFnPrototypeArgs`, `CompatibleFnPointerTypes` wrapper, thin `RawFunctionToFnPointer` / `TrySpecialTypeCast` AND-predicates, `MethodPointerClearsThisArg`, `RejectsVirtualActionPointer`.

**Ratio ~64%:** Unhosted `FxFloatCast`/`FxIntCast`/… nodes and error messaging dominate native LOC; cannot reach 60% without hosting those AST emitters.
