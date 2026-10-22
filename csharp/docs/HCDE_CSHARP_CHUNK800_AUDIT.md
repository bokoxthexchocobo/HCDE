# Chunk 800 — Whole `LookForEnemiesEx`, `A_Look`, and `A_Wander` (`p_enemy.cpp` 1451–1478, 1944–2054, 2283–2322).

## Scope

Whole `LookForEnemiesEx`, `A_Look`, and `A_Wander` (`p_enemy.cpp` 1451–1478, 1944–2054, 2283–2322).

## Native ranges

- `src/playsim/p_enemy.cpp` **AActor::LookForEnemiesEx** 1451–1478 (28 lines)
- `src/playsim/p_enemy.cpp` **AActor::A_Look** 1944–2054 (111 lines)
- `src/playsim/p_enemy.cpp` **A_Wander** 2283–2322 (40 lines)

**Credited native LOC:** 179 (tally `nativeLoc` 179).

## Managed

`VmPEnemyLookAction.cs`
Managed LOC (tally): 208.
Conversion commit: `96b6a1f0c1e3145366ecf6d5065146aa78dd70e4`.

## Padding review (2026-10-21)

| Managed | Native (`p_enemy.cpp`) |
|---------|----------------------|
| `LookForEnemiesEx` | `AActor::LookForEnemiesEx` **1451–1478** |
| `Look` | `AActor::A_Look` **1944–2054** |
| `Wander` | `A_Wander` **2283–2322** |


**Post-review ratio:** 208/179 managed/native ≈ 116% (src file LOC at HEAD; see tally).

## Tests

- `VmPEnemyLookActionTests.LookForEnemiesEx_CollectsValidEnemies`
- `VmPEnemyLookActionTests.Look_SkipsConversation`
- `VmPEnemyLookActionTests.Look_WakesWhenLookForPlayersSucceeds`
- `VmPEnemyLookActionTests.Wander_DecrementsReactionTime`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 800 --to 800` (0 hard / 0 soft).

## Seams

Blockmap iterators, `ValidEnemyInBlock`, `P_LookForPlayers`, `P_CheckSight`, `SetState`, `S_Sound`, and `P_SmartMove` are injected.

## Credit rationale

Completes look/wake action glue left after look-core/targets chunks 707/721.
