# Chunk 865 — FraggleScript script runner from `fragglescript/t_parse.cpp` (283–451).

## Scope

FraggleScript script runner from `fragglescript/t_parse.cpp` (283–451).

## Native ranges

- `src/playsim/fragglescript/t_parse.cpp` **FParser::PrintTokens** 283–314 (32 lines)
- `src/playsim/fragglescript/t_parse.cpp` **FParser::Run** 323–365 (43 lines)
- `src/playsim/fragglescript/t_parse.cpp` **FParser::RunStatement** 380–451 (72 lines)
- `src/playsim/fragglescript/t_parse.cpp` **parse separators** 315–322 (8 lines)

**Credited native LOC:** 155 (tally `nativeLoc` 155).

## Portfolio delta

**Gross CLI band:** **169** lines in the UNCredited-range probe; tally credits **155** whole-function native LOC only.
**14** uncovered lines in that band are inter-function gaps, comments, or non-function lines (not credited).

| | Native LOC |
|---|---:|
| **Gross** (CLI UNCredited band) | **169** |
| **Credited** (whole functions, this row) | **155** |
| **Overlap trim** | **0** |
| **Net headline Δ** | **+155** (see `convertedBehaviorNative` in tally at land commit) |

## Managed

`VmFScriptParserParse865.cs`
Managed LOC (tally): 198.
Conversion commit: `62f44dcea0830782aa09982955f8e1a1f3b5c940`.

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

### Out-of-body uncredit (865)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **169** |
| **Out-of-body removed** | **14** |
| **Honest** | **155** |


**Post-review ratio:** 198/155 managed/native ≈ 128% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserParseTests.RunScript_TwoStatements_AdvancesPastBoth`
- `VmFScriptParserParseTests.RunScript_EvaluatesExpression`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 865 --to 865` (0 hard / 0 soft).

## Seams

`RunStatement` dispatches **859** `spec_*` and **860** `spec_variable`; plain lines call **866** `EvaluateExpression`.

## Credit rationale

Main `Run` loop and statement router after **864** tokenization.
