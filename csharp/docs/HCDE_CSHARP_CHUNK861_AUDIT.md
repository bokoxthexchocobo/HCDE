# Chunk 861 — FraggleScript operator table and assign/short-circuit/compare-equality from `t_oper.cpp` (39–189).

## Scope

FraggleScript operator table and assign/short-circuit/compare-equality from `t_oper.cpp` (39–189).

## Native ranges

- `src/playsim/fragglescript/t_oper.cpp` **FParser::operators[]** 39–65 (27 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPequals** 75–90 (16 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPor** 99–117 (19 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPand** 126–143 (18 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPcmp** 151–182 (32 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 66–74 (9 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 91–98 (8 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 118–125 (8 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 144–150 (7 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 183–189 (7 lines)

**Credited native LOC:** 151 (tally `nativeLoc` 151).

## Portfolio delta

**Gross CLI band:** **151** lines in the UNCredited-range probe; tally credits **151** whole-function native LOC only.

| | Native LOC |
|---|---:|
| **Gross** (CLI UNCredited band) | **151** |
| **Credited** (whole functions, this row) | **151** |
| **Overlap trim** | **0** |
| **Net headline Δ** | **+151** (see `convertedBehaviorNative` in tally at land commit) |

## Managed

`VmFScriptParserOper861.cs`
Managed LOC (tally): 98.
Conversion commit: `68051a085d4aa8ffd88b20d98539839dde8f2891`.

## Padding review (2026-10-21)

**Band seam **189|190****: after `OPcmp` closing brace and separator; **`OPnotcmp` begins at 190** (whole function).

**Native → managed (`VmFScriptParserOper861.cs`)**

| Native (`t_oper.cpp`) | Managed |
|-----------------------|---------|
| `operators[]` **39–65** | `Operators` |
| `OPequals` **75–90** | `OpEquals` |
| `OPor` **99–117** | `OpOr` |
| `OPand` **126–143** | `OpAnd` |
| `OPcmp` **151–182** | `OpCmp` |
| (separators) **66–74**, **91–98**, **118–125**, **144–150**, **183–189** | layout |

### Gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **151** |
| Net | **+151** vs post-**860** |

**Post-review ratio:** 98/151 managed/native ≈ 65% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserExprTests.OpEquals_AssignsVariable`
- `VmFScriptParserExprTests.OpOr_ShortCircuitsSecondOperand`
- `VmFScriptParserExprTests.OpCmp_Equality`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 861 --to 861` (0 hard / 0 soft).

## Seams

`Operators[]` + `OpEquals`/`OpOr`/`OpAnd`/`OpCmp` in `VmFScriptParserOper861.cs`; eval loop in **859** `VmFScriptParserExprEval.cs`.

## Credit rationale

Native `t_oper` credit for table and assign/`||`/`&&`/`==` through `OPcmp`; band ends before `OPnotcmp` at line **190**.
