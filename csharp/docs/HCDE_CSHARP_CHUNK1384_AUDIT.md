# Chunk 1384 — Debugger object state node from ObjectStateNode.cpp (1-196), through end of file.

## Scope

Debugger object state node from ObjectStateNode.cpp (1-196), through end of file.

## Native ranges

- `src/common/scripting/dap/Nodes/ObjectStateNode.cpp` **ObjectStateNode::SerializeToProtocol** 1–196 (196 lines)

**Credited native LOC:** 196 (tally `nativeLoc` 196).

## Managed

`VmObjectStateNode1384Slice.cs`
Managed LOC (tally): 271.
Conversion commit: `363e72f4c5bb4f472c2cc79c3aab61caccf8ee55`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Preamble and includes (**1–31**) | yes |
| Constructor (**33–36**) | yes |
| `SerializeToProtocol` (**38–77**) | yes |
| `GetChildNames` (**79–168**) | yes |
| `GetChildNode` (**170–183**) | yes |
| `Reset` (**185**) | yes |
| `GetVirtualContainerChildren` (**187–195**) | yes |
| Namespace close (**196**) | yes |
| Band hull (**1–196**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Header `IsVirtualStructure` | inline, not this cpp |
| `SetVariableName` and validity helpers | callees |
| Inner null field branch | unreachable after the earlier null return |

A null address formats as the declared type plus ` <NULL>` and does not log. A valid object whose VM class is the declared class prints that name and the `0x` pointer. A descendant prints the VM class and the pointer. A class that is not a descendant prints the VM class, the pointer, and ` as ` the declared type. A subview pointer keeps the declared type. A non-pointer subview leaves the value unset. Human lookup is case-insensitive. `GetChildNames` returns the cache without reading fields again. A recoverable read removes the parent from the name list, keeps it in the virtual map, and keeps the actual class field names once. The break skips a second append of the failing field. `Reset` clears the child map and leaves the cache, so the next lookup misses. `GetVirtualContainerChildren` returns a copy. An invalid object keeps a zero reference and still prints the raw pointer. A non-class type logs the failure and `SerializeToProtocol` still returns true.

| Managed (`VmObjectStateNode1384Slice.cs`) | Native (`ObjectStateNode.cpp`) |
|------------------------------------------------|--------------------------------|
| Object node protocol text and children | **1–196** |
| Band hull (exclusive slice) | **1–196** |

### Chunk **1384** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **196** |
| **Net headline** | **+196** vs the prior chunk |


**Post-review ratio:** 271/196 managed/native ≈ 138% (src file LOC at HEAD; see tally).

## Tests

- `VmObjectStateNode1384SliceTests.SerializeFormatsNullSubclassUnrelatedAndSubview`
- `VmObjectStateNode1384SliceTests.RetryDropsTheParentNameAndResetKeepsTheCache`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1384 --to 1384` (0 hard / 0 soft).

## Seams

IsVirtualStructure is inline in the header and is not part of this cpp. SetVariableName, IsVMValValidDObject, GetVMValueVar, CreateNodeForVariable, and LogError stay callees. The inner null-object field branch is unreachable after the earlier null return. A recoverable field read clears the name cache and rescans the object's class. The break skips the failing field's extra name append. Reset clears children only.

## Credit rationale

UNCredited ObjectStateNode.cpp 1-196 (+196). After chunk 1383 this is the unique largest hole. DebugServer.cpp 1-194 is 194. Gaps longer than 400 lines stay whole and are not sliced.
