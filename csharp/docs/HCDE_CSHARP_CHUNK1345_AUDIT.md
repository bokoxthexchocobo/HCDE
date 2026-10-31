# Chunk 1345 — Boolean-not compare, sizeof and alignof, and pre/post increment from codegen.cpp (2386-2684).

## Scope

Boolean-not compare, sizeof and alignof, and pre/post increment from codegen.cpp (2386-2684).

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxPreIncrDecr** 2386–2684 (299 lines)

**Credited native LOC:** 299 (tally `nativeLoc` 299).

## Managed

`VmCodegen1345Slice.cs`
Managed LOC (tally): 607.
Conversion commit: `61cbe6a4f68d8400b45fd733b6ecc285bf0fc8c9`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Compare forward (**2386–2397**) | yes |
| Size and align (**2405–2458**) | yes |
| Pre increment (**2466–2552**) | yes |
| Post increment (**2560–2655**) | yes |
| Assignment shell (**2663–2684**) | yes |
| Band hull (**2386–2684**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Unary-not emit body | the XOR and its return sit before 2386 |
| Assignment resolve | starts at 2685 |
| Commented address request | the override is inside a comment and stays unimplemented |

Boolean-not compare passes both patch lists through and flips invert. A constant sizeof names size, and a constant alignof names alignment; neither asks for an address. A null writable pointer does not throw. A failed address request names alignment or size to match the operator. Success deletes the operand and returns a resolved constant of Align or Size. Any operator other than alignof uses Size. A null operand resolve deletes this expression and adds no message. A second resolve returns this and does not resolve the operand again. The size emit is a nil register. Pre-increment address request before resolve returns true and writes false. A non-numeric operand is rejected before an address request. Bool uses the quoted token name. An address request that writes true and returns false still errors, and the writable flag stays true. Success stores that flag, and a later address request reports it. A memory pre-increment loads, adds the immediate, and stores. The integer pool holds 0; the added 1 is not a pooled constant. A register pre-increment that was asked for its address frees the copied emit and returns the original pointer. A register float decrement that was not asked returns a copy with Target clear and the original freed, using subtract and the float constant 1. A bad token throws before any constant is pooled. A memory post-increment ignores a false result flag and still returns the loaded value. A memory float decrement subtracts. A register post-increment with a result moves, then adds. A register float post-increment moves float, then adds the pooled 1. A register post-decrement with no result adds the immediate 255 and returns nil. Assignment defaults the bit-write field to -1 and the modify flag to false. Delete drops both sides. Its address request is the base expression: it writes false, returns false, and leaves the requested flag clear. The modify constructor stores true.

| Managed (VmCodegen1345Slice.cs) | Native (codegen.cpp) |
|----------------------------------|----------------------|
| Compare, size, increment, and the assignment shell | **2386–2684** |
| Band hull (exclusive slice) | **2386–2684** |

### Chunk **1345** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **299** |
| **Net headline** | **+299** vs the prior chunk |


**Post-review ratio:** 607/299 managed/native ≈ 203% (src file LOC at HEAD; see tally).

## Tests

- `VmCodegen1345SliceTests.CompareSizeAndPreIncrement`
- `VmCodegen1345SliceTests.PostIncrementAndAssign`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1345 --to 1345` (0 hard / 0 soft).

## Seams

Boolean-not compare forwards the same patch lists and flips invert. sizeof and alignof reject a constant before they ask for an address, then return a resolved integer constant and delete the operand. Pre and post increment require a numeric modifiable value and reject bool. The decrement immediate is the low 8 bits of -1. An ExpEmit assignment is a copy, so freeing it does not mark the original. A memory post-increment still returns the old value when the result is not needed. Assignment stores the modify flag and keeps its address request commented out.

## Credit rationale

UNCredited codegen.cpp 2386-2684 (+299). A full tally line scan shows no overlap. This is the largest remaining hole of at most 400 lines, ahead of the 289-line tie between ValueStateNode.cpp and i_steam.cpp.
