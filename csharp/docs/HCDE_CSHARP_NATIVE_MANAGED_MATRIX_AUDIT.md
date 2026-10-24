# Principal audit — native-to-managed support matrix

**Date:** 2026-10-19  
**Scope:** Inventory generator, committed `HCDE_CSHARP_NATIVE_MANAGED_MATRIX.md`, and `docs/data/native_managed_matrix.json`.  
**Result:** Accepted for inventory use. This is a coverage map, not a parity certification.

## What was delivered

- `csharp/tools/generate_native_managed_matrix.py` rebuilds the matrix from `AcsPcode`, `AcsVm.cs`, `MapBehaviorBytecodeWalker.cs`, `vmops.h`, `VmExec.cs`, and `p_acs.cpp` `EACSFunctions` names.
- `HCDE.MigrationMatrix.Tests` verifies the generator reproduces committed summary JSON.
- LOC scope: **508,086** native engine lines under `src/**/*.{{cpp,c}}`; **95,649+** managed lines under `csharp/src` (regenerated totals in JSON).

## Headline numbers (generated)

See the committed matrix for exact counts. ACS word opcodes are mostly **executed** or **decoded-only**; VM opcodes remain predominantly **absent** outside the integer core; ACS `CallFunc` entries are mostly **absent** with a growing **executed** subset in `AcsCallFunctions.cs`.

## Limitations

- VM opcode matching uses name heuristics between `vmops.h` and `VmOp`; misclassified rows are possible.
- `EACSFunctions` wire indices are not fully correlated to managed `case` constants in this inventory pass.
- Line specials, actor classes, and save fields are not yet separate rows (next matrix revision).
- Partial ACS rows are maintained in the generator script (`ACS_PARTIAL`).

## Test evidence

```text
dotnet test csharp/tests/HCDE.MigrationMatrix.Tests/HCDE.MigrationMatrix.Tests.csproj -c Release
python3 csharp/tools/generate_native_managed_matrix.py
```

Both succeeded in this run.

## LOC tally

`LOC tally: converted this run = 0 native lines (312 C# lines added); native remaining = 508,086`
