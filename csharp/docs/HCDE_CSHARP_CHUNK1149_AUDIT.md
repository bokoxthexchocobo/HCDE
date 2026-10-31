# Chunk 1149 — Player search and A_Look from `p_enemy.cpp` (**1778–2062**); follows **1148**. A_LookEx at **2063** is already credited.

## Scope

Player search and A_Look from `p_enemy.cpp` (**1778–2062**); follows **1148**. A_LookEx at **2063** is already credited.

## Native ranges

- `src/playsim/p_enemy.cpp` **Enemy player look and A_Look** 1778–2062 (285 lines)

**Credited native LOC:** 285 (tally `nativeLoc` 285).

## Managed

`VmPEnemyWake1149Slice.cs`
Managed LOC (tally): 273.
Conversion commit: `4f9d4912179927c9b1bc324082e5b853ecac2822`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `P_LookForPlayers` (**1789–1933**) | yes |
| `A_Look` (**1944–2054**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1258–1488** | look scan already credited by **1148** |
| **1811–1842** | `MBF_FRIENDS` is not defined, so the block is not compiled |
| **2063+** | `A_LookEx` already credited |

`LOF_DONTCHASEGOAL` is **4**. With no parameter block, goals are chased. A hate tid returns immediately on a tid hit, and returns false without `MF3_HUNTPLAYERS` (**0x80000000**). A friendly with no friend player and either no team play or `TEAM_NONE` (**255**) returns the enemy search. A friendly who does have a friend player returns true in coop even when no enemy was found, and falls through only in deathmatch. `MF8_SEEFRIENDLYMONSTERS` returns only when an enemy was found. A dead player on a primary solo non-title map looks for monsters only when the game is outside `GAME_DoomStrifeChex` (**25**) and there is no goal. Otherwise the scan starts at the next slot after `LastLook`, or after `pr_look2() & 63` for a hate tid, and gives every one of the **64** slots one chance. Empty slots still consume a chance. `LastLook` advances only for occupied slots when there is no hate tid. A player who is the current goal clears `reactiontime`. After the mask, a goal is adopted unless chasing goals is off, otherwise a living unfriendly last enemy is adopted and a friendly one is dropped. The final result is whether the existing target is the goal. `A_Look` returns before doing anything during conversation. `Thing_SetGoal` (**229**) with `args[0] == 0` takes the first patrol point for `args[1]`, clears the special, sets `reactiontime` to `args[2] * 35 + maptime`, and toggles `MF5_CHASEGOAL` from `args[3]`. A dead heard actor is ignored. A heard player with `CF_NOTARGET` or without `MF_FRIENDLY` aborts after the threshold is cleared. Sector sound is used when `COMPATF_SOUNDTARGET` or `MF_NOSECTOR` is set. Stealth sets `visdir` to **-1**. A friendly heard target looks once and, on failure, enters the see state or wanders, then looks again. An ambush monster keeps the heard target and looks only when sight fails; any other shootable heard target sees immediately. At the see label, a goal whose reaction time is still ahead is cleared and makes no sound. A boss or `MF8_FULLVOLSEE` plays the see sound at full volume. The see state runs only when a target remains. `MF4_LOOKALLAROUND` is **0x00010000** and is the host's all-around bit.

| Managed (`VmPEnemyWake1149Slice.cs`) | Native (`p_enemy.cpp`) |
|----------------------------------------|----------------------|
| Player look and `A_Look` | **1778–2062** |
| Band hull (exclusive slice) | **1778–2062** |

### Chunk **1149** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **285** |
| **Net headline** | **+285** vs post-**1148** |


**Post-review ratio:** 273/285 managed/native ≈ 96% (src file LOC at HEAD; see tally).

## Tests

- `VmPEnemyWake1149SliceTests.LookForPlayers_ReturnsEarlyForFriendsAndThenWalksTheMask`
- `VmPEnemyWake1149SliceTests.Look_WakesFromAShootableHeardActorAndRefusesANoTargetPlayer`
- `VmPEnemyWake1149SliceTests.Look_AmbushAndFriendsUseTheSearchBeforeTheSeeState`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1149 --to 1149` (0 hard / 0 soft).

## Seams

TID, enemy, and monster searches, player masks, ambush sight, and the see sound stay host-supplied. The MBF friend loop is not compiled.

## Credit rationale

UNCredited **`p_enemy.cpp`** **1778–2062** (+285), inside the 3488-line file, with no conversion or recognition overlap.
