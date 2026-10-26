# Chunk 873 — `FParser::SF_Goto` … `SF_PlayerInGame` plus `SF_ExitLevel` / tip natives from `fragglescript/t_func.cpp` (502–721). Retired (2026-10-23): out-of-body credit removed; honest **98** < **150**.

## Scope

`FParser::SF_Goto` … `SF_PlayerInGame` plus `SF_ExitLevel` / tip natives from `fragglescript/t_func.cpp` (502–721). Retired (2026-10-23): out-of-body credit removed; honest **98** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `f3f9bbade90f381835a16b1ba3f445792297c249`.

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

### Out-of-body uncredit (873)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **203** |
| **Out-of-body removed** | **105** |
| **Honest** | **98** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 873 --to 873` (0 hard / 0 soft).

## Seams

Uses **837** `PrintHigh` / `GameTic`, **742** local view, and `VmSoundEngineStartSound` for credited paths. `SF_Include` / `SF_ExitLevel` console hosts deferred; `exitlevel` maps to `MarkExited`.

## Credit rationale

Honest re-credit of retired **852** footprint plus additional ported `SF_*` in the same uncredited **502–721** island to meet the **150** native floor without padding helpers.
