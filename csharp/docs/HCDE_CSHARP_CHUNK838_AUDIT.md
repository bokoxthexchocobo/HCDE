# Chunk 838 — Mid `FParser::SF_*` control, include, and player messaging from `fragglescript/t_func.cpp` (509–721).

## Scope

Mid `FParser::SF_*` control, include, and player messaging from `fragglescript/t_func.cpp` (509–721).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Goto** 509–524 (16 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Return** 532–535 (4 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Include** 543–559 (17 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Input** 567–570 (4 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Beep** 578–581 (4 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Clock** 589–593 (5 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ExitLevel** 603–606 (4 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Tip** 614–621 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_TimedTip** 633–642 (10 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_PlayerTip** 651–661 (11 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Message** 669–676 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_PlayerMsg** 684–694 (11 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_PlayerInGame** 702–714 (13 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF band separators** 525–531 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF band separators** 536–542 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF band separators** 560–566 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF band separators** 571–577 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF band separators** 582–588 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF band separators** 594–602 (9 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF band separators** 607–613 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF band separators** 622–632 (11 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF band separators** 643–650 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF band separators** 662–668 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF band separators** 677–683 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF band separators** 695–701 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF band separators** 715–721 (7 lines)

**Credited native LOC:** 213 (tally `nativeLoc` 213).

## Managed

`VmFScriptParserSf.cs`
Managed LOC (tally): 135.
Conversion commit: `957579c7ab9be6f7769cc4ccbcba2761398ea38c`.

## Padding review (2026-10-21)

| Managed | Native (`t_func.cpp` 509–721) |
|---------|-------------------------|
| `FsTerminatorException` | **532–535** |
| `SfGoto` | **509–524** |
| `SfReturn` | **532–535** |
| `SfInclude` (truncate + gap at **557**) | **543–559** |
| `SfInput` (gap **569**) | **567–570** |
| `SfBeep` → `VmSoundEngineStartSound` | **578–581** |
| `SfClock` | **589–593** |
| `SfExitLevel` (gap **605**) | **603–606** |
| `SfTip` (view check + gap **619**) | **614–621** |
| `SfTimedTip` (gap **637–639**) | **633–642** |
| `SfPlayerTip` (gap **658**) | **651–661** |
| `SfMessage` / `SfPlayerMsg` → **837** `PrintHigh` | **669–676**, **684–694** |
| `SfPlayerInGame` | **702–714** |
| `ParserContext.LocalView` / `Sfx` | hosts for **617+** / **580** call sites only |
| (separators) | **525–531**, **536–542**, **560–566**, **571–577**, **582–588**, **594–602**, **607–613**, **622–632**, **643–650**, **662–668**, **677–683**, **695–701**, **715–721** |

**Post-review ratio:** 135/213 managed/native ≈ 63% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfTests.SfGoto_Label_SetsRover`
- `VmFScriptParserSfTests.SfReturn_ThrowsTerminator`
- `VmFScriptParserSfTests.SfInclude_WithScript_DoesNotScriptError`
- `VmFScriptParserSfTests.SfGoto_NonLabel_ScriptError`
- `VmFScriptParserSfTests.SfPlayerInGame_ValidPlayer_ReturnsOne`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 838 --to 838` (0 hard / 0 soft).

## Seams

No `ParserContext` Action/Func forwarders added by **838** (uses **837** `PrintHigh` only where native calls `Printf(PRINT_HIGH, …)`). `SF_Return` → `FsTerminatorException`. **Known callee gaps** (no capture/pending buffers): `Printf(PRINT_BOLD)` → `SfInput` (**569**); `DFsScript::ParseInclude` → `SfInclude` (**557** / `t_prepro.cpp` 401–425); `Level->ExitLevel` → `SfExitLevel` (**605**); `C_MidPrint` + `con_midtime` → `SfTip` / `SfTimedTip` / `SfPlayerTip` (**619**, **637–639**, **658**). Wired: `S_Sound` → `VmSoundEngineStartSound`; `CheckLocalView` → `VmPMobjLocalViewTouch`; `Printf(PRINT_HIGH)` → **837** `PrintHigh`.

## Credit rationale

Whole-function band after parser core (**837**) through `SF_PlayerInGame` before player-name/spawn API (**839**).
