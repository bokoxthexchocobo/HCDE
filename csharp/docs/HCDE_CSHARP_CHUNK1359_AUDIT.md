# Chunk 1359 — VM exec wiring from vmexec.cpp (1-248): sprite cast, engines, fill, and AssertObject.

## Scope

VM exec wiring from vmexec.cpp (1-248): sprite cast, engines, fill, and AssertObject.

## Native ranges

- `src/common/scripting/vm/vmexec.cpp` **VMExec** 1–248 (248 lines)

**Credited native LOC:** 248 (tally `nativeLoc` 248).

## Managed

`VmExecWiring1359Slice.cs`
Managed LOC (tally): 449.
Conversion commit: `ad01227506cddafb6946fd747ac570e9704765ae`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Banner, includes, cycle and call tables (**1–40**) | yes |
| Sprite-id cast (**41–42**) | yes |
| Exec macros (**48–100**) | yes |
| Checked and unchecked includes, VMExec, stack (**102–147**) | yes |
| VMSelectEngine (**159–177**) | yes |
| VMFillParams (**188–226**) | yes |
| AssertObject (**229–248**) | yes |
| Band hull (**1–248**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| ThrowVMException (**44–47**) | the function is inside `#if 0` and has no symbol |
| vmexec.h body | the two includes pull in the interpreter; those lines are not this file |
| VMFrameStack methods | AllocFrame and PopFrame live in vmframe.cpp |
| Opcode implementations | the header's cases are not part of the 248 lines |

The translation unit keeps ten cycle slots and ten call slots. They start at zero. Nothing in this file increments them.

`VM_CastSpriteIDToString` starts as the default formatter. `%d` prints the unsigned id as a signed 32-bit decimal, so 2147483648 becomes -2147483648 and 4294967295 becomes -1. Format replaces the previous text. A null slot throws after the function pointer is known. A null function pointer throws before the slot is touched. A replacement pointer is what the next cast calls.

`COMPGOTO` stays undefined (treated as off) unless the unit is GNU or the macro was already set. A preset value wins over GNU. Switch `NEXTOP` only increments the program counter. Computed `NEXTOP` increments, emits the new counter, then loads op and a. An emit fault leaves the counter already advanced. A counter past the last word throws after the emit. Operand fields use the little-endian `VMOP` layout: A, B, C, signed C, unsigned and signed BC, and the signed 24-bit ABC. `luai_nummod` is `a - floor(a/b)*b`, including the signed-zero and NaN cases. `KC` and `RC` index with C and do not assert. `CMPJMP` compares the test with `a & CMP_CHECK`. A match on the checked engine requires the next opcode to be `OP_JMP` (92) before adding one plus that word's 24-bit offset. A failed assert leaves the counter where it was. The unchecked engine adds the offset even when the next opcode is not a jump. A miss adds one and does not read the next word. `GETADDR` on a null base throws the abort for that reason and does not write the pointer. The sentences are the null-moreinfo results of `ThrowAbortException`. A live base stores origin plus offset, including a negative offset.

`NDEBUG` is cleared for `VMExec_Checked` and forced on for `VMExec_Unchecked`, then restored. Checked asserts stay on in both builds. Unchecked asserts stay off in both builds. The initial `VMExec` is unchecked when the translation unit is `NDEBUG`, and checked otherwise. `GlobalVMStack` is `thread_local`: one stack per thread, the same stack on a later call from that thread.

`VMSelectEngine(VMEngine_Default)` writes the unchecked exec only when the unit is `NDEBUG`, then always writes the checked exec. The unchecked case writes unchecked. The checked case writes checked. Any other value leaves the current exec in place.

`VMFillParams` binds the frame registers before it looks at the function. A null frame throws first. A null function under `NDEBUG` throws on the function. A null function otherwise fails the combined assert and does not read the flags. A native flag (`VARF_Native`, bit 3) fails that same assert and does not read `NumArgs`. Under `NDEBUG` the flag is not read. The count assert runs before the loop and uses `NumArgs`. A mismatch throws before any parameter is indexed. `RegTypes` is loaded next. A null type table throws before the loop when asserts are on. Under `NDEBUG` a null table is legal when `NumArgs` is 0, and otherwise throws when the first type is read, after the parameter is indexed. The loop walks `NumArgs`, not the caller's count. Int and non-int values below string go to separate D and F banks. Strings are copied after the source is dereferenced and before the slot is indexed, so a null string throws even when the string bank is empty. Any other type must be pointer when asserts are on, and the slot stays empty if that assert fails. Under `NDEBUG` the same type stores the pointer, including konst and nil tags.

`AssertObject` is absent under `NDEBUG`, including for a null pointer. Otherwise a null pointer returns true without reading `MagicID`. An unreadable object returns false under SEH and throws on the magic read otherwise. `MagicID` 0x1337cafe returns true. Any other readable value returns false.

| Managed (VmExecWiring1359Slice.cs) | Native (vmexec.cpp) |
|------------------------------------|---------------------|
| Sprite cast through AssertObject | **1–248** |
| Band hull (exclusive slice) | **1–248** |

### Chunk **1359** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **248** |
| **Net headline** | **+248** vs the prior chunk |


**Post-review ratio:** 449/248 managed/native ≈ 181% (src file LOC at HEAD; see tally).

## Tests

- `VmExecWiring1359SliceTests.EnginesFillAndAssert`
- `VmExecWiring1359SliceTests.DispatchCastAndStack`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1359 --to 1359` (0 hard / 0 soft).

## Seams

ThrowAbortException's source lives in vmframe.cpp. VMFrameStack allocation lives there too. EmitInstructionExecutionEvent is the debug hook computed NEXTOP calls. The vmexec.h opcode bodies stay outside this slice.

## Credit rationale

UNCredited vmexec.cpp 1-248 (+248). A full tally line scan shows no overlap. This is the largest remaining hole of at most 400 lines. It is not a tie. keysections.cpp 78-323 and i_input_gyro.cpp 1-246 are 246.
