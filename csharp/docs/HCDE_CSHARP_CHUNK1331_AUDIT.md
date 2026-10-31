# Chunk 1331 — JIT test, jump, indexed jump, scope check, return, immediate return, throw, and bounds checks from jit_flow.cpp (1-362), through end of file.

## Scope

JIT test, jump, indexed jump, scope check, return, immediate return, throw, and bounds checks from jit_flow.cpp (1-362), through end of file.

## Native ranges

- `src/common/scripting/jit/jit_flow.cpp` **JitCompiler::EmitTEST** 1–362 (362 lines)

**Credited native LOC:** 362 (tally `nativeLoc` 362).

## Managed

`VmJitFlow1331Slice.cs`
Managed LOC (tally): 324.
Conversion commit: `75ab76c22eb1aeebf071b5819b041e1fa9226386`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| TEST, TESTN, JMP, and IJMP (**26–68**) | yes |
| Scope check and string return helper (**71–94**) | yes |
| RET and RETI (**96–288**) | yes |
| THROW and the three bounds checks (**290–361**) | yes |
| Band hull (**1–362**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| EmitPopFrame | callee |
| FScopeBarrier::ValidateCall | callee, called with b - 1 |
| PCToLine | callee |

TEST compares register D with the unsigned BC immediate and jumps to pc + 2 when they differ. TESTN negates that unsigned immediate before the same jump. JMP adds the signed 24-bit offset and one. IJMP copies D, compares it with each following slot whose opcode is JMP (92), and skips slots that are not jumps. The compile cursor then advances by the signed BC count, including zero, and the miss path throws X_OTHER. SCOPE records the nil-read throw on a forward label, tests register A, and calls validation with the B immediate. The callee subtracts one. RET of NIL pops the frame and returns zero. Any other RET compares the slot with numret, stores the value on the in-range path, and a final bit pops and returns either the next slot or numret. Integer stores are dwords. Float constants move one to four qword bit patterns, with four checked before three and two. Float registers use movsd. Strings add retnum times the host return-slot size and call SetString. Pointer width follows the code generator: qword when 64-bit, dword otherwise. The slot address uses 16 bytes on a 64-bit host and 8 bytes on a 32-bit host. RETI stores the signed 16-bit immediate. THROW passes BC. Each bounds check records the source line, compares, and jumps jae to ThrowArrayOutOfBounds. The message uses a signed compare: index >= size prints the size line, and every smaller index prints the negative-index line.

| Managed (`VmJitFlow1331Slice.cs`) | Native (`jit_flow.cpp`) |
|-----------------------------------|-------------------------|
| Test, jump, return, and bounds | **1–362** |
| Band hull (exclusive slice) | **1–362** |

### Chunk **1331** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **362** |
| **Net headline** | **+362** vs post-**1330** |


**Post-review ratio:** 324/362 managed/native ≈ 90% (src file LOC at HEAD; see tally).

## Tests

- `VmJitFlow1331SliceTests.Flow_BranchesScopeAndIndexedJump`
- `VmJitFlow1331SliceTests.Return_StoresValuesAndBounds`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1331 --to 1331` (0 hard / 0 soft).

## Seams

EmitPopFrame, FScopeBarrier::ValidateCall, and PCToLine stay callees. The assembler cursor places the throw and the bounds call on the forward label, then emits the compare in front of that label.

## Credit rationale

UNCredited jit_flow.cpp 1-362 (+362), through end of file. A full tally line scan shows no overlap. This is the largest remaining hole of at most 400 lines, ahead of c_cvars.cpp 321-676.
