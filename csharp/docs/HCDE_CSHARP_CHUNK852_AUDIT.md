# Chunk 852 — Retired: honest whole-function credit for `t_func.cpp` 502–721 matches retired **838** at **105** native lines (<150). Managed bodies for the seven ported `SF_*` live in `VmFScriptParserSfCtrl.cs` without matrix credit until `ParseInclude`, `Printf(PRINT_BOLD)`, `Level->ExitLevel`, and `C_MidPrint`/`con_midtime` perform native effects in-tree (no `ParserContext` no-op hooks).

## Scope

Retired: honest whole-function credit for `t_func.cpp` 502–721 matches retired **838** at **105** native lines (<150). Managed bodies for the seven ported `SF_*` live in `VmFScriptParserSfCtrl.cs` without matrix credit until `ParseInclude`, `Printf(PRINT_BOLD)`, `Level->ExitLevel`, and `C_MidPrint`/`con_midtime` perform native effects in-tree (no `ParserContext` no-op hooks).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmFScriptParserSfCtrl.cs`
Managed LOC (tally): 0.
Conversion commit: `a7a55cd0839ecac0abd5101de676c610bca8ba24`.

## Padding review (2026-10-21)

### Comparison to retired **838** (function-by-function)

| Native `SF_*` | **838** behavior ported | **852** managed | How former **838** seam/gap is resolved |
|---------------|-------------------------|-----------------|----------------------------------------|
| `SF_Goto` | yes | yes (`SfGoto`) | In-band; no host seam (same as **838**). |
| `SF_Return` | yes | yes (`SfReturn` → `FsTerminatorException`) | In-band; no host seam (same as **838**). |
| `SF_Include` | no (`ParseInclude`) | **deferred** (not in `BuildTable`) | **838** gap unchanged: needs `DFsScript::ParseInclude` (**853** `VmFScriptPrepro.ParseInclude`) plus a real included-script runner, not `ParserContext.ParseInclude` no-op. |
| `SF_Input` | no (`Printf(PRINT_BOLD)`) | **deferred** | **838** gap unchanged: no managed `PRINT_BOLD` console path; removed `PrintBold` hook. |
| `SF_Beep` | yes (`VmSoundEngineStartSound`) | yes (`SfBeep` → `StartSoundEngine` default `VmSoundEngineStartSound.StartSound`, `misc/chat`, `ATTN_IDLE`) | **838** seam resolved: no `PlayChatBeep` callback; sound engine port performs effect when enabled. |
| `SF_Clock` | yes | yes (`SfClock` + `GameTic` on **837** `ParserContext`) | **838** gap resolved: centiseconds from `GameTic`/`TICRATE`, not a no-op `GameTic` hook default. |
| `SF_ExitLevel` | no (`Level->ExitLevel`) | **deferred** | **838** gap unchanged: no level-exit managed callee; removed `ExitLevel` hook. |
| `SF_Tip` | no (`C_MidPrint`) | **deferred** | **838** gap unchanged: removed `MidPrint`/`TriggerLocalView` hooks. |
| `SF_TimedTip` | no (`C_MidPrint` + `con_midtime`) | **deferred** | **838** gap unchanged: removed `ConMidTime`/`SetConMidTime` hooks. |
| `SF_PlayerTip` | no (`C_MidPrint`) | **deferred** | **838** gap unchanged (same as `SF_Tip`). |
| `SF_Message` | yes (**837** `PrintHigh`) | yes (`SfMessage` + `VmPMobjLocalViewTouch.CheckLocalView`) | **838** `PrintHigh` path kept; local view uses **742** query object, not empty `TriggerLocalView` default. |
| `SF_PlayerMsg` | yes (**837** `PrintHigh`) | yes (`SfPlayerMsg`, same callees) | Same as `SF_Message`. |
| `SF_PlayerInGame` | yes | yes (`SfPlayerInGame` + **837** `PlayerInGame`) | In-band; no new seam. |

**Honest native lines** (ported `SF_*` + separators between consecutive ported functions only): **105** — same as **838**; **150** floor not met → row **retired** (no duplicate dishonest **220**-line credit).

| Managed (off-tally) | Native (`t_func.cpp`) |
|---------------------|-------------------------|
| `SfGoto` | **509–524** |
| `SfReturn` | **532–535** |
| `SfBeep` | **578–581** |
| `SfClock` | **589–593** |
| `SfMessage` | **669–676** |
| `SfPlayerMsg` | **684–694** |
| `SfPlayerInGame` | **702–714** |
| (separators) | **502–508**, **525–531**, **582–588**, **662–668**, **677–683**, **695–701**, **715–721** |

### Chunk **852** gross vs net (correction)

| | Value |
|--|------:|
| Prior dishonest row `nativeLoc` | **220** (reverted at retire) |
| Honest native footprint | **105** |
| Matrix credit after retire | **0** |

## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 852 --to 852` (0 hard / 0 soft).

## Seams

Uses **837** roster/`PrintHigh`, **742** `VmPMobjLocalViewTouch`, and `VmSoundEngineStartSound` for credited paths only. Deferred `SF_*` are not registered in `BuildTable`.

## Credit rationale

Band **502–721** overlaps retired **838** (**509–721**); matrix row retired at the same honest native footprint as **838** after removing seam-based credit.
