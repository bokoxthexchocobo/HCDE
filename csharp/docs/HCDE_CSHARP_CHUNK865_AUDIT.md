# Chunk 865 — FraggleScript script runner from `fragglescript/t_parse.cpp` (283–451).

## Scope

FraggleScript script runner from `fragglescript/t_parse.cpp` (283–451).

## Native ranges

- `src/playsim/fragglescript/t_parse.cpp` **FParser::PrintTokens** 283–314 (32 lines)
- `src/playsim/fragglescript/t_parse.cpp` **FParser::Run** 323–365 (43 lines)
- `src/playsim/fragglescript/t_parse.cpp` **FParser::RunStatement** 380–451 (72 lines)
- `src/playsim/fragglescript/t_parse.cpp` **parse separators** 315–322 (8 lines)
- `src/playsim/fragglescript/t_parse.cpp` **parse separators** 366–379 (14 lines)

**Credited native LOC:** 169 (tally `nativeLoc` 169).

## Portfolio delta

**Gross CLI band:** **169** lines in the UNCredited-range probe; tally credits **169** whole-function native LOC only.

| | Native LOC |
|---|---:|
| **Gross** (CLI UNCredited band) | **169** |
| **Credited** (whole functions, this row) | **169** |
| **Overlap trim** | **0** |
| **Net headline Δ** | **+169** (see `convertedBehaviorNative` in tally at land commit) |

## Managed

`VmFScriptParserParse865.cs`
Managed LOC (tally): 198.
Conversion commit: `aaba98033e210c6f87970c5bbf9b559b565ee10b`.

## Padding review (2026-10-21)

**Behavior ported**

| Native | Ported |
|--------|--------|
| `PrintTokens` | yes |
| `Run` | yes |
| `RunStatement` | yes |

| Managed (`VmFScriptParserParse865.cs`) | Native (`t_parse.cpp`) |
|----------------------------------------|----------------------|
| `PrintTokens` | **283–314** |
| `RunScript` | **323–365** |
| `RunStatement` | **380–451** |
| (separators) | **315–322**, **366–379** |

### Gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **169** |
| Net | **+169** vs post-**864** |

**Post-review ratio:** 198/169 managed/native ≈ 117% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserParseTests.RunScript_EvaluatesExpression`
- `VmFScriptParserSpecControlTests.SpecIf_FalseWithBraces_SkipsToSectionEnd`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 865 --to 865` (0 hard / 0 soft).

## Seams

`RunStatement` dispatches **859** `spec_*` and **860** `spec_variable`; plain lines call **866** `EvaluateExpression`.

## Credit rationale

Main `Run` loop and statement router after **864** tokenization.
