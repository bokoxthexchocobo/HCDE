# Chunk 1398 — Local scope names, line qualification, protocol scope, and the child cache from LocalScopeStateNode.cpp (1-168).

## Scope

Local scope names, line qualification, protocol scope, and the child cache from LocalScopeStateNode.cpp (1-168).

## Native ranges

- `src/common/scripting/dap/Nodes/LocalScopeStateNode.cpp` **LocalScopeStateNode** 1–168 (168 lines)

**Credited native LOC:** 168 (tally `nativeLoc` 168).

## Managed

`VmLocalScope1398Slice.cs`
Managed LOC (tally): 294.
Conversion commit: `ba4320ab91e7afa29bfde81648709540e1e00496`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| LOCAL, SELF, INVOKER (**32–34**) | yes |
| Constructor (**36**) | yes |
| GetLineQualifiedName (**39–42**) | yes |
| GetLineFromLineQualifiedName (**44–52**) | yes |
| SerializeToProtocol (**54–68**) | yes |
| GetChildNames (**71–99**) | yes |
| CacheChildren (**102–150**) | yes |
| GetChildNode (**152–167**) | yes |
| Band hull (**1–168**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| GetLocalsState | callee |
| GetClassDescriptor | callee |
| IsFunctionAction | callee; the frame carries the result |
| GetId | callee; the frame carries the id |
| CreateNodeForVariable | callee |
| unused nodes vector | declared in CacheChildren and never read |

A script function skips a local when the current line is less than or equal to the local line. Duplicate raw names are qualified with the earlier line, and a third copy of that raw name is not qualified. The child map folds A-Z. Both qualified names use the newer local's spelling. An invoker local at index 1 wins over self. A node that cannot be renamed overwrites the existing child and keeps the stored key. GetChildNode fills the cache only while it is empty.

| Managed (`VmLocalScope1398Slice.cs`) | Native (`LocalScopeStateNode.cpp`) |
|--------------------------------------|--------------------------------------|
| Names, protocol scope, and the child cache | **1–168** |
| Band hull (exclusive slice) | **1–168** |

### Chunk **1398** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **168** |
| **Net headline** | **+168** vs the prior chunk |


**Post-review ratio:** 294/168 managed/native ≈ 175% (src file LOC at HEAD; see tally).

## Tests

- `VmLocalScope1398SliceTests.QualifiedNamesSerializeAndSkipFutureLines`
- `VmLocalScope1398SliceTests.CacheRenamesOnCaseFoldAndResolvesInvoker`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1398 --to 1398` (0 hard / 0 soft).

## Seams

GetLocalsState, GetId, GetClassDescriptor, IsFunctionAction, and CreateNodeForVariable are callees. The file ends at line 168.

## Credit rationale

UNCredited LocalScopeStateNode.cpp 1-168 (+168). After chunk 1397 this is the unique largest hole. slopes.cpp 422-588 is 167. The band is shorter than 400 lines and is not sliced.
