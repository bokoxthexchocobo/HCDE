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
- `src/playsim/fragglescript/t_func.cpp` **FParser mid SF band (section glue)** 525–531 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser mid SF band (section glue)** 536–542 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser mid SF band (section glue)** 560–566 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser mid SF band (section glue)** 571–577 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser mid SF band (section glue)** 582–588 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser mid SF band (section glue)** 594–602 (9 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser mid SF band (section glue)** 607–613 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser mid SF band (section glue)** 622–632 (11 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser mid SF band (section glue)** 643–650 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser mid SF band (section glue)** 662–668 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser mid SF band (section glue)** 677–683 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser mid SF band (section glue)** 695–701 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser mid SF band (section glue)** 715–721 (7 lines)

**Credited native LOC:** 213 (tally `nativeLoc` 213).

## Managed

`VmFScriptParserSf.cs`
Managed LOC (tally): 109.
Conversion commit: `fb08990aab253f427ac26252d5fafbab31905e1f`.

## Padding review (2026-10-21)

| Managed | Native (`t_func.cpp`) |
|---------|-------------------------|
| `FsTerminatorException` | `CFsTerminator` |
| `SfGoto` | **509–524** |
| (section glue) | **525–531** |
| `SfReturn` | **532–535** |
| (section glue) | **536–542** |
| `SfInclude` | **543–559** |
| (section glue) | **560–566** |
| `SfInput` | **567–570** |
| (section glue) | **571–577** |
| `SfBeep` | **578–581** |
| (section glue) | **582–588** |
| `SfClock` | **589–593** |
| (section glue) | **594–602** |
| `SfExitLevel` | **603–606** |
| (section glue) | **607–613** |
| `SfTip` | **614–621** |
| (section glue) | **622–632** |
| `SfTimedTip` | **633–642** |
| (section glue) | **643–650** |
| `SfPlayerTip` | **651–661** |
| (section glue) | **662–668** |
| `SfMessage` | **669–676** |
| (section glue) | **677–683** |
| `SfPlayerMsg` | **684–694** |
| (section glue) | **695–701** |
| `SfPlayerInGame` | **702–714** |
| (section glue) | **715–721** |

**Post-review ratio:** 109/213 managed/native ≈ 51% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfTests.SfGoto_Label_SetsRover`
- `VmFScriptParserSfTests.SfReturn_ThrowsTerminator`
- `VmFScriptParserSfTests.SfInclude_String_TruncatesToEightChars`
- `VmFScriptParserSfTests.SfGoto_NonLabel_ScriptError`
- `VmFScriptParserSfTests.SfPlayerInGame_ValidPlayer_ReturnsOne`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 838 --to 838` (0 hard / 0 soft).

## Seams

`ParseInclude`, console print, sound, and `ExitLevel` are host callbacks; `SF_Return` maps to `FsTerminatorException`.

## Credit rationale

Whole-function band after parser core (**837**) through `SF_PlayerInGame` before player-name/spawn API (**839**).
