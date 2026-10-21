# Chunk 527 — FxTypeCast::Resolve

## Scope

FxTypeCast::Resolve

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxTypeCast::convertRawFunctionToFunctionPointer** 1775–1802 (28 lines)
- `src/common/scripting/backend/codegen.cpp` **FxTypeCast::Resolve** 1804–2017 (214 lines)

**Credited native LOC:** 242 (tally `nativeLoc` 242).

## Managed

`ZccTypeCastResolve.cs`, `ZccTypeCastResolveTests.cs`
Managed LOC (tally): 154.
Conversion commit: `47eb7fcab4c6ad2be55c6493c9a67afb86787e0f`.

## Tests

- `ZccTypeCastResolveTests.PlanRawFunction_RejectsVirtual`
- `ZccTypeCastResolveTests.NullPtrToPointer_Allowed`
- `ZccTypeCastResolveTests.VersionAtLeast_4141`
- `ZccTypeCastResolveTests.NullPtrToIdTypes_RequiresVersion`
- `ZccTypeCastResolveTests.CompatiblePointerTypes_RejectsConstStrip`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 527 --to 527` (0 hard / 0 soft).

## Seams

`FxFloatCast`, `convertRawFunctionToFunctionPointer`, and `ScriptPosition.Message` are not hosted.

## Credit rationale

Credits ZScript type-cast resolve dispatch (null pointer, ID types, numeric/string casts) and raw-function pointer conversion from `codegen.cpp` (1775–2017).
