# Chunk 1340 — RuntimeState path resolve, variable nodes, and the VM frame walk from RuntimeState.cpp (1-324).

## Scope

RuntimeState path resolve, variable nodes, and the VM frame walk from RuntimeState.cpp (1-324).

## Native ranges

- `src/common/scripting/dap/RuntimeState.cpp` **ResolveStateByPath** 1–324 (324 lines)

**Credited native LOC:** 324 (tally `nativeLoc` 324).

## Managed

`VmRuntimeState1340Slice.cs`
Managed LOC (tally): 708.
Conversion commit: `121204d5e0d0364330ed778d31ce76ff033b61af`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Construction and reset (**40–44**, **318–323**) | yes |
| Resolve by path (**46–166**) | yes |
| Path by id and resolve by id (**168–191**) | yes |
| Children by path and id (**193–238**) | yes |
| Variable node factory (**240–270**) | yes |
| Stack and frame walk (**272–317**) | yes |
| Band hull (**1–324**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| ToLower, Split, and Join | string callees |
| std::stoi | stack ids are numbers |
| Array, value, state, object, and struct node bodies | callees constructed here |
| IdProvider::GetNext | callee; the first id is 1000 |

The path is lowercased before it is split. A stack id of 1 reuses the root. Any other parsed id, including a leading sign or trailing junk, builds a new stack node. Child names are registered in lowercase before the lookup. An evaluate walk inserts a virtual container when the direct child is missing, and a null container is skipped. The final id is the stack id when the path has one element, otherwise the id of the lowercase path. Id 1 reports the path "1" without consulting the map, and resolve-by-id does not use that shortcut. A child count of 0 or the default maximum walks the remaining names, and a failed child extends the window by one. Arrays are chosen before values, so an array pointer is not a value node. A pointer to a non-struct container is a value node. Reset clears the path map and replaces the root, and it leaves the global frame stack in place. Frames are walked from the top parent pointer. An empty stack still returns true.

| Managed (`VmRuntimeState1340Slice.cs`) | Native (`RuntimeState.cpp`) |
|----------------------------------------|-----------------------------|
| Path resolve, variable nodes, and the frame walk | **1–324** |
| Band hull (exclusive slice) | **1–324** |

### Chunk **1340** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **324** |
| **Net headline** | **+324** vs post-**1339** |


**Post-review ratio:** 708/324 managed/native ≈ 219% (src file LOC at HEAD; see tally).

## Tests

- `VmRuntimeState1340SliceTests.PathsIdsAndChildWindow`
- `VmRuntimeState1340SliceTests.VariableNodesAndFrameWalk`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1340 --to 1340` (0 hard / 0 soft).

## Seams

State-node child lists stay on the structured node. IdProvider starts at 1000. ParseInt is std::stoi, and its catch of void* does not catch invalid_argument or out_of_range, so a bad stack id throws. GetStack ignores the stack id and returns the one global frame stack.

## Credit rationale

UNCredited RuntimeState.cpp 1-324 (+324). A full tally line scan shows no overlap. This whole file was tied with models_ue1.cpp 1-324. Path order credited the model loader as chunk 1339, so this is the other half of that tie.
