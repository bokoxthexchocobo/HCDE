# Chunk 721 — Whole `P_IsVisible`, `isTargetablePlayer`, `ValidEnemyInBlock`, and `P_LookForPlayers` (`p_enemy.cpp` 1268–1440, 1789–1933).

## Scope

Whole `P_IsVisible`, `isTargetablePlayer`, `ValidEnemyInBlock`, and `P_LookForPlayers` (`p_enemy.cpp` 1268–1440, 1789–1933).

## Native ranges

- `src/playsim/p_enemy.cpp` **P_IsVisible** 1268–1314 (47 lines)
- `src/playsim/p_enemy.cpp` **isTargetablePlayer** 1316–1363 (48 lines)
- `src/playsim/p_enemy.cpp` **ValidEnemyInBlock** 1365–1440 (76 lines)
- `src/playsim/p_enemy.cpp` **P_LookForPlayers** 1789–1933 (145 lines)

**Credited native LOC:** 316 (tally `nativeLoc` 316).

## Managed

`VmPEnemyLookCore.cs`
Managed LOC (tally): 246.
Conversion commit: `4ae2c600f29c1f1e8aa3a280d000b3a0f1b4702b`.

## Padding review (2026-10-21)

| Managed | Native (`p_enemy.cpp`) |
|---------|----------------------|
| `IsVisible` | `P_IsVisible` 1268–1314 |
| `IsTargetablePlayer` | `isTargetablePlayer` 1316–1363 |
| `ValidEnemyInBlock` | `ValidEnemyInBlock` 1365–1440 |
| `LookForPlayers` | `P_LookForPlayers` 1789–1933 |


**Post-review ratio:** 246/316 managed/native ≈ 78% (src file LOC at HEAD; see tally).

## Tests

- `VmPEnemyLookCoreTests.IsVisible_RejectsBeyondMaxDist`
- `VmPEnemyLookCoreTests.IsTargetablePlayer_RejectsFriend`
- `VmPEnemyLookCoreTests.ValidEnemyInBlock_RejectsNeverTarget`
- `VmPEnemyLookCoreTests.ValidEnemyInBlock_AllowsNonFriendlyKeepChecking`
- `VmPEnemyLookCoreTests.LookForPlayers_PicksGoalWhenScanExhausted`
- `VmPEnemyLookCoreTests.LookForPlayers_SetsTargetWhenPlayerTargetable`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 721 --to 721` (0 hard / 0 soft).

## Seams

`P_CheckSight`, blockmap iterators, `P_LookForTID`/`P_LookForEnemies`/`P_LookForMonsters`, and `A_Look` action glue remain native.

## Credit rationale

Core visibility and player scan drivers for monster look.
