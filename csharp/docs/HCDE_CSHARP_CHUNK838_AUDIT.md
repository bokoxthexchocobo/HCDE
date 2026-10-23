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
Managed LOC (tally): 134.
Conversion commit: `c2c27051bd6597c2896532cd0d2d728318c8ada3`.

## Padding review (2026-10-21)

**Seam removal delta (vs 113-line delegate port):** every added physical line maps to a credited **`t_func.cpp` 509–721** line (not to `t_prepro.cpp`, `s_sound.cpp`, or `t_script.cpp`).

| Added / changed managed (in-band) | Native (`t_func.cpp` 509–721) |
|-----------------------------------|-------------------------------|
| `ParserContext` capture fields (`FsPrintCapture`, `FsMidPrintCapture`, `FsPendingIncludes`, `FsExitLevelPending`, `FsConMidtime`, `LocalView`, `Sfx`) | one field per **call-site** callee in band: **557** `ParseInclude`, **569** `Printf`, **580** `S_Sound`, **605** `ExitLevel`, **617–619** `CheckLocalView`+`C_MidPrint`, **637–639** `con_midtime`+`C_MidPrint`, **656–658** / **689–691** player `CheckLocalView`+`C_MidPrint` / `Printf` |
| `SfInclude` lump resolve + `FsPendingIncludes.Add` | **543–559** (includes **557** `Script->ParseInclude` — `DFsScript::ParseInclude` body **401–424** stays uncredited on **835+** / future prepro chunk, not counted here) |
| `SfInput` → `FsPrintCapture` | **567–570** |
| `SfBeep` → `FindSound` + `VmSoundEngineStartSound.StartSound` | **578–581** (engine body elsewhere) |
| `SfExitLevel` → `FsExitLevelPending` | **603–606** |
| `SfTip` / `SfTimedTip` / `SfPlayerTip` inlined `CheckLocalView` + capture | **614–621**, **633–642**, **651–661** |
| `SfMessage` / `SfPlayerMsg` inlined `CheckLocalView` + `FsPrintCapture` | **669–676**, **684–694** |
| (unchanged band) | `SfGoto`, `SfReturn`, `SfClock`, `SfPlayerInGame`, separators **525–531** … **715–721** |

**Not in 838 managedLoc:** `VmFScriptParserSfCallees.cs` removed; no edits to **`VmFScriptScript.cs`** (**835** stays `t_script.cpp` **97–317** only).

**Post-review ratio:** 134/213 managed/native ≈ 63% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfTests.SfGoto_Label_SetsRover`
- `VmFScriptParserSfTests.SfReturn_ThrowsTerminator`
- `VmFScriptParserSfTests.SfInclude_String_TruncatesToEightChars`
- `VmFScriptParserSfTests.SfGoto_NonLabel_ScriptError`
- `VmFScriptParserSfTests.SfPlayerInGame_ValidPlayer_ReturnsOne`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 838 --to 838` (0 hard / 0 soft).

## Seams

No `ParserContext` Action/Func forwarders. `SF_Return` → `FsTerminatorException`. Uncredited callee bodies (`Printf`, `S_Sound`, `C_MidPrint`, `Level->ExitLevel`, `DFsScript::ParseInclude` tail) are invoked only via call sites in **509–721**; capture fields record those calls for tests/integration — not separate callee ports.

## Credit rationale

Whole-function band after parser core (**837**) through `SF_PlayerInGame` before player-name/spawn API (**839**).
