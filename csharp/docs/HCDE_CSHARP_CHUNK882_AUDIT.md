# Chunk 882 — Cajun `FCajunMaster` lifecycle, spawn scheduling, net add-bot, and default `LoadBots` from `b_game.cpp` (117–656; excludes destructor, platform path helpers, `ADD_STAT`).

## Scope

Cajun `FCajunMaster` lifecycle, spawn scheduling, net add-bot, and default `LoadBots` from `b_game.cpp` (117–656; excludes destructor, platform path helpers, `ADD_STAT`).

## Native ranges

- `src/playsim/bots/b_game.cpp` **b_game statics** 85–109 (25 lines)
- `src/playsim/bots/b_game.cpp` **FCajunMaster::Main** 117–145 (29 lines)
- `src/playsim/bots/b_game.cpp` **FCajunMaster::Init** 147–179 (33 lines)
- `src/playsim/bots/b_game.cpp` **FCajunMaster::End** 182–207 (26 lines)
- `src/playsim/bots/b_game.cpp` **FCajunMaster::SpawnBot** 222–322 (101 lines)
- `src/playsim/bots/b_game.cpp` **FCajunMaster::TryAddBot** 324–366 (43 lines)
- `src/playsim/bots/b_game.cpp` **FCajunMaster::DoAddBot** 368–404 (37 lines)
- `src/playsim/bots/b_game.cpp` **FCajunMaster::RemoveAllBots** 406–442 (37 lines)
- `src/playsim/bots/b_game.cpp` **FCajunMaster::ForgetBots** 465–477 (13 lines)
- `src/playsim/bots/b_game.cpp` **FCajunMaster::LoadBots default seed** 516–541 (26 lines)

**Credited native LOC:** 370 (tally `nativeLoc` 370).

## Managed

`VmBBotGame.cs`
Managed LOC (tally): 320.
Conversion commit: `c24b699af0fbcb96b68d1129ac6580794e375710`.

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

**Post-review ratio:** 320/370 managed/native ≈ 86% (src file LOC at HEAD; see tally).

## Tests

- `VmBBotGameTests.LoadBots_SeedsDefaultBots`
- `VmBBotGameTests.Main_DecrementsJoinTimerAndSchedulesSpawn`
- `VmBBotGameTests.SpawnBot_WritesDemAddBotAndDrainAddsPlayer`
- `VmBBotGameTests.SpawnBot_FailsWhenNamedBotMissing`
- `VmBBotGameTests.DoAddBot_AddsCajunPlayerSlot`
- `VmBBotGameTests.RemoveAllBots_ClearsCajunPlayers`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 882 --to 882` (0 hard / 0 soft).

## Seams

Headless authority: `HeadlessGameSession.Tick` calls `VmBBotGame.Main` then `AuthoritySimulation.DrainNetDemoCommands` (`DEM_ADDBOT` → `TryAddBot`, matching `d_net.cpp` / `b_game.cpp`). `SpawnBot` writes `NetDemoEventBytes` like native `Net_Write*`. CCMD `addbot` calls `VmBBotGame.SpawnBot` + drain on `Cajun` + `Simulation` (no optional host stub). `DoAddBot` → `TryAddCajunBotPlayer`.

## Chunk 877 overlap (post-review)

| | |
|--|--|
| **877** native (`b_bot.cpp`) | **882** credits **0** lines — no re-credit |
| **882** native (`b_game.cpp`) | **877** credits **0** lines |
| Managed delta on **877** files | `BotInfoNode.Info`/`Skill`; `RunAddBotCcmd` → `SpawnBot`+drain — **877** behavior table unchanged (`Tick`/`Think` still **no**) |

## Credit rationale

Whole uncredited `b_game.cpp` TU (**665** lines) adjacent to landed **877** `b_bot.cpp`; dense spawn/net/lifecycle without overlapping **875** movement credit.
