# Chunk 1179 — FraggleScript operator table and handlers through OPmultiply from t_oper.cpp (1–356).

## Scope

FraggleScript operator table and handlers through OPmultiply from t_oper.cpp (1–356).

## Native ranges

- `src/playsim/fragglescript/t_oper.cpp` **OPmultiply** 1–356 (356 lines)

**Credited native LOC:** 356 (tally `nativeLoc` 356).

## Managed

`VmTOperLogic1179Slice.cs`
Managed LOC (tally): 475.
Conversion commit: `db9f9bfd9d23df669d509f0a3c4f99cae5268273`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Operator table and count (**39–65**) | yes |
| OPequals, OPor, OPand, OPcmp, OPnotcmp (**75–195**) | yes |
| OPlessthan, OPgreaterthan, OPnot (**203–250**) | yes |
| OPplus, OPminus, OPmultiply (**258–349**) | yes |
| Band hull (**1–356**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **357–626** | divide, remainder, bitwise, increment, and structure handlers are already credited or stay past this band |

The table lists 21 operators. Assignment is the only backward operator. Handlers whose bodies start at divide are recorded on the row and are not run here.

Assignment looks the name up in the global script. A hit evaluates the right side into the result and stores that value. A miss prints `unknown variable '%s'` and leaves the result alone.

Or and and return an int. A true left side of or, and a false left side of and, skip the right side. Compare checks strings, then mobj references, then fixed-point integers, then ints. Not-equal runs compare and flips a zero result to one. Less-than and greater-than use the fixed integer when either side is fixed, so a fixed value of −1 is less than int zero. Logical not evaluates only the right span.

Plus concatenates two strings, formats a fixed right side with four decimal places, and formats any other right side with its int. Two fixed sides add the 16.16 bits. Minus of an operator in the left slot evaluates only the right side and subtracts it from a zero int, so a fixed right side stays fixed. Multiply of a fixed side converts both sides to doubles, multiplies, and truncates back to fixed. An int multiply stays in ints.

| Managed (`VmTOperLogic1179Slice.cs`) | Native (`t_oper.cpp`) |
|---------------------------------------|------------------------|
| Table and handlers through multiply | **1–356** |
| Band hull (exclusive slice) | **1–356** |

### Chunk **1179** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **356** |
| **Net headline** | **+356** vs post-**1178** |


**Post-review ratio:** 475/356 managed/native ≈ 133% (src file LOC at HEAD; see tally).

## Tests

- `VmTOperLogic1179SliceTests.Table_ListsTheOperatorsAndStopsBeforeDivide`
- `VmTOperLogic1179SliceTests.Logic_ShortCircuitsComparesAndAssigns`
- `VmTOperLogic1179SliceTests.Arithmetic_AddsFixedBitsAndTruncatesTheProduct`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1179 --to 1179` (0 hard / 0 soft).

## Seams

FindVariable, SetValue, and EvaluateExpression are host callbacks. intvalue, fixedvalue, and floatvalue follow t_variable.cpp. OPdivide at 357 stays outside this slice.

## Credit rationale

UNCredited t_oper.cpp 1–356 (+356). OPmultiply closes at 349 and the following banner ends at 356. A full tally line scan shows no overlap. OPdivide starts at 357.
