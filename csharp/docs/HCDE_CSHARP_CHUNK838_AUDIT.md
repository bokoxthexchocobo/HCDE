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
Managed LOC (tally): 113.
Conversion commit: `70d2ae548dd889309cb4421554631658b11cdb78`.

## Padding review (2026-10-21)

| Managed | Native (`t_func.cpp`) |
|---------|-------------------------|
| `FsTerminatorException` | **532–535** (`SF_Return` / `CFsTerminator`) |
| `SfGoto` | **509–524** |
| `SfReturn` | **532–535** |
| `SfInclude` | **543–559** |
| `ParseInclude` | **543–559** (`Script->ParseInclude`) |
| `SfInput` | **567–570** |
| `PrintBold` | **567–570** |
| `SfBeep` | **578–581** |
| `PlaySound` | **578–581** |
| `SfClock` | **589–593** |
| `SfExitLevel` | **603–606** |
| `ExitLevel` | **603–606** |
| `SfTip` | **614–621** |
| `TriggerLocalView` | **614–621** |
| `MidPrint` | **614–621**, **633–642**, **651–661** |
| `SfTimedTip` | **633–642** |
| `SfPlayerTip` | **651–661** |
| `PlayerLocalView` | **651–661**, **684–694** |
| `SfMessage` | **669–676** |
| `SfPlayerMsg` | **684–694** |
| `SfPlayerInGame` | **702–714** |
| (native separators) | **525–531**, **536–542**, **560–566**, **571–577**, **582–588**, **594–602**, **607–613**, **622–632**, **643–650**, **662–668**, **677–683**, **695–701**, **715–721** |

**Post-review ratio:** 113/213 managed/native ≈ 53% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfTests.SfGoto_Label_SetsRover`
- `VmFScriptParserSfTests.SfReturn_ThrowsTerminator`
- `VmFScriptParserSfTests.SfInclude_String_TruncatesToEightChars`
- `VmFScriptParserSfTests.SfGoto_NonLabel_ScriptError`
- `VmFScriptParserSfTests.SfPlayerInGame_ValidPlayer_ReturnsOne`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 838 --to 838` (0 hard / 0 soft).

## Seams

`ParseInclude`, console print, sound, and `ExitLevel` are host callbacks on `ParserContext` (partial in `VmFScriptParserSf.cs` only); `SF_Return` → `FsTerminatorException`.

## Credit rationale

Whole-function band after parser core (**837**) through `SF_PlayerInGame` before player-name/spawn API (**839**).
