# Chunk 862 — FraggleScript compare, unary `!`, `+`/`-`/`*` from `t_oper.cpp` (190–349).

## Scope

FraggleScript compare, unary `!`, `+`/`-`/`*` from `t_oper.cpp` (190–349).

## Native ranges

- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPnotcmp** 190–195 (6 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPlessthan** 203–215 (13 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPgreaterthan** 224–236 (13 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPnot** 244–252 (9 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPplus** 258–291 (34 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPminus** 299–325 (27 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPmultiply** 333–349 (17 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 196–202 (7 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 216–223 (8 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 237–243 (7 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 253–257 (5 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 292–298 (7 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 326–332 (7 lines)

**Credited native LOC:** 160 (tally `nativeLoc` 160).

## Portfolio delta

**Gross CLI band:** **160** lines in the UNCredited-range probe; tally credits **160** whole-function native LOC only.

| | Native LOC |
|---|---:|
| **Gross** (CLI UNCredited band) | **160** |
| **Credited** (whole functions, this row) | **160** |
| **Overlap trim** | **0** |
| **Net headline Δ** | **+160** (see `convertedBehaviorNative` in tally at land commit) |

## Managed

`VmFScriptParserOper862.cs`
Managed LOC (tally): 99.
Conversion commit: `68051a085d4aa8ffd88b20d98539839dde8f2891`.

## Padding review (2026-10-21)

**Band seam **349|350****: after `OPmultiply` (**333–349**); **`OPdivide` begins at 357** (separator **350–355**). Rejects prior **340|341** floor slice that bisected `OPmultiply`.

**Native → managed (`VmFScriptParserOper862.cs`)**

| Native | Managed |
|--------|---------|
| `OPnotcmp` **190–195** | `OpNotCmp` |
| `OPlessthan` **203–215** | `OpLessThan` |
| `OPgreaterthan` **224–236** | `OpGreaterThan` |
| `OPnot` **244–252** | `OpNot` |
| `OPplus` **258–291** | `OpPlus` |
| `OPminus` **299–325** | `OpMinus` |
| `OPmultiply` **333–349** | `OpMultiply` |
| (separators) **196–202**, **216–223**, **237–243**, **253–257**, **292–298**, **326–332** | layout |

### Gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **160** |
| Net | **+160** vs post-**861** |

**Post-review ratio:** 99/160 managed/native ≈ 62% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserExprTests.OpCmp_Equality`
- `VmFScriptParserExprTests.OpPlus_Integers`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 862 --to 862` (0 hard / 0 soft).

## Seams

`VmFScriptParserOper862.cs`; eval helpers on **859** `VmFScriptParserExprEval.cs`.

## Credit rationale

Credits `OPnotcmp` through `OPmultiply` inclusive; band ends at **349** (end of `OPmultiply`).
