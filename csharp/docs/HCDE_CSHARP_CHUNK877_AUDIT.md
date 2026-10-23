# Chunk 877 — Cajun bot lifecycle, console hooks, and BOTSUPP init from `b_bot.cpp` (1–304).

## Scope

Cajun bot lifecycle, console hooks, and BOTSUPP init from `b_bot.cpp` (1–304).

## Native ranges

- `src/playsim/bots/b_bot.cpp` **b_bot headers** 1–44 (44 lines)
- `src/playsim/bots/b_bot.cpp` **IMPLEMENT_CLASS DBot** 45–56 (12 lines)
- `src/playsim/bots/b_bot.cpp` **b_bot separators** 57–57 (1 lines)
- `src/playsim/bots/b_bot.cpp` **DBot::Construct** 58–61 (4 lines)
- `src/playsim/bots/b_bot.cpp` **b_bot separators** 62–62 (1 lines)
- `src/playsim/bots/b_bot.cpp` **DBot::Clear** 63–86 (24 lines)
- `src/playsim/bots/b_bot.cpp` **b_bot separators** 87–87 (1 lines)
- `src/playsim/bots/b_bot.cpp` **Serialize botskill_t** 88–99 (12 lines)
- `src/playsim/bots/b_bot.cpp` **b_bot separators** 100–100 (1 lines)
- `src/playsim/bots/b_bot.cpp` **DBot::Serialize** 101–126 (26 lines)
- `src/playsim/bots/b_bot.cpp` **b_bot separators** 127–127 (1 lines)
- `src/playsim/bots/b_bot.cpp` **DBot::Tick** 128–142 (15 lines)
- `src/playsim/bots/b_bot.cpp` **b_bot separators** 143–143 (1 lines)
- `src/playsim/bots/b_bot.cpp` **CVAR bot_next_color** 144–144 (1 lines)
- `src/playsim/bots/b_bot.cpp` **b_bot separators** 145–145 (1 lines)
- `src/playsim/bots/b_bot.cpp` **CCMD addbot** 146–170 (25 lines)
- `src/playsim/bots/b_bot.cpp` **b_bot separators** 171–171 (1 lines)
- `src/playsim/bots/b_bot.cpp` **FCajunMaster::ClearPlayer** 172–196 (25 lines)
- `src/playsim/bots/b_bot.cpp` **b_bot separators** 197–197 (1 lines)
- `src/playsim/bots/b_bot.cpp` **CCMD removebots** 198–207 (10 lines)
- `src/playsim/bots/b_bot.cpp` **b_bot separators** 208–208 (1 lines)
- `src/playsim/bots/b_bot.cpp` **CCMD freeze** 209–222 (14 lines)
- `src/playsim/bots/b_bot.cpp` **b_bot separators** 223–223 (1 lines)
- `src/playsim/bots/b_bot.cpp` **CCMD listbots** 224–236 (13 lines)
- `src/playsim/bots/b_bot.cpp` **b_bot separators** 237–237 (1 lines)
- `src/playsim/bots/b_bot.cpp` **BotInfoMap decl** 238–241 (4 lines)
- `src/playsim/bots/b_bot.cpp` **b_bot separators** 242–242 (1 lines)
- `src/playsim/bots/b_bot.cpp` **InitBotStuff** 243–304 (62 lines)

**Credited native LOC:** 304 (tally `nativeLoc` 304).

## Managed

`VmBBot.cs`
Managed LOC (tally): 358.
Conversion commit: `9da82ab0dfa89c02c62768f30f9ab8d95891bcf5`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native | Behavior ported |
|--------|-----------------|
| `DBot::Construct` / `DBot::Clear` | yes |
| `Serialize` (`botskill_t`, `DBot`) | yes (field snapshot API) |
| `DBot::Tick` | yes |
| `CCMD` `addbot` / `removebots` / `freeze` / `listbots` | yes |
| `FCajunMaster::ClearPlayer` | yes |
| `InitBotStuff` | yes (BOTSUPP lines + warn-bot missile flags) |

| Managed | Native (`b_bot.cpp`) |
|---------|----------------------|
| `Clear` | **63–86** |
| `WriteSkillSnapshot` / `WriteBotSnapshot` | **88–99**, **101–126** |
| `Tick` | **128–142** |
| `TryAddBot` | **146–170** |
| `ClearPlayer` | **172–196** |
| `TryRemoveBots` | **198–207** |
| `TryFreeze` | **209–222** |
| `ListBots` | **224–236** |
| `InitBotStuffFromLumps` | **243–304** |
| (headers / IMPLEMENT / CVAR / decl) | **1–44**, **45–56**, **58–61**, **144**, **238–241**, separators |

### Gross vs net

| | Value |
|--|------:|
| `nativeLoc` | **304** |

**Post-review ratio:** 358/304 managed/native ≈ 118% (src file LOC at HEAD; see tally).

## Tests

- `VmBBotTests.Clear_ZeroesPointersAndTimers`
- `VmBBotTests.WriteBotSnapshot_RecordsAngleAndSkill`
- `VmBBotTests.Tick_InvokesThinkWhenActive`
- `VmBBotTests.AddBot_SpawnsWhenAllowed`
- `VmBBotTests.ClearPlayer_DestroysBotAndClearsInGame`
- `VmBBotTests.RemoveBots_WritesKillWhenAllowed`
- `VmBBotTests.Freeze_WritesCheatWhenAllowed`
- `VmBBotTests.ListBots_PrintsActiveSuffix`
- `VmBBotTests.InitBotStuff_ParsesWeaponAndFlags`
- `VmBBotTests.InitBotStuff_RejectsNonWeaponType`
- `VmBBotTests.ParseBotSupp_UnknownProjectileTokenThrows`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 877 --to 877` (0 hard / 0 soft).

## Seams

`Think`, level freeze, `SpawnBot`, `Net_Write*`, and `FSerializer` hosts stay at call sites; snapshot helpers mirror native field lists without serializer framework coupling.

## Credit rationale

Whole uncredited TU (304 native) adjacent to landed **875** `b_move.cpp`; dense bot setup without overlapping movement credit.
