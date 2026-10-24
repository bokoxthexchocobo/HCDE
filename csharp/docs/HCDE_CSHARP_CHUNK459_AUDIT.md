# Chunk 459 — ValidEnemyInBlock and P_LookFor* target scans

## Scope

ValidEnemyInBlock and P_LookFor* target scans

## Native ranges

- `src/playsim/p_enemy.cpp` **ValidEnemyInBlock** 1365–1488 (124 lines)
- `src/playsim/p_enemy.cpp` **P_LookForMonsters** 1489–1603 (115 lines)
- `src/playsim/p_enemy.cpp` **P_LookForTID** 1604–1736 (133 lines)
- `src/playsim/p_enemy.cpp` **P_LookForEnemies** 1737–1764 (28 lines)

**Credited native LOC:** 400 (tally `nativeLoc` 400).

## Managed

`VmPEnemyValidLook.cs`, `VmPEnemyValidLookTests.cs`
Managed LOC (tally): 201.
Conversion commit: `0bea479f8d62b8f5a03541a4bf84111623058f28`.

## Tests

- `VmPEnemyValidLookTests.ShootableAlive_RejectsDormant`
- `VmPEnemyValidLookTests.MbfSkipMutualFight_WhenHealthyFriend`
- `VmPEnemyValidLookTests.ValidEnemyPassesAll_RequiresVisible`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 459 --to 459` (0 hard / 0 soft).

## Seams

Blockmap iterators, `P_IsVisible`, and `pr_skiptarget` RNG are not hosted.

## Credit rationale

Credits friendly infight skips, dormant filters, TID/enemy/player look scans from `p_enemy.cpp`.
