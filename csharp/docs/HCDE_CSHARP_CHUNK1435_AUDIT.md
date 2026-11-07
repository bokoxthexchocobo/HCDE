# Chunk 1435 — CheckOpen through FScriptPosition::Message in sc_man.cpp (1126-1342).

## Scope

CheckOpen through FScriptPosition::Message in sc_man.cpp (1126-1342).

## Native ranges

- `src/common/engine/sc_man.cpp` **CheckOpen** 1126–1342 (217 lines)

**Credited native LOC:** 217 (tally `nativeLoc` 217).

## Managed

`VmScMan1435Slice.cs`
Managed LOC (tally): 318.
Conversion commit: `eabc3067db5e9aa906d02f4041360b649cfbbcf2`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Symbols and braces (**1126–1233**) | yes |
| Script position messages (**1241–1342**) | yes |
| Band hull (**1126–1342**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Comment banners | lines 1119-1125 and 1343-1348 stay uncredited |
| GetString and Compare | callees |
| ParseHex | starts at line 1350 |

A closed script errors before a string is read. Integer, unsigned, and float symbols replace a same-name entry without regard to case. An unsigned value keeps its bits in the signed field. A float symbol stores the truncated integer and the full value. Skipping a block counts nested braces and stops on the brace that drops below zero. A missing string is a fatal script error. Starting braces saves the position after the opening brace, finds the matching close, then restores that saved position and returns the end index. A missing opening brace is an error and the token is put back. The end test is true once the cursor is at or past that end. A script position copies the file name and the message line, including a put-back line. Debug severities below the developer level are ignored. An optional error is a warning unless strict errors are on. An error with the error-out switch becomes fatal and does not increment the error count. Warnings and debug errors that are printed increment the warning count. Messages are green. Log lines have no color and use the log level. An unknown severity is not printed.

| Managed (`VmScMan1435Slice.cs`) | Native (`sc_man.cpp`) |
|---------------------------------|----------------------|
| Braces and script position messages | **1126–1342** |
| Band hull (exclusive slice) | **1126–1342** |

### Chunk **1435** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **217** |
| **Net headline** | **+217** vs the prior chunk |


**Post-review ratio:** 318/217 managed/native ≈ 147% (src file LOC at HEAD; see tally).

## Tests

- `VmScMan1435SliceTests.SymbolsBracesAndOpen`
- `VmScMan1435SliceTests.ScriptPositionMessages`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1435 --to 1435` (0 hard / 0 soft).

## Seams

GetString and Compare are callees. ParseHex starts after this band.

## Credit rationale

UNCredited sc_man.cpp 1126-1342 (+217). This is symbol insertion, brace matching, and script position messages. The band is shorter than 400 lines and is not sliced.
