# Chunk 857 — FraggleScript preprocessor from `fragglescript/t_prepro.cpp` (59–425). Retired (2026-10-23): out-of-body credit removed; honest **146** < **150**.

## Scope

FraggleScript preprocessor from `fragglescript/t_prepro.cpp` (59–425). Retired (2026-10-23): out-of-body credit removed; honest **146** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `c7ea812156fe712e89e39fa02fb4b0bb46f9b92a`.

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

### Out-of-body uncredit (857)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **354** |
| **Out-of-body removed** | **208** |
| **Honest** | **146** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 857 --to 857` (0 hard / 0 soft).

## Seams

`ParseInclude` calls host `RunIncludedScript` after lump IO; `DryRunScript` uses in-band `PreproDryRunParser` (`GetTokens` subset faithful to `t_parse.cpp` for section typing only).

## Credit rationale

First credit for `{}` sections, comment blanking, label harvest, dry-run section types, and include lumps before `t_parse.cpp` main loop chunks.
