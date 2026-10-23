# Chunk 862 — FraggleScript compare and `+`/`-` operators from `t_oper.cpp` (191–340).

## Scope

FraggleScript compare and `+`/`-` operators from `t_oper.cpp` (191–340).

## Native ranges

- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPnotcmp** 191–195 (5 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPlessthan** 203–215 (13 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPgreaterthan** 224–236 (13 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPnot** 244–252 (9 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPplus** 258–291 (34 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPminus** 299–325 (27 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 196–202 (7 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 216–223 (8 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 237–243 (7 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 253–257 (5 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 292–298 (7 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 326–340 (15 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Portfolio delta

**Gross CLI band:** **150** lines in the UNCredited-range probe; tally credits **150** whole-function native LOC only.

| | Native LOC |
|---|---:|
| **Gross** (CLI UNCredited band) | **150** |
| **Credited** (whole functions, this row) | **150** |
| **Overlap trim** | **0** |
| **Net headline Δ** | **+150** (see `convertedBehaviorNative` in tally at land commit) |

## Managed

`VmFScriptParserOper862.cs`
Managed LOC (tally): 107.
Conversion commit: `7efde43fb4224962d736a9c8fe4699e99c4e83ba`.

## Padding review (2026-10-21)

**Native → managed (`VmFScriptParserOper862.cs`)**

| Native | Managed |
|--------|---------|
| `OPnotcmp` **191–195** | `OpNotCmp` |
| `OPlessthan` **203–215** | `OpLessThan` |
| `OPgreaterthan` **224–236** | `OpGreaterThan` |
| `OPnot` **244–252** | `OpNot` |
| `OPplus` **258–291** | `OpPlus` |
| `OPminus` **299–325** | `OpMinus` |
| (separators) **196–202**, **216–223**, **237–243**, **253–257**, **292–298**, **326–340** | layout |

### Gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| Net | **+150** vs post-**861** |

**Post-review ratio:** 107/150 managed/native ≈ 71% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserExprTests.OpCmp_Equality`
- `VmFScriptParserExprTests.OpPlus_Integers`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 862 --to 862` (0 hard / 0 soft).

## Seams

`VmFScriptParserOper862.cs`; eval helpers on **859** `VmFScriptParserExprEval.cs`.

## Credit rationale

Credits native compare/plus/minus already in the **859** partial-class split.
