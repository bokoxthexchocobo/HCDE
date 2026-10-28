# Chunk 882 — Cajun `FCajunMaster` lifecycle, spawn scheduling, net add-bot, and default `LoadBots` from `b_game.cpp` (117–656; excludes destructor, platform path helpers, `ADD_STAT`).

## Scope

Cajun `FCajunMaster` lifecycle, spawn scheduling, net add-bot, and default `LoadBots` from `b_game.cpp` (117–656; excludes destructor, platform path helpers, `ADD_STAT`).

## Native ranges

- `src/playsim/bots/b_game.cpp` **FCajunMaster::Main** 117–145 (29 lines)
- `src/playsim/bots/b_game.cpp` **FCajunMaster::Init** 147–179 (33 lines)
- `src/playsim/bots/b_game.cpp` **FCajunMaster::End** 182–207 (26 lines)
- `src/playsim/bots/b_game.cpp` **FCajunMaster::SpawnBot** 222–322 (101 lines)
- `src/playsim/bots/b_game.cpp` **FCajunMaster::TryAddBot** 324–366 (43 lines)
- `src/playsim/bots/b_game.cpp` **FCajunMaster::DoAddBot** 368–404 (37 lines)
- `src/playsim/bots/b_game.cpp` **FCajunMaster::RemoveAllBots** 406–442 (37 lines)
- `src/playsim/bots/b_game.cpp` **FCajunMaster::ForgetBots** 465–477 (13 lines)
- `src/playsim/bots/b_game.cpp` **FCajunMaster::LoadBots default seed** 516–541 (26 lines)

**Credited native LOC:** 345 (tally `nativeLoc` 345).

## Managed

`VmBBotGame.cs`
Managed LOC (tally): 380.
Conversion commit: `60d8c1c62c51fd9272dcaf3627760e34f15f3efe`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native | Behavior ported |
|--------|-----------------|
| `FCajunMaster::Main` | yes |
| `FCajunMaster::Init` / `End` | yes |
| `FCajunMaster::SpawnBot` | yes |
| `FCajunMaster::TryAddBot` / `DoAddBot` | yes |
| `FCajunMaster::RemoveAllBots` / `ForgetBots` | yes |
| `FCajunMaster::LoadBots` (default bots path) | yes |
| `M_GetCajunPath` (**479–514**) | **no** (platform IO deferred) |
| `ADD_STAT(bots)` (**658–665**) | **no** (stats only) |

| Managed | Native (`b_game.cpp`) |
|---------|----------------------|
| `Main` | **117–145** |
| `Init` | **147–179** |
| `End` | **182–207** |
| `SpawnBot` | **222–322** |
| `TryAddBot` | **324–366** |
| `DoAddBot` | **368–404** |
| `RemoveAllBots` | **406–442** |
| `ForgetBots` | **465–477** |
| `LoadBots` (default seed) | **516–541** |
| color suffix statics | **85–109** |

**Padding map (retired from credit, not ported)**

| Native lines | Reason |
|--------------|--------|
| **209–221**, **443–464**, **542–657** | blank separators / file tail outside claimed behavior |
| **111–115**, **323**, **367**, **405**, **444–464** (in-band), **478**, **515**, destructor **658–665** | uncredited neighbors left for later rows |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` (chunk JSON ranges) | **370** |
| Net behavior-ported | **370** |
| Retired padding (excluded from JSON) | **151** |

**Chunk 877 managed overlap (no native re-credit)**

| | |
|--|--|
| Native credited to **877** (`b_bot.cpp` **1–304**) | **unchanged** — **882** claims **zero** `b_bot.cpp` lines |
| Native credited to **882** (`b_game.cpp` bands above) | **unchanged** — **877** claims **zero** `b_game.cpp` lines |
| `VmBBot.BotInfoNode.Info` / `Skill` (+2 fields) | **877** tally `managedLoc` **368→379**; supports `LoadBots`/`SpawnBot` only |
| `VmBBotCcmd.RunAddBotCcmd` | calls `VmBBotGame.SpawnBot` when `Cajun`+`Simulation` set; **877** CCMD body lines **146–170** still accurate |
| **877** behavior table | still valid (`DBot::Tick`/`Think` **no**, four CCMDs **yes**) |

### Out-of-body uncredit (882)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **370** |
| **Out-of-body removed** | **25** |
| **Honest** | **345** |


**Post-review ratio:** 380/345 managed/native ≈ 110% (src file LOC at HEAD; see tally).

## Tests

- `VmBBotGameTests.Main_DecrementsJoinTimerAndSchedulesSpawn`
- `VmBBotGameTests.LoadBots_SeedsDefaultBots`
- `VmBBotGameTests.SpawnBot_WritesDemAddBotAndDrainAddsPlayer`
- `VmBBotGameTests.SpawnBot_FailsWhenNamedBotMissing`
- `VmBBotGameTests.DoAddBot_AddsCajunPlayerSlot`
- `VmBBotGameTests.RemoveAllBots_ClearsCajunPlayers`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 882 --to 882` (0 hard / 0 soft).

## Seams

Headless authority: `HeadlessGameSession.Tick` calls `VmBBotGame.Main` then `AuthoritySimulation.DrainNetDemoCommands` (native `Net_DoCommand` `DEM_ADDBOT` → `TryAddBot`). `SpawnBot` uses `Net_WriteInt8`/`Net_WriteString` on `NetDemoEventBytes`. CCMD `addbot` calls `VmBBotGame.SpawnBot` + drain directly (no `CcmdHost` branch). `DoAddBot` → `TryAddCajunBotPlayer`.

## Credit rationale

Whole uncredited `b_game.cpp` TU (**665** lines) adjacent to landed **877** `b_bot.cpp`; dense spawn/net/lifecycle without overlapping **875** movement credit.
