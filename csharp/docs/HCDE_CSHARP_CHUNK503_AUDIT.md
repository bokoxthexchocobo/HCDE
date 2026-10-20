# Chunk 503 — P_LookForMonsters, LookForTIDInBlock, P_LookForTID, P_LookForEnemies

## Scope

P_LookForMonsters, LookForTIDInBlock, P_LookForTID, P_LookForEnemies

## Native ranges

- `src/playsim/p_enemy.cpp` **P_LookForMonsters** 1489–1531 (43 lines)
- `src/playsim/p_enemy.cpp` **LookForTIDInBlock** 1543–1594 (52 lines)
- `src/playsim/p_enemy.cpp` **P_LookForTID** 1604–1703 (100 lines)
- `src/playsim/p_enemy.cpp` **P_LookForEnemies** 1737–1777 (41 lines)

**Credited native LOC:** 236 (tally `nativeLoc` 236).

## Managed

`VmPEnemyLookTargets.cs`, `VmPEnemyLookTargetsTests.cs`
Managed LOC (tally): 215.
Conversion commit: `56f4c5fa747849f645963889bc5807715b5a8411`.

## Tests

- `VmPEnemyLookTargetsTests.LookMonsters_SkipsOutOfRange`
- `VmPEnemyLookTargetsTests.LookMonsters_FindsInRange`
- `VmPEnemyLookTargetsTests.TidBlock_MatchesDirectTid`
- `VmPEnemyLookTargetsTests.LookTid_FallbackGoal`
- `VmPEnemyLookTargetsTests.LookEnemies_BlockSearchHit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 503 --to 503` (0 hard / 0 soft).

## Seams

`P_CheckSight`, `P_BlockmapSearch`, `P_IsVisible`, `GetActorIterator`, and `IsFriend` are not hosted.

## Credit rationale

Credits monster look range/limit filters, TID blockmap hate matching, iterator budget and goal/last-enemy fallbacks, and friendly enemy search from `p_enemy.cpp` (1489–1777).
