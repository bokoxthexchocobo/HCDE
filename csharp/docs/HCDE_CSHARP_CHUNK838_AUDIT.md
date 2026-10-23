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

`VmFScriptParserSf.cs`, `VmFScriptParserSfCallees.cs`
Managed LOC (tally): 181.
Conversion commit: `4010efd48cff9bb627f7bf52aa83040375c10d9f`.

## Padding review (2026-10-21)

| Managed | Native (`t_func.cpp` / callee) |
|---------|-------------------------|
| `FsTerminatorException` | **532–535** (`SF_Return` / `CFsTerminator`) |
| `SfGoto` | **509–524** |
| `SfReturn` | **532–535** |
| `SfInclude` / `ParseIncludeLump` | **543–559** (`Script->ParseInclude` → `t_prepro.cpp` **401–409** lump resolve; **411–424** queued) |
| `SfInput` / `Printf` (`FsPrintBold`) | **567–570** (`Printf`) |
| `SfBeep` / `PlayChatBeep` | **578–581** (`S_Sound` → `VmSoundEngineStartSound.StartSound`) |
| `SfClock` | **589–593** |
| `SfExitLevel` / `RequestExitLevel` | **603–606** (`Level->ExitLevel` exit intent fields) |
| `SfTip` / `ActorCheckLocalView` / `MidPrint` | **614–621** (`CheckLocalView`, `C_MidPrint`) |
| `SfTimedTip` / `ConMidtime` | **633–642** (`con_midtime`, `C_MidPrint`) |
| `SfPlayerTip` / `PlayerMo` + `ActorCheckLocalView` | **651–661** |
| `SfMessage` / `Printf` (`FsPrintHigh`) | **669–676** |
| `SfPlayerMsg` | **684–694** |
| `SfPlayerInGame` | **702–714** |
| (native separators) | **525–531**, **536–542**, **560–566**, **571–577**, **582–588**, **594–602**, **607–613**, **622–632**, **643–650**, **662–668**, **677–683**, **695–701**, **715–721** |

**Post-review ratio:** 181/213 managed/native ≈ 85% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfTests.SfGoto_Label_SetsRover`
- `VmFScriptParserSfTests.SfReturn_ThrowsTerminator`
- `VmFScriptParserSfTests.SfInclude_String_TruncatesToEightChars`
- `VmFScriptParserSfTests.SfGoto_NonLabel_ScriptError`
- `VmFScriptParserSfTests.SfPlayerInGame_ValidPlayer_ReturnsOne`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 838 --to 838` (0 hard / 0 soft).

## Seams

No `ParserContext` forwarding delegates. `SF_Return` → `FsTerminatorException`. `DFsScript::ParseInclude` (`t_prepro.cpp` 401–425): lump resolve in `ParseIncludeLump`; read/preprocess/`FParser::Run` (**411–424**) uncredited — `IncludeLumpRequests` queues lump id for fs/level integration when `Run` is ported (no delegate).

## Credit rationale

Whole-function band after parser core (**837**) through `SF_PlayerInGame` before player-name/spawn API (**839**).
