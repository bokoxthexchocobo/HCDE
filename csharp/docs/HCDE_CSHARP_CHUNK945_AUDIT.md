# Chunk 945 — `DLevelScript::RunScript` `PCD_ANDLOGICAL` tail through start of `PCD_CLEARLINESPECIAL` from `p_acs.cpp` (**8700–8849**). Follows **916** (**≤8514**); outside deferred **7366–7509**.

## Scope

`DLevelScript::RunScript` `PCD_ANDLOGICAL` tail through start of `PCD_CLEARLINESPECIAL` from `p_acs.cpp` (**8700–8849**). Follows **916** (**≤8514**); outside deferred **7366–7509**.

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::RunScript** 8700–8849 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsVmOpcode945Slice.cs`
Managed LOC (tally): 79.
Conversion commit: `ebea43d1c9d55df5f254e149accd8fed41582f66`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native opcode region | Behavior ported |
|----------------------|-----------------|
| `PCD_ANDLOGICAL` … `PCD_EORBITWISE` malformed guards (**8700–8757**) | yes |
| `PCD_NEGATELOGICAL` / `PCD_NEGATEBINARY` (**8760–8768**) | yes |
| `PCD_LSHIFT` / `PCD_RSHIFT` / `PCD_UNARYMINUS` (**8771–8798**) | yes |
| `PCD_IFNOTGOTO` (**8801–8806**) | yes |
| `PCD_LINESIDE` (**8809–8810**) | yes |
| `PCD_SCRIPTWAIT` / `DIRECT` / `NAMED` (**8813–8844**) | yes |
| `PCD_CLEARLINESPECIAL` head (**8846–8849**) | yes |

| Managed (`VmPAcsVmOpcode945Slice.cs`) | Native (`p_acs.cpp`) |
|----------------------------------------|---------------------|
| `BinaryStackReady` / `PopBinary` / logical+bitwise ops | **8696–8757** |
| `NegateLogical` / `NegateBinary` / shifts / `UnaryMinus` | **8760–8798** |
| `IfNotGotoBranches` / `IfNotGotoAdvancesPcInline` | **8801–8806** |
| `LineSideStackValue` | **8809–8810** |
| `ResolveScriptWaitState` / `ResolveScriptWaitDirect` / named wait helpers | **8813–8844** |
| `ClearLineSpecialHasActivationLine` / `ClearedLineSpecialValue` | **8846–8849** |

### Chunk **945** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**944** |


**Post-review ratio:** 79/150 managed/native ≈ 53% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsVmOpcode945SliceTests.AndLogical_TruthyIntsCombineToOne`
- `VmPAcsVmOpcode945SliceTests.PopBinary_AndBitwise_WritesCombinedValue`
- `VmPAcsVmOpcode945SliceTests.IfNotGotoBranches_WhenConditionZero`
- `VmPAcsVmOpcode945SliceTests.ResolveScriptWaitState_UsesPreWhenNotRunning`
- `VmPAcsVmOpcode945SliceTests.ResolveScriptWaitDirect_CompatPathSkipsPreWait`
- `VmPAcsVmOpcode945SliceTests.EncodeScriptWaitNamedKey_NegatesNameIndex`
- `VmPAcsVmOpcode945SliceTests.ClearLineSpecial_HasActivationLine`
- `VmPAcsVmOpcode945SliceTests.BinaryStackReady_RequiresTwoSlots`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 945 --to 945` (0 hard / 0 soft).

## Seams

Full `RunScript` dispatch, `FBehavior::Ofs2PC`, `controller->RunningScripts`, and linedef mutation remain host seams.

## Credit rationale

Honest **150**-line UNCredited band with zero tally overlap; exclusive `VmPAcsVmOpcode945Slice.cs` (`AcsVm` opcode handlers are not matrix credit).
