# Chunk 1355 — Debugger array state node from ArrayStateNode.cpp (1-259), through end of file.

## Scope

Debugger array state node from ArrayStateNode.cpp (1-259), through end of file.

## Native ranges

- `src/common/scripting/dap/Nodes/ArrayStateNode.cpp` **ArrayStateNode** 1–259 (259 lines)

**Credited native LOC:** 259 (tally `nativeLoc` 259).

## Managed

`VmArrayStateNode1355Slice.cs`
Managed LOC (tally): 382.
Conversion commit: `1560ba2e7d074bc57492790b82af3d8ae7315d6a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Element type and element count (**33–87**) | yes |
| Constructor, protocol value, and child names (**89–146**) | yes |
| Child lookup, element reads, and the fixed-array address (**148–257**) | yes |
| Band hull (**1–259**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| NewPointer | the pointer wrapper for an element larger than 8 bytes is supplied by the caller |
| GetVMValue | the fixed-array load is supplied by the caller |
| CreateNodeForVariable | the child node is supplied by the caller |
| CacheChildren | the header declares it and this file does not define it |

A pointer type unwraps before the array or dynamic-array test and does not dereference the array address. Element type is set for an array, including a static array that is also an array, and for a dynamic array. A static array that is not an array leaves the element type unset. Dynamic and static counts read the header count when the word is valid: count 4294967295 returns -1, and an invalid word returns 0. A fixed array returns its element count with no validity test, and that same 4294967295 returns -1. Anything else returns 0. Validity is a non-null word whose address is non-zero and whose low 32 bits are non-zero. Serialize always stores GetId, then the clamped index count, then the name. A matching evaluate name is left alone. A null element type throws after that name write. An invalid word or a negative count formats as element[<NONE>]. Otherwise the value is the element name, the clamped count, and, for a pointer type, glibc %p of the address. Child names are appended and are not cleared. An invalid word returns before the count loop. A negative count does not iterate. Child lookup returns false for an invalid word, a non-positive count, or an index outside 0..count-1, and leaves the output node unchanged. ParseInt is std::stoi: C whitespace, an optional sign, digits, and trailing junk. No digits throws FormatException. A value outside int throws OverflowException. The cache key is the canonical decimal, so 12abc and 0012 share one node. A dynamic or static element larger than 8 bytes uses the injected pointer type. Float size 8 stores a double, size 4 stores the double of that float, and any other float size stores a zero word. Object pointers and pointers read 8-byte elements even when the type size is 4. The string type stores the address of the element. Sizes 8, 4, 2, and 1 store the little-endian bits. Any other size stores the data address plus index times size. A fixed array calls GetVMValue at the original address plus element size times index and creates the node with the element type. A missing delegate throws InvalidOperationException.

| Managed (VmArrayStateNode1355Slice.cs) | Native (ArrayStateNode.cpp) |
|----------------------------------------|--------------------------------|
| Array state node, counts, and children | **1–259** |
| Band hull (exclusive slice) | **1–259** |

### Chunk **1355** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **259** |
| **Net headline** | **+259** vs the prior chunk |


**Post-review ratio:** 382/259 managed/native ≈ 147% (src file LOC at HEAD; see tally).

## Tests

- `VmArrayStateNode1355SliceTests.ProtocolCountsAndNames`
- `VmArrayStateNode1355SliceTests.ChildrenElementsAndParse`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1355 --to 1355` (0 hard / 0 soft).

## Seams

NewPointer, GetVMValue, and CreateNodeForVariable stay injected. CacheChildren is declared on the header and has no body in this file. The commented pointer dereference stays unused, so the array address is the original VM word.

## Credit rationale

UNCredited ArrayStateNode.cpp 1-259 (+259). A full tally line scan shows no overlap. This is the largest remaining hole of at most 400 lines, ahead of the 257-line tie between i_interface.cpp 1-257 and p_saveg.cpp 336-592.
