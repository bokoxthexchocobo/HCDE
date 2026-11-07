# Chunk 1400 — Global names, protocol scope, and the child cache from GlobalScopeStateNode.cpp (1-163).

## Scope

Global names, protocol scope, and the child cache from GlobalScopeStateNode.cpp (1-163).

## Native ranges

- `src/common/scripting/dap/Nodes/GlobalScopeStateNode.cpp` **GlobalScopeStateNode** 1–163 (163 lines)

**Credited native LOC:** 163 (tally `nativeLoc` 163).

## Managed

`VmGlobalScope1400Slice.cs`
Managed LOC (tally): 190.
Conversion commit: `bd8a0ccb9abd467e1efef5f4bf5d3951e129deaf`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| GlobalNames (**30–92**) | yes |
| SerializeToProtocol (**96–110**) | yes |
| GetChildNames (**112–119**) | yes |
| GetChildNode (**121–161**) | yes |
| Band hull (**1–163**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| GetVMValue | callee in GameInterfaces.h |
| CreateNodeForVariable | callee |
| GetId | the scope id is supplied |

GetChildNames appends the static global list and does not include the commented level spelling. The child set matches those names without case. A symbol whose name is NAME_None is skipped, and a symbol that is not a field is skipped, before the type is read. A field reads its type, then GetVMValue at the field offset, and stores the node under a case-insensitive key. A later key that folds to the same name replaces the node and keeps the first stored spelling. The cache is built only while it is empty, so a scan that stores nothing runs again. A missing name leaves the caller's node unchanged.

| Managed (`VmGlobalScope1400Slice.cs`) | Native (`GlobalScopeStateNode.cpp`) |
|---------------------------------------|--------------------------------------|
| Global names, serialize, and child cache | **1–163** |
| Band hull (exclusive slice) | **1–163** |

### Chunk **1400** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **163** |
| **Net headline** | **+163** vs the prior chunk |


**Post-review ratio:** 190/163 managed/native ≈ 117% (src file LOC at HEAD; see tally).

## Tests

- `VmGlobalScope1400SliceTests.SerializeAppendsTheStaticGlobalsAndOmitsTheCommentedLevel`
- `VmGlobalScope1400SliceTests.ChildCacheFoldsLevelSkipsNonFieldsAndKeepsTheFirstKey`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1400 --to 1400` (0 hard / 0 soft).

## Seams

The file is the whole translation unit. GetVMValue and CreateNodeForVariable are callees. The file ends at line 163.

## Credit rationale

UNCredited GlobalScopeStateNode.cpp 1-163 (+163). After chunk 1399 this is the unique largest hole. system_theme.cpp 1-162 is 162. The band is shorter than 400 lines and is not sliced. It runs through end of file.
