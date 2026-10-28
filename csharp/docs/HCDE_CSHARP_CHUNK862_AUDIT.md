# Chunk 862 — FraggleScript compare, unary `!`, `+`/`-`/`*` from `t_oper.cpp` (190–349). Retired (2026-10-23): out-of-body credit removed; honest **118** < **150**.

## Scope

FraggleScript compare, unary `!`, `+`/`-`/`*` from `t_oper.cpp` (190–349). Retired (2026-10-23): out-of-body credit removed; honest **118** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `b290a11407e174c22e762cd3167397a04092eae5`.

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

### Out-of-body uncredit (862)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **160** |
| **Out-of-body removed** | **42** |
| **Honest** | **118** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 862 --to 862` (0 hard / 0 soft).

## Seams

`VmFScriptParserOper862.cs`; eval helpers on **859** `VmFScriptParserExprEval.cs`.

## Credit rationale

Credits `OPnotcmp` through `OPmultiply` inclusive; band ends at **349** (end of `OPmultiply`).
