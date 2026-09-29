# Phase 7 audit — ACS core

**Date:** 2026-09-22  
**Scope:** The largest single C++ file, `p_acs.cpp` (about 11,000 lines). This slice executes the word-format arithmetic, compares, script and map variables, direct line specials 1 through 5, thing count, and the small game queries.  
**Result:** Accepted for this slice. Print, strings, world variables, inventory, and the ZScript VM in `src/common/scripting` (about 44,000 lines, the largest subsystem) are not executed. No C++ was deleted.

`dotnet test` in `csharp/`: **644 passed, 0 failed.** That is the Phase 6 total of 638 plus 6 ACS tests. Local SDK is 10.0.201; `global.json` still pins 8.0.423, so the run moved that file aside and put it back.

## What runs

Each fiber has 20 script variables. The VM has 32 map variables, shared by every script. An index outside that range ends the fiber.

Arithmetic is add, subtract, multiply, divide, and modulus. Divide or modulus by zero pushes 0. Compares push 1 or 0. Logical and, or, and not, plus bitwise and, or, xor, and both shifts, are on the stack. `IfNotGoto` jumps when the top is 0.

`RandomDirect` is inclusive and deterministic from a fixed generator. `ThingCountDirect` counts actors of that editor number when the tid is 0. A non-zero tid pushes 0, because actors do not store tids. `PlayerCount` is the player list. `GameType` is 0. `GameSkill` is 3. `Timer` is the tic. `ClearLineSpecial` does nothing, because the fiber does not remember an activator line. `Lspec2Direct` through `Lspec5Direct` run the special with the first argument as the tag and read the rest.

Checked: 6 times 7 equals 42 and the level exits. 1 divided by 0 is 0. Script variable 0 goes from 9 to 13. A map variable written by script 1 is read by script 2. One thing of type 3001 is counted, and the same count with tid 7 is 0. On tic 1, the timer and the one player combine with logical and.

Left out: the rest of `p_acs.cpp`, and the whole VM under `src/common/scripting`.

## Verdict

The largest source file now has a wider tested opcode set. It is not replaced. The largest subsystem, the ZScript VM, is the next component.
