# Chunk 861 — FraggleScript operator table and assign/short-circuit/compare-equality from `t_oper.cpp` (39–189). Retired (2026-10-23): out-of-body credit removed; honest **85** < **150**.

## Scope

FraggleScript operator table and assign/short-circuit/compare-equality from `t_oper.cpp` (39–189). Retired (2026-10-23): out-of-body credit removed; honest **85** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `99c0feec1771cbed737314216c514a11ddafd385`.

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

### Out-of-body uncredit (861)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **151** |
| **Out-of-body removed** | **66** |
| **Honest** | **85** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 861 --to 861` (0 hard / 0 soft).

## Seams

`Operators[]` + `OpEquals`/`OpOr`/`OpAnd`/`OpCmp` in `VmFScriptParserOper861.cs`; eval loop in **859** `VmFScriptParserExprEval.cs`.

## Credit rationale

Native `t_oper` credit for table and assign/`||`/`&&`/`==` through `OPcmp`; band ends before `OPnotcmp` at line **190**.
