# Chunk 859 — FraggleScript control-flow specials from `fragglescript/t_spec.cpp` (31–221).

## Scope

FraggleScript control-flow specials from `fragglescript/t_spec.cpp` (31–221).

## Native ranges

- `src/playsim/fragglescript/t_spec.cpp` **FParser::spec_brace** 31–46 (16 lines)
- `src/playsim/fragglescript/t_spec.cpp` **FParser::spec_if** 54–84 (31 lines)
- `src/playsim/fragglescript/t_spec.cpp` **FParser::spec_elseif** 92–127 (36 lines)
- `src/playsim/fragglescript/t_spec.cpp` **FParser::spec_else** 135–139 (5 lines)
- `src/playsim/fragglescript/t_spec.cpp` **FParser::spec_while** 147–168 (22 lines)
- `src/playsim/fragglescript/t_spec.cpp` **FParser::spec_for** 176–221 (46 lines)
- `src/playsim/fragglescript/t_spec.cpp` **spec separators** 1–30 (30 lines)
- `src/playsim/fragglescript/t_spec.cpp` **spec separators** 47–53 (7 lines)
- `src/playsim/fragglescript/t_spec.cpp` **spec separators** 85–91 (7 lines)
- `src/playsim/fragglescript/t_spec.cpp` **spec separators** 128–134 (7 lines)
- `src/playsim/fragglescript/t_spec.cpp` **spec separators** 140–146 (7 lines)
- `src/playsim/fragglescript/t_spec.cpp` **spec separators** 169–175 (7 lines)

**Credited native LOC:** 221 (tally `nativeLoc` 221).

## Managed

`VmFScriptParserExprEval.cs`, `VmFScriptParserSpecControl.cs`
Managed LOC (tally): 347.
Conversion commit: `7efde43fb4224962d736a9c8fe4699e99c4e83ba`.

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

**Post-review ratio:** 347/221 managed/native ≈ 157% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSpecControlTests.SpecIf_FalseWithBraces_SkipsToSectionEnd`
- `VmFScriptParserExprTests.OpPlus_Integers`
- `VmFScriptParserExprTests.OpCmp_Equality`
- `VmFScriptParserExprTests.OpOr_ShortCircuitsSecondOperand`
- `VmFScriptParserExprTests.OpEquals_AssignsVariable`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 859 --to 859` (0 hard / 0 soft).

## Seams

`EvaluateExpression` in `VmFScriptParserExprEval.cs` dispatches operators credited on **861–864** (`t_oper.cpp`); `.` calls **860** `OpStructure` via `ExprFrame.Tag`.

## Credit rationale

First `t_spec` band after **858**; expression parse/eval loop only (no duplicate operator bodies).
