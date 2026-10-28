# Chunk 463 — A_Look and A_Wander

## Scope

A_Look and A_Wander

## Native ranges

- `src/playsim/p_enemy.cpp` **A_Look** 1944–2054 (111 lines)
- `src/playsim/p_enemy.cpp` **A_Wander** 2283–2322 (40 lines)

**Credited native LOC:** 151 (tally `nativeLoc` 151).

## Managed

`VmPEnemyLookPlayers.cs`
Managed LOC (tally): 336.
Conversion commit: `a5543a5137d0b691ce91e77dbd2413a0793bb9a2`.

## Tests

- `VmPEnemyLookPlayersTests.GoalReactionGate_BlocksEarly`
- `VmPEnemyLookPlayersTests.SinglePlayerDeadMonsters_Triggers`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 463 --to 463` (0 hard / 0 soft).

## Seams

`P_LookForPlayers` (recognition), `P_SmartMove`, and `SetState` are not hosted.

## Credit rationale

Credits look wake/seestate gating and wander direction/move from `p_enemy.cpp`.
