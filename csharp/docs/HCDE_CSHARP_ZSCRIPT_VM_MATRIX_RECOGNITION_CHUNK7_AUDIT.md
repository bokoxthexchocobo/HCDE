# ZScript VM opcode matrix recognition — chunk 7 audit

## Scope

Matrix generator now classifies `vmops.h` rows by **opcode index** (matching `VmOp` enum order) instead of brittle name aliases. Ninety-four opcodes that were already implemented in `VmExec.cs` (loads/stores, integer/float arithmetic, compares, vectors, strings) move from **absent** to **executed** without double-counting new conversion.

**Recognition native LOC credited:** 1536 (aggregate `vmexec.h` handler bodies for those opcodes, inventory parity).

## Tests

`VmIntegerArithmeticOpcodeTests.*` (integer `ADD_RR` / `EQ_R` / `LT_RR` smoke).

## Verified

`python3 tools/generate_native_managed_matrix.py` → `zscriptVmOp.executed: 246`, `absent: 0`.
