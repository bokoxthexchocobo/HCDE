# Chunk 1180 — FraggleScript brace, if, elseif, else, while, and for from t_spec.cpp (1–230).

## Scope

FraggleScript brace, if, elseif, else, while, and for from t_spec.cpp (1–230).

## Native ranges

- `src/playsim/fragglescript/t_spec.cpp` **spec_for** 1–230 (230 lines)

**Credited native LOC:** 230 (tally `nativeLoc` 230).

## Managed

`VmTSpecControl1180Slice.cs`
Managed LOC (tally): 237.
Conversion commit: `ec42bac904531473a8396bb65216f0b77f56ccc5`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| spec_brace (**31–46**) | yes |
| spec_if (**54–84**) | yes |
| spec_elseif (**92–127**) | yes |
| spec_else (**135–139**) | yes |
| spec_while (**147–168**) | yes |
| spec_for (**176–221**) | yes |
| Band hull (**1–230**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **231–605** | variable creation and the later specials stay outside this band |

A closing brace on an if or else section does nothing. A closing brace on a loop section jumps to the loop start. An opening brace returns immediately. Other section types are left in place.

if and elseif report the parse error and return false when the closing parenthesis is missing. A true previous branch makes elseif skip to the section end and return true without evaluating its condition. Otherwise the condition is tokens 2 through the token before the parenthesis. A braced form whose parenthesis is the last token skips to the section end when the condition is zero. Any other true condition with tokens after the parenthesis evaluates that tail. The result is nonzero-as-true, including a string of zero.

else skips to the section end only when the previous branch matched. while without a section, or without a parenthesis, reports its own error. A zero condition skips to the section end.

for without a section reports the delimiter error. A missing comma stops before the second search when the first comma is absent, and reports the argument error. The first visit evaluates only the initializer. A return to the same section evaluates the step, then the condition, and leaves the loop when that condition is zero.

| Managed (`VmTSpecControl1180Slice.cs`) | Native (`t_spec.cpp`) |
|----------------------------------------|------------------------|
| Brace and control statements | **1–230** |
| Band hull (exclusive slice) | **1–230** |

### Chunk **1180** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **230** |
| **Net headline** | **+230** vs post-**1179** |


**Post-review ratio:** 237/230 managed/native ≈ 103% (src file LOC at HEAD; see tally).

## Tests

- `VmTSpecControl1180SliceTests.BraceAndIf_SkipClosedLoopsAndBareStatements`
- `VmTSpecControl1180SliceTests.ElseIfAndElse_SkipWhenThePreviousBranchMatched`
- `VmTSpecControl1180SliceTests.Loops_RunTheInitOnceAndTheStepOnTheWayBack`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1180 --to 1180` (0 hard / 0 soft).

## Seams

FindOperator, EvaluateExpression, SectionEnd, and SectionLoop are host callbacks. intvalue is the chunk 1179 conversion. CreateVariable at 231 stays outside this slice.

## Credit rationale

UNCredited t_spec.cpp 1–230 (+230). spec_for closes at 221. The CreateVariable banner is 223–230 and the function starts at 231. A full tally line scan shows no overlap.
