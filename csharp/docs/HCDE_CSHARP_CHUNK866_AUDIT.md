# Chunk 866 — FraggleScript expression evaluation and parse errors from `fragglescript/t_parse.cpp` (453–731); `VmFScriptParserExprEval.cs` removed.

## Scope

FraggleScript expression evaluation and parse errors from `fragglescript/t_parse.cpp` (453–731); `VmFScriptParserExprEval.cs` removed.

## Native ranges

- `src/playsim/fragglescript/t_parse.cpp` **FParser::FindOperator** 461–482 (22 lines)
- `src/playsim/fragglescript/t_parse.cpp` **FParser::FindOperatorBackwards** 490–515 (26 lines)
- `src/playsim/fragglescript/t_parse.cpp` **FParser::SimpleEvaluate** 530–564 (35 lines)
- `src/playsim/fragglescript/t_parse.cpp` **FParser::PointlessBrackets** 577–607 (31 lines)
- `src/playsim/fragglescript/t_parse.cpp` **FParser::EvaluateExpression** 625–676 (52 lines)
- `src/playsim/fragglescript/t_parse.cpp` **FS_Error** 685–694 (10 lines)
- `src/playsim/fragglescript/t_parse.cpp` **FParser::ErrorMessage** 697–711 (15 lines)
- `src/playsim/fragglescript/t_parse.cpp` **script_error** 719–726 (8 lines)

**Credited native LOC:** 199 (tally `nativeLoc` 199).

## Portfolio delta

**Gross CLI band:** **279** lines in the UNCredited-range probe; tally credits **199** whole-function native LOC only.
**80** uncovered lines in that band are inter-function gaps, comments, or non-function lines (not credited).

| | Native LOC |
|---|---:|
| **Gross** (CLI UNCredited band) | **279** |
| **Credited** (whole functions, this row) | **199** |
| **Overlap trim** | **0** |
| **Net headline Δ** | **+199** (see `convertedBehaviorNative` in tally at land commit) |

## Managed

`VmFScriptParserParse866.cs`
Managed LOC (tally): 229.
Conversion commit: `5edd525fdf4aaa7e58feadaefd0a38df3930b9c4`.

## Padding review (2026-10-21)

**Behavior ported**

| Native | Ported |
|--------|--------|
| `FindOperator` / `FindOperatorBackwards` | yes |
| `SimpleEvaluate` | yes |
| `PointlessBrackets` | yes |
| `EvaluateExpression` | yes |
| `FS_Error` / `ErrorMessage` / `script_error` | yes (`FsParseError` + `ScriptError`) |

| Managed (`VmFScriptParserParse866.cs`) | Native (`t_parse.cpp`) |
|----------------------------------------|----------------------|
| `FindOperator` | **461–482** |
| `FindOperatorBackwards` | **490–515** |
| `SimpleEvaluate` | **530–564** |
| `PointlessBrackets` | **577–607** |
| `EvaluateExpression` | **625–676** |
| `ErrorMessage` | **697–711** |
| `FS_Error` | **685–694** (via `FsParseError`) |
| `script_error` | **719–726** (via `ScriptError`) |
| (separators) | **453–460**, **483–489**, **516–529**, **565–576**, **608–624**, **677–684**, **695–696**, **712–718**, **727–731**, **452** |

### Gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **279** |
| Net | **+279** vs post-**865** |

### Out-of-body uncredit (866)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **280** |
| **Out-of-body removed** | **81** |
| **Honest** | **199** |


**Post-review ratio:** 229/199 managed/native ≈ 115% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserExprTests.OpPlus_Integers`
- `VmFScriptParserParseTests.RunScript_EvaluatesExpression`
- `VmFScriptParserParseTests.GetTokens_ParsesIntegerExpression`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 866 --to 866` (0 hard / 0 soft).

## Seams

Operator dispatch via **861–863** `Operators[]`; **860** `EvaluateFunction` for calls; **865** `RunStatement` invokes `EvaluateExpression`.

## Credit rationale

Completes `t_parse` eval loop moved off **859** `VmFScriptParserExprEval.cs`; **867** retired.
