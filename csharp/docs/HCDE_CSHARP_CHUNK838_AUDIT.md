# Chunk 838 — Retired: honest whole-function credit for `t_func.cpp` 509–721 is 105 native lines (<150). Band uncredited until `ParseInclude`, `Printf(PRINT_BOLD)`, `ExitLevel`, and `C_MidPrint` have existing managed ports. No duplicate off-tally `VmFScriptParserSf.cs` (re-port when callees exist).

## Scope

Retired: honest whole-function credit for `t_func.cpp` 509–721 is 105 native lines (<150). Band uncredited until `ParseInclude`, `Printf(PRINT_BOLD)`, `ExitLevel`, and `C_MidPrint` have existing managed ports. No duplicate off-tally `VmFScriptParserSf.cs` (re-port when callees exist).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: ``.

## Padding review (2026-10-21)

**Behavior ported (honest credit only — row retired at 105 native lines)**

| Native `SF_*` | Behavior ported | Notes |
|---------------|-----------------|-------|
| `SF_Goto` | yes | in-band |
| `SF_Return` | yes | `FsTerminatorException` |
| `SF_Include` | no | `ParseInclude` callee outside band |
| `SF_Input` | no | `Printf(PRINT_BOLD)` |
| `SF_Beep` | yes | `VmSoundEngineStartSound` |
| `SF_Clock` | yes | in-band |
| `SF_ExitLevel` | no | `Level->ExitLevel` |
| `SF_Tip` | no | `C_MidPrint` |
| `SF_TimedTip` | no | `C_MidPrint` + `con_midtime` |
| `SF_PlayerTip` | no | `C_MidPrint` |
| `SF_Message` | yes | **837** `PrintHigh` |
| `SF_PlayerMsg` | yes | **837** `PrintHigh` |
| `SF_PlayerInGame` | yes | in-band |

Honest credited native lines (functions + separators between credited SFs only): **105**. Threshold **150** not met — band left uncredited.

## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 838 --to 838` (0 hard / 0 soft).

## Seams

Callee bodies outside **509–721** never count toward **838**. Wired today: **837** `PrintHigh`, `VmSoundEngineStartSound`, `VmPMobjLocalViewTouch`. Gaps: `ParseInclude`, `PrintBold`, `ExitLevel`, `C_MidPrint`/`con_midtime`.

## Credit rationale

Band after parser core (**837**) through `SF_PlayerInGame` before **839**; row retired per stub-credit policy.
