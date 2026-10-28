# Chunk 1018 — `DLevelScript::RunScript` `PCD_PRINTFIXED`, `PCD_PRINTNAME`, and script/map/world/global char-array print opcodes from `p_acs.cpp` (**8941–9090**). Follows **945** (**≤8849**); gap **8850–8940** stays on prior credited RunScript bands.

## Scope

`DLevelScript::RunScript` `PCD_PRINTFIXED`, `PCD_PRINTNAME`, and script/map/world/global char-array print opcodes from `p_acs.cpp` (**8941–9090**). Follows **945** (**≤8849**); gap **8850–8940** stays on prior credited RunScript bands.

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::RunScript** 8941–9090 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsVmOpcode1018Slice.cs`
Managed LOC (tally): 107.
Conversion commit: `e0e68b0924d8805133dd129714e747c44840822f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native opcode region | Behavior ported |
|----------------------|-----------------|
| `PCD_PRINTFIXED` (**8939–8942**, in hull) | yes |
| `PCD_PRINTNAME` negative kind table (**8950–8990**) | yes |
| `PCD_PRINTNAME` player/activator resolution (**8994–9022**) | yes |
| `PCD_PRINTSCRIPT*` char arrays (**9027–9041**) | yes |
| `PCD_PRINTMAP*` char arrays (**9043–9057**) | yes |
| `PCD_PRINTWORLD*` char arrays (**9059–9073**) | yes |
| `PCD_PRINTGLOBAL*` char arrays (**9075–9089**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **9091+** | `PCD_PRINTBIND` (next band) |
| **8850–8940** | prior credited RunScript opcodes outside this hull |

| Managed (`VmPAcsVmOpcode1018Slice.cs`) | Native (`p_acs.cpp`) |
|----------------------------------------|---------------------|
| `FormatPrintFixed` | **8940–8941** |
| `TryResolvePrintNameSpecial` | **8950–8990** |
| `ResolvePrintNamePlayer` | **8994–9022** |
| `TryBuildPrintCharArray` / `AppendCharArrayRun` | **9027–9089** |
| Band hull (exclusive slice) | **8941–9090** |

### Chunk **1018** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1017** |


**Post-review ratio:** 107/150 managed/native ≈ 71% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsVmOpcode1018SliceTests.FormatPrintFixed_UsesInvariantDouble`
- `VmPAcsVmOpcode1018SliceTests.TryResolvePrintNameSpecial_MapsNegativeKinds`
- `VmPAcsVmOpcode1018SliceTests.ResolvePrintNamePlayer_UsesActivatorWhenIndexZero`
- `VmPAcsVmOpcode1018SliceTests.TryBuildPrintCharArray_AppendsUntilNul`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1018 --to 1018` (0 hard / 0 soft).

## Seams

Full `RunScript` dispatch, `work`/`lookup` string builders, `CharArrayParms` host arrays, `HCDE_ResolveACSPlayer`, and `Level->Behaviors.LookupString` remain native.

## Credit rationale

First tally-clean **150**-line UNCredited `p_acs.cpp` band after **945** with no overlap to retired/off-tally ACS string or opcode VM slices.
