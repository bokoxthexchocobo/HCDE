# Chunk 859 — FraggleScript control-flow specials from `fragglescript/t_spec.cpp` (31–221). Retired (2026-10-23): out-of-body credit removed; honest **149** < **150**.

## Scope

FraggleScript control-flow specials from `fragglescript/t_spec.cpp` (31–221). Retired (2026-10-23): out-of-body credit removed; honest **149** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `340c010dd15b74ec742705409608c9afa3a49bed`.

## Padding review (2026-10-21)

**Behavior ported**

| Native | Ported |
|--------|--------|
| `spec_brace` | yes |
| `spec_if` / `spec_elseif` / `spec_else` | yes |
| `spec_while` / `spec_for` | yes |

| Managed | Native (`t_spec.cpp`) |
|---------|----------------------|
| `SpecBrace` | **31–46** |
| `SpecIf` | **54–84** |
| `SpecElseif` | **92–127** |
| `SpecElse` | **135–139** |
| `SpecWhile` | **147–168** |
| `SpecFor` | **176–221** |
| (separators) | **1–30**, **47–53**, **85–91**, **128–134**, **140–146**, **169–175** |

### Gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **221** |
| Net | **+221** vs post-**858** |

### Out-of-body uncredit (859)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **221** |
| **Out-of-body removed** | **72** |
| **Honest** | **149** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 859 --to 859` (0 hard / 0 soft).

## Seams

Expression eval loop credited on **866** (`t_parse.cpp`); operator bodies on **861–863** (`t_oper.cpp`); `.` calls **860** `OpStructure` via `ExprFrame.Tag`.

## Credit rationale

First `t_spec` band after **858**; control-flow specials only (no `t_parse` eval on this row).
