# Chunk 1434 — ScanValue through ScriptMessage in sc_man.cpp (933-1118).

## Scope

ScanValue through ScriptMessage in sc_man.cpp (933-1118).

## Native ranges

- `src/common/engine/sc_man.cpp` **ScanValue** 933–1118 (186 lines)

**Credited native LOC:** 186 (tally `nativeLoc` 186).

## Managed

`VmScMan1434Slice.cs`
Managed LOC (tally): 255.
Conversion commit: `44c0f4f7b40f1bf8f31f46dc763487d7ea13236b`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Numeric values (**933–1004**) | yes |
| Token names and errors (**1014–1118**) | yes |
| Band hull (**933–1118**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Comment banner | lines 925-932 stay uncredited |
| GetToken and CheckToken | callees below this band |
| Brace helpers | start at line 1126 |

A value is an optional sign and then an integer, a float, or a named constant. A float is rejected when floats are not allowed, before the sign is applied. A constant that is not a whole number is rejected the same way, and its value is not stored. A minus sign negates the number, including a float token's stale integer fields. CheckValue restores the script position when the scan fails and leaves those number fields in place. A closed script errors before the scan. True sets the number and float to 1. False sets them to 0. Anything else is put back. Token names for bytes 33 through 255 are quoted characters. Named tokens from 256 through 412 use the token table. Identifier through float constant append the token text. Anything else is Unknown. A null message is Bad syntax. Script errors use the already-got line when a token was put back. Fatal errors omit the color codes. Nonfatal errors and script messages print the red error line and keep going.

| Managed (`VmScMan1434Slice.cs`) | Native (`sc_man.cpp`) |
|---------------------------------|----------------------|
| Values, names, and script errors | **933–1118** |
| Band hull (exclusive slice) | **933–1118** |

### Chunk **1434** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **186** |
| **Net headline** | **+186** vs the prior chunk |


**Post-review ratio:** 255/186 managed/native ≈ 137% (src file LOC at HEAD; see tally).

## Tests

- `VmScMan1434SliceTests.ValuesSignsConstantsAndBools`
- `VmScMan1434SliceTests.TokenNamesAndScriptErrors`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1434 --to 1434` (0 hard / 0 soft).

## Seams

GetToken, UnGet, and CheckToken are callees below this band. The comment banner at 925-932 stays uncredited.

## Credit rationale

UNCredited sc_man.cpp 933-1118 (+186). This is numeric token scanning, token names, and script errors. The band is shorter than 400 lines and is not sliced.
