# Chunk 864 — FraggleScript tokenizer from `fragglescript/t_parse.cpp` (31–274).

## Scope

FraggleScript tokenizer from `fragglescript/t_parse.cpp` (31–274).

## Native ranges

- `src/playsim/fragglescript/t_parse.cpp` **FParser::NextToken** 40–111 (72 lines)
- `src/playsim/fragglescript/t_parse.cpp` **escape_sequence** 120–130 (11 lines)
- `src/playsim/fragglescript/t_parse.cpp` **add_char** 138–144 (7 lines)
- `src/playsim/fragglescript/t_parse.cpp` **FParser::GetTokens** 168–274 (107 lines)
- `src/playsim/fragglescript/t_parse.cpp` **parse separators** 31–39 (9 lines)
- `src/playsim/fragglescript/t_parse.cpp` **parse separators** 112–119 (8 lines)
- `src/playsim/fragglescript/t_parse.cpp` **parse separators** 131–137 (7 lines)
- `src/playsim/fragglescript/t_parse.cpp` **parse separators** 145–167 (23 lines)

**Credited native LOC:** 244 (tally `nativeLoc` 244).

## Portfolio delta

**Gross CLI band:** **244** lines in the UNCredited-range probe; tally credits **244** whole-function native LOC only.

| | Native LOC |
|---|---:|
| **Gross** (CLI UNCredited band) | **244** |
| **Credited** (whole functions, this row) | **244** |
| **Overlap trim** | **0** |
| **Net headline Δ** | **+244** (see `convertedBehaviorNative` in tally at land commit) |

## Managed

`VmFScriptParserParse864.cs`
Managed LOC (tally): 227.
Conversion commit: `f59be8bc61a503981a2991e04754b0708fa0c8d2`.

## Padding review (2026-10-21)

**Behavior ported**

| Native | Ported |
|--------|--------|
| `NextToken` | yes |
| `escape_sequence` | yes |
| `add_char` | yes |
| `GetTokens` | yes |

| Managed (`VmFScriptParserParse864.cs`) | Native (`t_parse.cpp`) |
|----------------------------------------|----------------------|
| `NextToken` | **40–111** |
| `EscapeSequence` | **120–130** |
| `AppendChar` | **138–144** |
| `GetTokens` | **168–274** |
| (separators) | **31–39**, **112–119**, **131–137**, **145–167** |

### Gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **244** |
| Net | **+244** vs post-**863** |

**Post-review ratio:** 227/244 managed/native ≈ 93% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserParseTests.GetTokens_ParsesIntegerExpression`
- `VmFScriptParserParseTests.GetTokens_ParsesStringLiteral`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 864 --to 864` (0 hard / 0 soft).

## Seams

`GetTokens` stops on `{`/`}` for **859** `spec_*`; string color escapes use `TEXTCOLOR_ESCAPE` like native.

## Credit rationale

First `t_parse` band: `NextToken`, string/number/name accumulation, and `GetTokens` before **865** runner loop.
