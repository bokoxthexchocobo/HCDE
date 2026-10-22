# Chunk 707 — Whole look cluster (`p_enemy.cpp` 1489–1777). Supersedes chunk503.

## Scope

Whole look cluster (`p_enemy.cpp` 1489–1777). Supersedes chunk503.

## Native ranges

- `src/playsim/p_enemy.cpp` **P_LookForMonsters** 1489–1531 (43 lines)
- `src/playsim/p_enemy.cpp` **LookForTIDInBlock** 1543–1594 (52 lines)
- `src/playsim/p_enemy.cpp` **P_LookForTID** 1604–1703 (100 lines)
- `src/playsim/p_enemy.cpp` **LookForEnemiesInBlock** 1714–1727 (14 lines)
- `src/playsim/p_enemy.cpp` **P_LookForEnemies** 1737–1777 (41 lines)

**Credited native LOC:** 250 (tally `nativeLoc` 250).

## Managed

`VmPEnemyLookTargets.cs`
Managed LOC (tally): 389.
Conversion commit: `9f1fdbae5a2c14f00a5244cc078d6b5009c69ad3`.

## Padding review (2026-10-21)

**Superseded chunk numbers:** 503.

| Managed | Native (`p_enemy.cpp`) |
|---------|----------------------|
| `LookForMonsters` | `P_LookForMonsters` 1489–1531 |
| `LookForTidInBlock` | `LookForTIDInBlock` 1543–1594 |
| `LookForTid` | `P_LookForTID` 1604–1703 |
| `LookForEnemiesInBlock` | `LookForEnemiesInBlock` 1714–1727 |
| `LookForEnemies` | `P_LookForEnemies` 1737–1777 |


**Post-review ratio:** 389/250 managed/native ≈ 156% (src file LOC at HEAD; see tally).

## Tests

- `VmPEnemyLookTargetsTests.LookForMonsters_WholeFunctionFindsTarget`
- `VmPEnemyLookTargetsTests.LookMonsters_SkipsOutOfRange`
- `VmPEnemyLookTargetsTests.TidBlock_MatchesDirectTid`
- `VmPEnemyLookTargetsTests.LookTid_FallbackGoal`
- `VmPEnemyLookTargetsTests.LookTid_BudgetExhausted`
- `VmPEnemyLookTargetsTests.LookForEnemiesInBlock_FindsValidLink`
- `VmPEnemyLookTargetsTests.LookForEnemies_UsesBlockSearchHit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 707 --to 707` (0 hard / 0 soft).

## Seams

`P_CheckSight`, `P_BlockmapSearch`, `P_IsVisible`, `GetActorIterator`, and `ValidEnemyInBlock` are injected.

## Credit rationale

Monster/TID/enemy look targets as whole functions.
