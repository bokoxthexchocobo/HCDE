# Chunk 527 — FxTypeCast::Resolve

## Scope

FxTypeCast::Resolve

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxTypeCast::Resolve** 1804–2017 (214 lines)

**Credited native LOC:** 214 (tally `nativeLoc` 214).

## Managed

`ZccTypeCastResolve.cs`, `ZccTypeCastResolveTests.cs`
Managed LOC (tally): 108.
Conversion commit: `ca059a9e0c8d5d3ccc561673c6583f2b5f352c14`.

## Tests

- `ZccTypeCastResolveTests.NullPtrToPointer_Allowed`
- `ZccTypeCastResolveTests.VersionAtLeast_4141`
- `ZccTypeCastResolveTests.NullPtrToIdTypes_RequiresVersion`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 527 --to 527` (0 hard / 0 soft).

## Seams

`FxFloatCast`, `convertRawFunctionToFunctionPointer`, and `ScriptPosition.Message` are not hosted.

## Credit rationale

Credits ZScript type-cast resolve dispatch (null pointer, ID types, numeric/string casts) from `codegen.cpp` (1804–2017).
