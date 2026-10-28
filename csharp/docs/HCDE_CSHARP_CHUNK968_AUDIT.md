# Chunk 968 — Baseline ACS P-code enum opcodes **0–69** (`PCD_NOP` … `PCD_RESTART`) from `p_acs.cpp` (**1–150**).

## Scope

Baseline ACS P-code enum opcodes **0–69** (`PCD_NOP` … `PCD_RESTART`) from `p_acs.cpp` (**1–150**).

## Native ranges

- `src/playsim/p_acs.cpp` **PCD_NOP** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsPcodeEnum968Slice.cs`
Managed LOC (tally): 119.
Conversion commit: `f9c54c72e984b96e2e7faaac3d9bb9e4e77a7b5c`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| ACS P-code enum **0–69** (**78–149**) | yes (constants + classification helpers) |
| Copyright/includes (**1–76**) | layout only |
| Comment `/* 70*/` line (**150**) | layout only (next opcode starts after band) |

| Managed (`VmPAcsPcodeEnum968Slice.cs`) | Native (`p_acs.cpp`) |
|----------------------------------------|---------------------|
| `Pcd*` constants | **80–149** |
| `IsBaselineOpcode` / `IsLspec*` / `LspecArgCount` | enum structure |
| `IsCompareOpcode` / `IsBinaryArithmeticOpcode` | **94–124** clusters |
| `BaselineOpcodeLabel` | symbolic names |
| Band hull (exclusive slice) | **1–150** |

### Chunk **968** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**967** |


**Post-review ratio:** 119/150 managed/native ≈ 79% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsPcodeEnum968SliceTests.BaselineOpcodeMax_MatchesNativeRestart`
- `VmPAcsPcodeEnum968SliceTests.LspecArgCount_StackAndDirect`
- `VmPAcsPcodeEnum968SliceTests.CompareAndArithmeticRanges`
- `VmPAcsPcodeEnum968SliceTests.BaselineOpcodeLabel_KnownNames`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 968 --to 968` (0 hard / 0 soft).

## Seams

Include list and big-endian comment block remain host-only; interpreter dispatch for these opcodes lives in later `p_acs.cpp` bands.

## Credit rationale

Primary **968** target `a_action.cpp` **1–150** rejected (`VmAAction.cs` off-tally duplicate); `p_map` **6119–6268** rejected (`VmPMapRadiusDamage` **6130–6480**). First alternate UNCredited playsim band with zero conversion/recognition/Vm overlap.
