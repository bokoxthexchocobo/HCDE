# ZScript VM pointer arithmetic — chunk 7 audit

## Scope

| Native (`vmops.h` / `vmexec.h`) | Managed |
| --- | --- |
| `ADDA_RR` / `ADDA_RK` | `VmOp.AddARr`, `VmOp.AddARk`, `VmExec` |
| `SUBA` | `VmOp.SubA` |
| `EQA_R`, `EQA_K`, `NULLCHECK` | Reindexed to opcodes 243–245 (`EqaR`, `EqaK`, `NullCheck`) |

**Behavior native LOC credited (conversion):** 52 (`vmexec.h` `OP(ADDA_RR)`–`OP(SUBA)`).

## Tests

`VmPointerArithmeticOpcodeTests.*`, existing `Phase8VmTests.PointerEqualityComparesAllocationAndByteOffset`.

## Verified

`dotnet build -warnaserror`, `HCDE.Scripting.Tests` and `HCDE.Playsim.Tests` green.
