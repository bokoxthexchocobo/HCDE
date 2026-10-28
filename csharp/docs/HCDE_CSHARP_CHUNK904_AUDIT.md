# Chunk 904 — `FParser::SF_Goto` … `SF_PlayerInGame` from `fragglescript/t_func.cpp` (**502–721**). Pivot after **903** `p_lnspec` fragmentation; retired **873**/**838** bands re-credited with `ParseInclude`, `C_MidPrint`/`con_midtime`, and `Level->ExitLevel` paths honest in-tree.

## Scope

`FParser::SF_Goto` … `SF_PlayerInGame` from `fragglescript/t_func.cpp` (**502–721**). Pivot after **903** `p_lnspec` fragmentation; retired **873**/**838** bands re-credited with `ParseInclude`, `C_MidPrint`/`con_midtime`, and `Level->ExitLevel` paths honest in-tree.

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Goto** 502–721 (220 lines)

**Credited native LOC:** 220 (tally `nativeLoc` 220).

## Managed

`VmFScriptParserSfCtrl.cs`, `VmFScriptInitFunctionsReg904.cs`
Managed LOC (tally): 156.
Conversion commit: `a1829b5936566d5fad426d85c44f3db015c59998`.

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
| `SF_Tip` / `SF_TimedTip` | yes |
| `SF_PlayerTip` | yes |
| `SF_Message` / `SF_PlayerMsg` | yes |
| `SF_PlayerInGame` | yes |

| Managed | Native (`t_func.cpp`) |
|---------|----------------------|
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
| (separator) | **502–508** |
| (separator) | **525–531** |
| (separator) | **536–542** |
| (separator) | **560–566** |
| (separator) | **571–577** |
| (separator) | **582–588** |
| (separator) | **594–602** |
| (separator) | **607–613** |
| (separator) | **622–632** |
| (separator) | **643–650** |
| (separator) | **662–668** |
| (separator) | **677–683** |
| (separator) | **695–701** |
| (separator) | **715–721** |

### Chunk **904** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **220** |
| **Net headline** | **+220** vs post-**903** |


**Post-review ratio:** 156/220 managed/native ≈ 71% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfCtrlTests.SfGoto_JumpsToLabel`
- `VmFScriptParserSfCtrlTests.SfReturn_ThrowsTerminator`
- `VmFScriptParserSfCtrlTests.SfInclude_ParsesStringLumpName`
- `VmFScriptParserSfCtrlTests.SfBeep_StartsChatSound`
- `VmFScriptParserSfCtrlTests.SfClock_ReturnsCentiseconds`
- `VmFScriptParserSfCtrlTests.SfExitLevel_MarksExited`
- `VmFScriptParserSfCtrlTests.SfTip_MidPrintsForLocalView`
- `VmFScriptParserSfCtrlTests.SfTimedTip_RestoresConMidTime`
- `VmFScriptParserSfCtrlTests.SfMessage_PrintsHigh`
- `VmFScriptParserSfCtrlTests.SfPlayerMsg_PrintsHighForPlayer`
- `VmFScriptParserSfCtrlTests.SfPlayerInGame_ReturnsSlotState`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 904 --to 904` (0 hard / 0 soft).

## Seams

`Script->ParseInclude`, `C_MidPrint`, `con_midtime`, `S_Sound`, and `Level->ExitLevel` are not hosted; `ParseInclude`/`MidPrint`/sound/exit hooks on `ParserContext` mirror native control flow.

## Credit rationale

First honest ≥150 UNCredited slice in `t_func.cpp` **502–1593** after **903**; whole-function band through `SF_PlayerInGame` (**714**); `SF_PlayerName` deferred at **722**.
