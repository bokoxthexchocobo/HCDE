# Chunk 1341 — JIT byte, word, float, string, object, vector, and bit stores from jit_store.cpp (1-314).

## Scope

JIT byte, word, float, string, object, vector, and bit stores from jit_store.cpp (1-314).

## Native ranges

- `src/common/scripting/jit/jit_store.cpp` **EmitSB** 1–314 (314 lines)

**Credited native LOC:** 314 (tally `nativeLoc` 314).

## Managed

`VmJitStore1341Slice.cs`
Managed LOC (tally): 296.
Conversion commit: `a966e650bffcc000c5b742ce18e7a35e2b2c2b9e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Byte and word stores (**26–48**) | yes |
| Dword stores (**50–60**) | yes |
| Single-float stores (**62–78**) | yes |
| Double stores (**80–90**) | yes |
| String stores (**92–110**) | yes |
| Object and raw pointer stores (**112–142**) | yes |
| Double vector stores (**144–208**) | yes |
| Float vector stores (**210–300**) | yes |
| Bit set and clear (**302–314**) | yes |
| Band hull (**1–314**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| EmitNullPointerThrow | callee; the line note and the nil test |
| CallAssignString | callee; copies the string |
| GC::WriteBarrier | callee; color test stays in the collector |
| asmjit address modes | callee; little-endian width is the store |

A nil base throws X_WRITE_NIL and does not add the displacement. Byte, word, and dword stores write the low 8, 16, or 32 bits. Single stores convert the double with round-to-nearest and write 4 bytes; the cleared temporary lanes are not stored. Double stores write 8 bytes. String stores copy the register string, and a null source pointer faults. Object stores write the pointer and then call the barrier, including when the pointer is zero. Raw pointer stores skip the barrier. Double vectors are 8 bytes apart. Float vectors convert each lane and are 4 bytes apart. The bit store uses offset zero. A zero D register clears the mask in C, and any other 32-bit value sets it. The mask is the instruction C byte, not a constant-pool displacement.

| Managed (`VmJitStore1341Slice.cs`) | Native (`jit_store.cpp`) |
|------------------------------------|--------------------------|
| Byte through bit stores | **1–314** |
| Band hull (exclusive slice) | **1–314** |

### Chunk **1341** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **314** |
| **Net headline** | **+314** vs the prior chunk |


**Post-review ratio:** 296/314 managed/native ≈ 94% (src file LOC at HEAD; see tally).

## Tests

- `VmJitStore1341SliceTests.ScalarStoresNullAndStrings`
- `VmJitStore1341SliceTests.ObjectsVectorsAndBits`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1341 --to 1341` (0 hard / 0 soft).

## Seams

EmitNullPointerThrow records the line and aborts when regA[A] is zero, before the displacement is added. CallAssignString copies the FString. GC::WriteBarrier is called only by the object stores, including a null object. Its white and propagate test stays in the collector. Displacements are signed 32-bit. Pointer stores are 8 bytes on this x64 emitter.

## Credit rationale

UNCredited jit_store.cpp 1-314 (+314). A full tally line scan shows no overlap. This whole file is the largest remaining hole of at most 400 lines, ahead of jit_move.cpp 1-312.
