# Phase 8 audit — ZScript integer core

**Date:** 2026-09-23  
**Scope:** The largest subsystem, `src/common/scripting` (about 44,000 lines). This slice executes the integer half of `vmexec.cpp`: the little-endian `VMOP` word, 256 integer registers, and the arithmetic, compare, and jump opcodes named in `vmops.h`.  
**Result:** Accepted for this slice. Floats, strings, objects, memory loads, calls, the compiler, and the JIT are not executed. The headless client still has its own stack stand-in. No C++ was deleted.

`dotnet test` in `csharp/`: **652 passed, 0 failed.** That is the Phase 7 total of 644 plus 8 VM tests. Local SDK is 10.0.201; `global.json` still pins 8.0.423, so the run moved that file aside and put it back.

## What runs

A word is opcode, register A, then B and C. `Li` writes a signed 16-bit immediate. `Lk` and `AddRk` read the constant table and stop if the index is past the end. `Addi` adds a signed 8-bit immediate.

Add, subtract, multiply, divide, and modulus use registers. Divide or modulus by zero stops the function. `int.MinValue / -1` stops as overflow. And, or, xor, arithmetic right shift, logical right shift, and left shift are included. A shift count outside 0..31 stops the function. Abs, neg, and bitwise not are included.

`EqR`, `LtRr`, and `LeRr` take the following `Jmp` when the compare matches bit 0 of A, and skip that `Jmp` otherwise. A compare whose next word is not `Jmp` stops the function. `Test` skips one instruction when the register is not the unsigned 16-bit immediate. `Jmp` adds its signed 24-bit offset and then advances one word, matching `vmexec.h`. A run that walks off the code, or past 256 instructions, stops.

`Ret` with an integer register and the final-return bit returns that register. `Reti` returns a signed 16-bit immediate. Any other opcode stops the function.

Checked: 6 times 7 is 42. Divide by zero stops. An equal compare jumps to 1 and a less-than of two equal registers falls through to 0. Adding constant 40 and then -2 yields 40. A backward jump counts to 3. Arithmetic shift of -8 is -4, and a logical shift of -1 is the max positive int. Opcode 255 stops.

Left out: the other 200 opcodes, the compiler under `frontend/`, and `libraries/asmjit`.

## Verdict

The ZScript VM now has a tested integer core in `HCDE.Scripting`. It is not `vmexec.cpp`. Calls and memory loads are the next part of this same file, because a script cannot touch the map until those exist.
