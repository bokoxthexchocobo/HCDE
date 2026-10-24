# ZScript VM opcode matrix recognition — chunk 7 audit

## Scope

Matrix generator now classifies `vmops.h` rows by **opcode index** (matching `VmOp` enum order) instead of brittle name aliases. Ninety-four opcodes that were already implemented in `VmExec.cs` (loads/stores, integer/float arithmetic, compares, vectors, strings) move from **absent** to **executed** without double-counting new conversion.

**Recognition native LOC credited:** 20 — only `vmexec.h` handler bodies for opcode families with named regression tests (`ADD_RR`, `SUB_RR`, `MUL_RR`, `EQ_R`, `LT_RR`). Other matrix-executed VM opcodes stay at zero recognition credit until a dedicated test is added.

## Tests

- `VmIntegerArithmeticOpcodeTests.IntegerBinaryOpsWriteRegister` — `ADD_RR`, `SUB_RR`, `MUL_RR`
- `VmIntegerArithmeticOpcodeTests.EqRBranchesWhenRegistersMatch` — `EQ_R`
- `VmIntegerArithmeticOpcodeTests.LtRrBranchesOnOrderedIntegers` — `LT_RR`

## Verified

`python3 tools/generate_native_managed_matrix.py` → `zscriptVmOp.executed: 246`, `absent: 0`.
