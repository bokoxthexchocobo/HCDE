# Chunk 852 — `FParser::SF_Goto` … `SF_PlayerInGame` from `fragglescript/t_func.cpp` (502–721).

## Scope

`FParser::SF_Goto` … `SF_PlayerInGame` from `fragglescript/t_func.cpp` (502–721).

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

**Credited native LOC:** 220 (tally `nativeLoc` 220).

## Managed

`VmFScriptParserSfCtrl.cs`
Managed LOC (tally): 116.
Conversion commit: `3c30873bec8eb83ed47bb9231463e69b3d2b5a32`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported |
|---------------|-----------------|
| `SF_Goto` | yes |
| `SF_Return` | yes |
| `SF_Include` | yes |
| `SF_Input` | yes |
| `SF_Beep` | yes |
| `SF_Clock` | yes |
| `SF_ExitLevel` | yes |
| `SF_Tip` | yes |
| `SF_TimedTip` | yes |
| `SF_PlayerTip` | yes |
| `SF_Message` | yes |
| `SF_PlayerMsg` | yes |
| `SF_PlayerInGame` | yes |

| Managed | Native (`t_func.cpp` 502–721) |
|---------|-------------------------|
| `SfGoto` | **509–524** |
| `SfReturn` | **532–535** |
| `SfInclude` | **543–559** |
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
| (separators) | **502–508**, **525–531**, **536–542**, **560–566**, **571–577**, **582–588**, **594–602**, **607–613**, **622–632**, **643–650**, **662–668**, **677–683**, **695–701**, **715–721** |

### Chunk **852** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` (gross **502–721**) | **220** |
| **Net headline** | **+220** vs post-**851** (expected at land sync) |

**Post-review ratio:** 116/220 managed/native ≈ 53% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfCtrlTests.SfGoto_JumpsToLabel`
- `VmFScriptParserSfCtrlTests.SfReturn_ThrowsTerminator`
- `VmFScriptParserSfCtrlTests.SfInclude_CallsParseInclude`
- `VmFScriptParserSfCtrlTests.SfInput_PrintsBoldNotice`
- `VmFScriptParserSfCtrlTests.SfBeep_StartsChatSound`
- `VmFScriptParserSfCtrlTests.SfClock_ReturnsCentiseconds`
- `VmFScriptParserSfCtrlTests.SfExitLevel_InvokesHost`
- `VmFScriptParserSfCtrlTests.SfTip_MidPrintsWhenLocalView`
- `VmFScriptParserSfCtrlTests.SfTimedTip_AdjustsMidTime`
- `VmFScriptParserSfCtrlTests.SfPlayerTip_MidPrintsForPlayer`
- `VmFScriptParserSfCtrlTests.SfMessage_PrintsHigh`
- `VmFScriptParserSfCtrlTests.SfPlayerMsg_PrintsHighForPlayer`
- `VmFScriptParserSfCtrlTests.SfPlayerInGame_ReturnsSlotState`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 852 --to 852` (0 hard / 0 soft).

## Seams

Uses `FsTerminatorException`, `VmSoundEngineStartSound`, and host hooks on `ParserContext` for `ParseInclude`, `PrintBold`, `ExitLevel`, and `MidPrint`.

## Credit rationale

Reopens retired **838** band with full callee wiring; **839** player/spawn follows at **722**.
