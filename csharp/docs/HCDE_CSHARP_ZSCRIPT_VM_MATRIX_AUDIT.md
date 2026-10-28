# Principal audit — ZScript VM op matrix accuracy

**Date:** 2026-10-19  
**Scope:** `generate_native_managed_matrix.py` VM op classifier vs `VmExec.cs` / `VmOp.cs`.  
**Result:** Accepted; matrix now marks load/store pointer and float vector ops as executed when implemented in `VmExec`.

## Notes

- `CallK`, float/string/object memory ops already have extensive coverage in `Phase8VmTests.cs`.
- Remaining VM gaps are classifier-only unless new `vmops.h` entries appear.

## LOC tally

`LOC tally: converted this run = 45 native lines (85 C# lines added); native remaining = 503,765`
