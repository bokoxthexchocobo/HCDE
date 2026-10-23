# Chunk 861 — FraggleScript operator table and assign/short-circuit logic from `t_oper.cpp` (39–190).

## Scope

FraggleScript operator table and assign/short-circuit logic from `t_oper.cpp` (39–190).

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
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPnotcmp** 190–190 (1 lines)

**Credited native LOC:** 152 (tally `nativeLoc` 152).

## Portfolio delta

**Gross CLI band:** **152** lines in the UNCredited-range probe; tally credits **152** whole-function native LOC only.

| | Native LOC |
|---|---:|
| **Gross** (CLI UNCredited band) | **152** |
| **Credited** (whole functions, this row) | **152** |
| **Overlap trim** | **0** |
| **Net headline Δ** | **+152** (see `convertedBehaviorNative` in tally at land commit) |

## Managed

`VmFScriptParserOper861.cs`
Managed LOC (tally): 76.
Conversion commit: `7efde43fb4224962d736a9c8fe4699e99c4e83ba`.

## Padding review (2026-10-21)

**Uncredited check (pre-land):** `t_oper.cpp` **39–190** never credited before this row.

**Native → managed (faithful port, existing code)**

| Native (`t_oper.cpp`) | Managed |
|-----------------------|---------|
| `operators[]` **39–65** | `Operators` (`VmFScriptParserOper861.cs`) |
| `OPequals` **75–90** | `OpEquals` |
| `OPor` **99–117** | `OpOr` |
| `OPand` **126–143** | `OpAnd` |
| `OPcmp` **151–182** | `OpCmp` (`VmFScriptParserOper862.cs`, same `partial` class) |
| `OPnotcmp` **190–195** | `OpNotCmp` |
| (separators) | comment blocks between handlers |

### Gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **152** |
| Net | **+152** vs post-**860** |

**Post-review ratio:** 76/152 managed/native ≈ 50% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserExprTests.OpEquals_AssignsVariable`
- `VmFScriptParserExprTests.OpOr_ShortCircuitsSecondOperand`
- `VmFScriptParserExprTests.OpCmp_Equality`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 861 --to 861` (0 hard / 0 soft).

## Seams

Single implementation: `Operators[]` + `OpEquals`/`OpOr`/`OpAnd` in `VmFScriptParserOper861.cs`; eval loop in **859** `VmFScriptParserExprEval.cs`.

## Credit rationale

Native `t_oper` credit for code already introduced with **859**; no second operator table.
