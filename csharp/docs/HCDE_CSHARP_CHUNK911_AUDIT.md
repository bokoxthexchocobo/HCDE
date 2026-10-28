# Chunk 911 — `DLevelScript::RunScript` call tail, return opcodes, and `PCD_ADD`…`PCD_LT` from `p_acs.cpp` (**7534–7709**). Chunk **17** keeps **7526–7533** (`PCD_CALLSTACK` head); **7701+** deferred.

## Scope

`DLevelScript::RunScript` call tail, return opcodes, and `PCD_ADD`…`PCD_LT` from `p_acs.cpp` (**7534–7709**). Chunk **17** keeps **7526–7533** (`PCD_CALLSTACK` head); **7701+** deferred.

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::RunScript** 7534–7709 (176 lines)

**Credited native LOC:** 176 (tally `nativeLoc` 176).

## Managed

`VmPAcsVmOpcode911Slice.cs`
Managed LOC (tally): 215.
Conversion commit: `4fb2ed99a2534121cec5445166eec8db786fa927`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native opcode region | Behavior ported |
|----------------------|-----------------|
| `PCD_CALL` / `PCD_CALLDISCARD` else tail (**7534–7570**) | yes |
| `PCD_RETURNVOID` / `PCD_RETURNVAL` (**7573–7607**) | yes |
| `PCD_ADD` … `PCD_NE` (**7609–7696**) | yes |
| `PCD_LT` (**7698–7709**) | yes |

| Managed (`VmPAcsVmOpcode911Slice.cs`) | Native (`p_acs.cpp`) |
|----------------------------------------|---------------------|
| `ResolveEmbeddedCallTarget` / `ResolveCallStackTarget` / `MaskCallStackFuncnum` | **7534–7538**, callstack tag clear **7532–7533** (head credited **17**) |
| `IsFunctionMissing` / `WouldOverflowStackForCall` / `TryEnterScriptFunction` | **7541–7569** |
| `PopReturnValueOperand` / `RestoreReturnFrame` / `ShouldPushReturnValue` | **7573–7607** |
| `TryApplyAdd` | **7609–7620** |
| `TryApplySubtract` | **7622–7633** |
| `TryApplyMultiply` | **7635–7646** |
| `TryApplyDivide` | **7648–7658** |
| `TryApplyModulus` | **7660–7670** |
| `TryApplyEq` | **7672–7683** |
| `TryApplyNe` | **7685–7696** |
| `TryApplyLt` | **7698–7709** |
| `ZeroInitializeLocalSlots` / `CallReturnStackWords` / `BuildCallReturnFrame` | **7557–7564**, frame sizing **7562–7564** |

### Chunk **911** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **176** |
| **Net headline** | **+176** vs post-**910** |


**Post-review ratio:** 215/176 managed/native ≈ 122% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsVmOpcode911SliceTests.ResolveEmbeddedCallTarget_UsesActiveModuleAndByte`
- `VmPAcsVmOpcode911SliceTests.ResolveCallStackTarget_StripsLibraryTag`
- `VmPAcsVmOpcode911SliceTests.WouldOverflowStackForCall_RespectsMargin`
- `VmPAcsVmOpcode911SliceTests.TryApplyAdd_CombinesTopTwoSlots`
- `VmPAcsVmOpcode911SliceTests.TryApplyAdd_MalformedWhenStackTooShallow`
- `VmPAcsVmOpcode911SliceTests.TryApplyDivide_SetsDivideByZeroState`
- `VmPAcsVmOpcode911SliceTests.TryApplyModulus_SetsModulusByZeroState`
- `VmPAcsVmOpcode911SliceTests.TryApplyEq_ReturnsOneWhenEqual`
- `VmPAcsVmOpcode911SliceTests.TryApplyLt_ComparesSecondToTop`
- `VmPAcsVmOpcode911SliceTests.RestoreReturnFrame_PushesValueWhenNotDiscarded`
- `VmPAcsVmOpcode911SliceTests.TryEnterScriptFunction_InitializesLocalsAndAdvancesPc`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 911 --to 911` (0 hard / 0 soft).

## Seams

`GetFunction`, `Printf`, profile `AddRun`, placement-new `CallReturn`, and full `RunScript` loop are not hosted; slice models stack/locals/return frame policy only.

## Credit rationale

Honest **176**-line UNCredited band after **910** (`t_func` dry); exclusive `VmPAcsVmOpcode911Slice.cs` without touching **910** `VmFScriptParserSfMath.cs`.
