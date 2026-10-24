# Chunk 857 — FraggleScript preprocessor from `fragglescript/t_prepro.cpp` (59–425).

## Scope

FraggleScript preprocessor from `fragglescript/t_prepro.cpp` (59–425).

## Native ranges

- `src/playsim/fragglescript/t_prepro.cpp` **DFsSection::Serialize** 65–73 (9 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **DFsScript::SectionStart** 81–84 (4 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **DFsScript::SectionEnd** 92–95 (4 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **DFsScript::SectionLoop** 103–106 (4 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **DFsScript::ClearSections** 114–127 (14 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **DFsScript::NewSection** 135–145 (11 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **DFsScript::FindSectionStart** 153–167 (15 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **DFsScript::FindSectionEnd** 176–197 (22 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **DFsScript::ProcessFindChar** 213–303 (91 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **DFsScript::DryRunScript** 328–374 (47 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **DFsScript::Preprocess** 382–387 (6 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **DFsScript::ParseInclude** 401–425 (25 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **prepro separators** 59–64 (6 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **prepro separators** 75–80 (6 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **prepro separators** 87–91 (5 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **prepro separators** 98–102 (5 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **prepro separators** 108–113 (6 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **prepro separators** 129–134 (6 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **prepro separators** 147–152 (6 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **prepro separators** 169–175 (7 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **prepro separators** 199–212 (14 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **prepro separators** 305–327 (23 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **prepro separators** 376–381 (6 lines)
- `src/playsim/fragglescript/t_prepro.cpp` **prepro separators** 389–400 (12 lines)

**Credited native LOC:** 354 (tally `nativeLoc` 354).

## Managed

`VmFScriptPrepro.cs`
Managed LOC (tally): 527.
Conversion commit: `92d09892e657abc2e6354314362b8a6d9dbb4e81`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `DFsSection::Serialize` | yes (field bundle) |
| `SectionStart` / `SectionEnd` / `SectionLoop` | yes |
| `ClearSections` | yes |
| `NewSection` / `FindSectionStart` / `FindSectionEnd` | yes |
| `ProcessFindChar` | yes |
| `DryRunScript` | yes |
| `Preprocess` | yes |
| `ParseInclude` | yes |

| Managed | Native (`t_prepro.cpp`) |
|---------|-------------------------|
| `SectionStart` | **81–84** |
| `SectionEnd` | **92–95** |
| `SectionLoop` | **103–106** |
| `ClearSections` | **114–127** |
| `NewSection` | **135–145** |
| `FindSectionStart` | **153–167** |
| `FindSectionEnd` | **176–197** |
| `ProcessFindChar` | **213–303** |
| `DryRunScript` | **328–374** |
| `Preprocess` | **382–387** |
| `ParseInclude` | **401–425** |
| `DFsSection::Serialize` | **65–73** |
| (separators) | **59–64**, **75–80**, **87–91**, **98–102**, **108–113**, **129–134**, **147–152**, **169–175**, **199–212**, **305–327**, **376–381**, **389–400** |

### Chunk **857** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` (gross **59–425**) | **367** |
| **Net headline** | **+367** vs post-**852** (expected at land sync) |

**Post-review ratio:** 527/354 managed/native ≈ 149% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptPreproTests.SectionSerialize_RoundTripsFields`
- `VmFScriptPreproTests.SectionStart_ReturnsDataOffset`
- `VmFScriptPreproTests.SectionEnd_ReturnsDataOffset`
- `VmFScriptPreproTests.SectionLoop_ReturnsDataOffset`
- `VmFScriptPreproTests.ClearSections_DestroyChain`
- `VmFScriptPreproTests.NewSection_InsertsInHashBucket`
- `VmFScriptPreproTests.FindSectionStart_LocatesBrace`
- `VmFScriptPreproTests.FindSectionEnd_LocatesClosingBrace`
- `VmFScriptPreproTests.ProcessFindChar_BlanksBlockComment`
- `VmFScriptPreproTests.ProcessFindChar_RegistersLabel`
- `VmFScriptPreproTests.DryRunScript_TagsIfSection`
- `VmFScriptPreproTests.Preprocess_SetsLenAndRunsStages`
- `VmFScriptPreproTests.ParseInclude_ReadsLumpAndRunsParser`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 857 --to 857` (0 hard / 0 soft).

## Seams

`ParseInclude` calls host `RunIncludedScript` after lump IO; `DryRunScript` uses in-band `PreproDryRunParser` (`GetTokens` subset faithful to `t_parse.cpp` for section typing only).

## Credit rationale

First credit for `{}` sections, comment blanking, label harvest, dry-run section types, and include lumps before `t_parse.cpp` main loop chunks.
