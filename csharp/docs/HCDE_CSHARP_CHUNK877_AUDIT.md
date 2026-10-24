# Chunk 877 — Cajun bot lifecycle, CCMD bodies + registration, and BOTSUPP init from `b_bot.cpp` (1–304).

## Scope

Cajun bot lifecycle, CCMD bodies + registration, and BOTSUPP init from `b_bot.cpp` (1–304).

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

**Credited native LOC:** 287 (tally `nativeLoc` 287).

## Managed

`VmBBot.cs`, `VmBBotCcmd.cs`
Managed LOC (tally): 368.
Conversion commit: `bf756868fa846dfd822d34cacc622ddc087688d7`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native | Behavior ported |
|--------|-----------------|
| `DBot::Construct` / `DBot::Clear` | yes |
| `Serialize` (`botskill_t`, `DBot`) | yes (field snapshot API) |
| `DBot::Tick` (**128–142**) | **no** (`Think` deferred) |
| `CCMD` `addbot` / `removebots` / `freeze` / `listbots` | yes (`VmBBotCcmdRegister` → `Run*Ccmd`) |
| `FCajunMaster::ClearPlayer` | yes |
| `InitBotStuff` | yes (BOTSUPP lines + warn-bot missile flags) |

**Not ported / not registered**

| Native | Policy |
|--------|--------|
| `CVAR bot_next_color` (**144**) | **no** (zero gross/net) |
| `DBot::Construct` IMPLEMENT / pointer tables | Gross credit only |

| Managed | Native (`b_bot.cpp`) |
|---------|----------------------|
| `Clear` | **63–86** |
| `WriteSkillSnapshot` / `WriteBotSnapshot` | **88–99**, **101–126** |
| `Tick` | **128–142** |
| `VmBBotCcmd.RunAddBotCcmd` | **146–170** |
| `ClearPlayer` | **172–196** |
| `VmBBotCcmd.RunRemoveBotsCcmd` | **198–207** |
| `VmBBotCcmd.RunFreezeCcmd` | **209–222** |
| `VmBBotCcmd.RunListBotsCcmd` | **224–236** |
| `InitBotStuffFromLumps` | **243–304** |
| (headers / IMPLEMENT / CVAR / decl) | **1–44**, **45–56**, **58–61**, **144**, **238–241**, separators |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **288** |
| Net behavior-ported | **288** |

**Post-review ratio:** 368/287 managed/native ≈ 128% (src file LOC at HEAD; see tally).

## Tests

- `VmBBotTests.Clear_ZeroesPointersAndTimers`
- `VmBBotTests.WriteBotSnapshot_RecordsAngleAndSkill`
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

**877 corrective (2026-10-23):** removed `BotTickHost` / `ClearPlayerHost` / `InitBotHost` hooks; `ClearPlayer` calls `Actor.Destroy` directly; BOTSUPP uses `PickupCatalog.TryWeaponPickupClassName` + `ResolveActorClassName`. `DBot::Tick` deferred (**128–142**, zero credit — `Think` not landed). `CVAR bot_next_color` (**144**) zero gross/net. CCMD registration unchanged.

## Credit rationale

Whole uncredited TU (304 native) adjacent to landed **875** `b_move.cpp`; dense bot setup without overlapping movement credit.
