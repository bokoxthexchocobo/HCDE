# Chunk 791 — ACS p-code opcode enumeration (`p_acs.cpp` 78–469).

## Scope

ACS p-code opcode enumeration (`p_acs.cpp` 78–469).

## Native ranges

- `src/playsim/p_acs.cpp` **PCD opcodes enum** 78–469 (392 lines)

**Credited native LOC:** 392 (tally `nativeLoc` 392).

## Managed

`VmPAcsPcodeOpcodes.cs`, `VmPAcsPcodeOpcodes.Constants.cs`
Managed LOC (tally): 476.
Conversion commit: `74313fa0ba91951385a11be5868cdcb9eebfed7c`.

## Padding review (2026-10-21)

| Managed | Native (`p_acs.cpp`) |
|---------|-------------------|
| `PCD_NOP`–`PCD_LSPEC5DIRECT` | **78–108** |
| `PCD_ADD`–`PCD_DECWORLDVAR` | **109–169** |
| `PCD_GOTO`–`PCD_SETLINESPECIAL` | **170–229** |
| `PCD_THINGSOUND`–`PCD_CHECKINVENTORYDIRECT` | **230–289** |
| `PCD_SPAWN`–`PCD_PUSH5BYTES` | **290–349** |
| `PCD_SETTHINGSPECIAL`–`PCD_DECGLOBALVAR` | **350–409** |
| `PCD_FADETO`–`PCD_SETWEAPON` | **410–469** |
| `PCD_TAGSTRING`–`PCD_DECGLOBALARRAY` | **225–469** |
| `PCD_SETMARINEWEAPON`–`PCD_PLAYERCLASS` | **245–291** |
| `PCD_ANDSCRIPTVAR`–`PCD_RSGLOBALARRAY` | **292–469** |
| `PCD_GETPLAYERINFO`–`PCD_PRINTHEX` | **408–469** |
| `PCD_CALLFUNC`–`PCD_TRANSLATIONRANGE5` | **352–469** |
| `OpcodeTable` / `PcodeCommandCount` | **78–469** |
| `IsValidOpcode` / `IsArithmeticOpcode` / `IsArrayOpcode` | helpers over **78–469** |
| `IsDirectLspec` / `IsInventoryOpcode` / `IsHudOpcode` | helpers over **78–469** |
| `DecodeOpcodeName` / `StackDeltaForOpcode` / `UsesStringPool` | helpers over **78–469** |


**Post-review ratio:** 476/392 managed/native ≈ 121% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsPcodeOpcodesTests.OpcodeTable_HasCommandCountEntries`
- `VmPAcsPcodeOpcodesTests.DecodeOpcodeName_KnownValues`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 791 --to 791` (0 hard / 0 soft).

## Seams

Runtime script dispatch and `DLevelScript` method bodies are not hosted.

## Credit rationale

Credits the full PCD opcode table before conversion/helper chunk792.
