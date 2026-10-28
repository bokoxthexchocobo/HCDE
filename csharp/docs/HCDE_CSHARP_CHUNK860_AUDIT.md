# Chunk 860 — FraggleScript variable/script specials from `fragglescript/t_spec.cpp` (231–605).

## Scope

FraggleScript variable/script specials from `fragglescript/t_spec.cpp` (231–605).

## Native ranges

- `src/playsim/fragglescript/t_spec.cpp` **FParser::CreateVariable** 231–265 (35 lines)
- `src/playsim/fragglescript/t_spec.cpp` **FParser::ParseVarLine** 274–287 (14 lines)
- `src/playsim/fragglescript/t_spec.cpp` **FParser::spec_variable** 295–350 (56 lines)
- `src/playsim/fragglescript/t_spec.cpp` **FParser::spec_script** 367–429 (63 lines)
- `src/playsim/fragglescript/t_spec.cpp` **FParser::EvaluateFunction** 443–519 (77 lines)
- `src/playsim/fragglescript/t_spec.cpp` **FParser::OPstructure** 534–605 (72 lines)
- `src/playsim/fragglescript/t_spec.cpp` **spec separators** 430–442 (13 lines)
- `src/playsim/fragglescript/t_spec.cpp` **spec separators** 520–533 (14 lines)

**Credited native LOC:** 344 (tally `nativeLoc` 344).

## Managed

`VmFScriptParserSpecRest.cs`
Managed LOC (tally): 357.
Conversion commit: `4fa511de8d9c332ce9fbaf7f4e4236dd5a56b458`.

## Padding review (2026-10-21)

**Behavior ported**

| Native | Ported |
|--------|--------|
| `CreateVariable` / `ParseVarLine` / `spec_variable` | yes |
| `spec_script` | yes |
| `EvaluateFunction` | yes |
| `OPstructure` (`.`) | yes |

| Managed | Native (`t_spec.cpp`) |
|---------|----------------------|
| `ParseVariableDeclarationHeader` / `SpecVariable` | **295–350** |
| `CreateVariable` / `ParseVarLine` | **231–287** |
| `CopyChildScriptSectionBody` / `SpecScript` | **367–429** |
| `CollectCommaSeparatedArguments` / `ResolveGlobalCallable` / `InvokeCallable` / `EvaluateFunction` | **443–519** |
| `OpStructure` | **534–605** |
| `RunEvaluateFunctionPipeline` / `RunOpStructurePipeline` / `AttachChildScriptFromSection` | orchestration **367–429**, **443–519**, **534–605** |
| (separators) | **222–230**, **266–273**, **288–294**, **351–366**, **430–442**, **520–533** |

### Gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **384** |
| Net | **+384** vs post-**859** |

### Out-of-body uncredit (860)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **384** |
| **Out-of-body removed** | **40** |
| **Honest** | **344** |


**Post-review ratio:** 357/344 managed/native ≈ 104% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSpecRestTests.SpecVariable_IntDecl_CreatesVariable`
- `VmFScriptParserSpecRestTests.SpecVariable_ConstDecl_CreatesVariable`
- `VmFScriptParserSpecRestTests.SpecScript_StoresChildAndAdvancesRover`
- `VmFScriptParserSpecRestTests.SpecScript_CopiesSectionBodyIntoChildData`
- `VmFScriptParserSpecRestTests.EvaluateFunction_InvokesHandler`
- `VmFScriptParserSpecRestTests.EvaluateFunction_PassesArgumentValues`
- `VmFScriptParserSpecRestTests.EvaluateFunction_LineSpec_InvokesRunLineSpecial`
- `VmFScriptParserSpecRestTests.OpStructure_InvokesDotCall`
- `VmFScriptParserSpecRestTests.OpStructure_DotCallWithExtraArgument`
- `VmFScriptParserSpecRestTests.ValidateFunctionCallSyntax_RejectsMissingParen`
- `VmFScriptParserSpecRestTests.CreateVariable_BlocksMobjInChildScript`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 860 --to 860` (0 hard / 0 soft).

## Seams

`spec_script` uses **857** `Preprocess`; **859** `VmFScriptParserExpr` calls `EvaluateFunction` / `OpStructure` here via `ExprFrame.Tag` (`SpecRestState`), not hooks.

## Credit rationale

Second `t_spec` band; completes `EvaluateFunction` and native `.` (`OPstructure`).
