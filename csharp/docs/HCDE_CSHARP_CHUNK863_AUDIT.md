# Chunk 863 — FraggleScript `/`/`%`, bitwise, `++`/`--`, `<=`/`>=`, and `.` dispatch from `t_oper.cpp` (350–626).

## Scope

FraggleScript `/`/`%`, bitwise, `++`/`--`, `<=`/`>=`, and `.` dispatch from `t_oper.cpp` (350–626).

## Native ranges

- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPdivide** 357–387 (31 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPremainder** 395–409 (15 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPor_bin** 419–427 (9 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPand_bin** 436–444 (9 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPnot_bin** 452–458 (7 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPincrement** 467–522 (56 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPdecrement** 530–586 (57 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPlessthanorequal** 594–606 (13 lines)
- `src/playsim/fragglescript/t_oper.cpp` **FParser::OPgreaterthanorequal** 614–626 (13 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 350–356 (7 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 388–394 (7 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 410–418 (9 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 428–435 (8 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 445–451 (7 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 459–466 (8 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 523–529 (7 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 587–593 (7 lines)
- `src/playsim/fragglescript/t_oper.cpp` **oper separators** 607–613 (7 lines)

**Credited native LOC:** 277 (tally `nativeLoc` 277).

## Portfolio delta

**Gross CLI band:** **277** lines in the UNCredited-range probe; tally credits **277** whole-function native LOC only.

| | Native LOC |
|---|---:|
| **Gross** (CLI UNCredited band) | **277** |
| **Credited** (whole functions, this row) | **277** |
| **Overlap trim** | **0** |
| **Net headline Δ** | **+277** (see `convertedBehaviorNative` in tally at land commit) |

## Managed

`VmFScriptParserOper863.cs`
Managed LOC (tally): 162.
Conversion commit: `68051a085d4aa8ffd88b20d98539839dde8f2891`.

## Padding review (2026-10-21)

**No `t_oper` chunk 864** — inc/dec and relational tail live in this row.

**Native → managed (`VmFScriptParserOper863.cs`)**

| Native | Managed |
|--------|---------|
| `OPdivide` **357–387** | `OpDivide` |
| `OPremainder` **395–409** | `OpRemainder` |
| `OPor_bin` **419–427** | `OpOrBin` |
| `OPand_bin` **436–444** | `OpAndBin` |
| `OPnot_bin` **452–458** | `OpNotBin` |
| `OPincrement` **467–522** | `OpIncrement` / `OpIncDec` |
| `OPdecrement` **530–586** | `OpDecrement` / `OpIncDec` |
| `OPlessthanorequal` **594–606** | `OpLessThanOrEqual` |
| `OPgreaterthanorequal` **614–626** | `OpGreaterThanOrEqual` |
| `.` table entry **62** | `OpStructureDispatch` |
| (separators) **350–356**, **388–394**, **410–418**, **428–435**, **445–451**, **459–466**, **523–529**, **587–593**, **607–613** | layout |

### Gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **277** |
| Net | **+277** vs post-**862** |

**Post-review ratio:** 162/277 managed/native ≈ 58% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserExprTests.OpPlus_Integers`
- `VmFScriptParserExprTests.OpCmp_Equality`
- `VmFScriptParserExprTests.OpEquals_AssignsVariable`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 863 --to 863` (0 hard / 0 soft).

## Seams

`VmFScriptParserOper863.cs` only; `.` handler body remains **860** `OpStructure`; dispatch via `OpStructureDispatch`.

## Credit rationale

Single `t_oper` row through file end (**626**); merged former **864** oper file. Session target **341–626** (286) adjusted to **350–626** (277) so **`OPmultiply` (333–349)** stays wholly in **862** (no function breach).
