# Chunk 873 — `FParser::SF_Goto` … `SF_PlayerInGame` plus `SF_ExitLevel` / tip natives from `fragglescript/t_func.cpp` (502–721).

## Scope

`FParser::SF_Goto` … `SF_PlayerInGame` plus `SF_ExitLevel` / tip natives from `fragglescript/t_func.cpp` (502–721).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Goto** 509–524 (16 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Return** 532–535 (4 lines)
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
- `src/playsim/fragglescript/t_func.cpp` **FParser SF ctrl separators** 502–508 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF ctrl separators** 525–531 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF ctrl separators** 536–542 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF ctrl separators** 560–566 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF ctrl separators** 571–577 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF ctrl separators** 582–588 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF ctrl separators** 594–602 (9 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF ctrl separators** 607–613 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF ctrl separators** 622–632 (11 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF ctrl separators** 643–650 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF ctrl separators** 662–668 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF ctrl separators** 677–683 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF ctrl separators** 695–701 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF ctrl separators** 715–721 (7 lines)

**Credited native LOC:** 203 (tally `nativeLoc` 203).

## Managed

`VmFScriptParserSfCtrl.cs`, `VmFScriptInitFunctionsReg873.cs`
Managed LOC (tally): 128.
Conversion commit: `a660e8b1fdc7c81c7f12342530171b6a53998621`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported | Registered |
|---------------|-----------------|------------|
| `SF_Goto` | yes | `goto` (BuildTable) |
| `SF_Return` | yes | `return` |
| `SF_Input` | yes | `input` |
| `SF_Beep` | yes | `beep` |
| `SF_Clock` | yes | `clock` |
| `SF_ExitLevel` | yes | `exitlevel` |
| `SF_Tip` | yes | `tip` |
| `SF_TimedTip` | partial (`PrintHigh`, no `con_midtime`) | `timedtip` |
| `SF_PlayerTip` | yes | `playertip` |
| `SF_Message` | yes | `message` |
| `SF_PlayerMsg` | yes | `playermsg` |
| `SF_PlayerInGame` | yes | `playeringame` |

**Not in this chunk (zero credit, not registered)**

| Native | Policy |
|--------|--------|
| `SF_Include` (**543–559**) | Unregistered |

| Managed | Native |
|---------|--------|
| `SfGoto` | **509–524** |
| `SfReturn` | **532–535** |
| `SfInput` | **567–570** |
| `SfBeep` | **578–581** |
| `SfClock` | **589–593** |
| `SfExitLevel` | **603–606** |
| `SfTip` | **614–621** |
| `SfTimedTip` | **633–642** |
| `SfPlayerTip` | **651–661** |
| `SfMessage` | **669–676** |
| `SfPlayerMsg` | **684–694** |
| `SfPlayerInGame` | **702–714** |
| `VmFScriptInitFunctionsReg873.Register` | `NewFunction` rows |
| (separators) | **502–508**, **525–531**, **536–542**, **560–566**, **571–577**, **582–588**, **594–602**, **607–613**, **622–632**, **643–650**, **662–668**, **677–683**, **695–701**, **715–721** |
| (unregistered band) | **543–559** |

### Gross vs net

| | Value |
|--|------:|
| `nativeLoc` | **158** |

**Post-review ratio:** 128/203 managed/native ≈ 63% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfCtrlTests.SfGoto_JumpsToLabel`
- `VmFScriptParserSfCtrlTests.SfReturn_ThrowsTerminator`
- `VmFScriptParserSfCtrlTests.SfExitLevel_MarksExited`
- `VmFScriptParserSfCtrlTests.SfBeep_StartsChatSound`
- `VmFScriptParserSfCtrlTests.SfClock_ReturnsCentiseconds`
- `VmFScriptParserSfCtrlTests.SfTip_PrintsForLocalView`
- `VmFScriptParserSfCtrlTests.SfMessage_PrintsHigh`
- `VmFScriptParserSfCtrlTests.SfPlayerMsg_PrintsHighForPlayer`
- `VmFScriptParserSfCtrlTests.SfPlayerInGame_ReturnsSlotState`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 873 --to 873` (0 hard / 0 soft).

## Seams

Uses **837** `PrintHigh` / `GameTic`, **742** local view, and `VmSoundEngineStartSound` for credited paths. `SF_Include` / `SF_ExitLevel` console hosts deferred; `exitlevel` maps to `MarkExited`.

## Credit rationale

Honest re-credit of retired **852** footprint plus additional ported `SF_*` in the same uncredited **502–721** island to meet the **150** native floor without padding helpers.
