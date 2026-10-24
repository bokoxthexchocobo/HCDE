# Chunk 532 — FxAssign::Resolve

## Scope

FxAssign::Resolve

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxAssign::Resolve** 2685–2861 (177 lines)

**Credited native LOC:** 177 (tally `nativeLoc` 177).

## Managed

`ZccAssignResolve.cs`, `ZccAssignResolveTests.cs`
Managed LOC (tally): 108.
Conversion commit: `7143bffe55fd5904a478942f6302b0adf664b170`.

## Tests

- `ZccAssignResolveTests.BoolModifyAssignInvalid_BlocksBoolBase`
- `ZccAssignResolveTests.DynArrayAssignPolicy_RewritesAt4111`
- `ZccAssignResolveTests.MapAssignPolicy_WarnsOnPointerSwapBelow4111`
- `ZccAssignResolveTests.ClassifyContainerAssign_MapIteratorFails`
- `ZccAssignResolveTests.NormalizeBitWriteIndex_StripsInternalFlag`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 532 --to 532` (0 hard / 0 soft).

## Seams

`FxBoolCast`, `FxTypeCast`, `FxMemberFunctionCall`, `RequestAddress`, and `Emit` are not hosted.

## Credit rationale

Credits ZScript assignment resolve guards (bool modify-assign, container copy vs pointer-swap policy, writable lvalue, bitfield write index) from `codegen.cpp` (2685–2861).
