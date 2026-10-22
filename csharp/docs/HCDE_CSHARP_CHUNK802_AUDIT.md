# Chunk 802 — Whole `LookForEnemiesEx`, `A_Look`, and `A_Wander` (`p_enemy.cpp` 1451–1478, 1944–2054, 2283–2322).

## Scope

Whole `LookForEnemiesEx`, `A_Look`, and `A_Wander` (`p_enemy.cpp` 1451–1478, 1944–2054, 2283–2322).

## Native ranges

- `src/playsim/p_enemy.cpp` **AActor::LookForEnemiesEx** 1451–1478 (28 lines)
- `src/playsim/p_enemy.cpp` **AActor::A_Look** 1944–2054 (111 lines)
- `src/playsim/p_enemy.cpp` **A_Wander** 2283–2322 (40 lines)

**Credited native LOC:** 179 (tally `nativeLoc` 179).

## Managed

`VmPEnemyLookWander.cs`
Managed LOC (tally): 225.
Conversion commit: `2a2305500a0c3ed12781dd06b356982f2615544a`.

## Padding review (2026-10-21)

| Managed | Native (`p_enemy.cpp`) |
|---------|----------------------|
| `LookForEnemiesEx` | `AActor::LookForEnemiesEx` **1451–1478** |
| `Look` | `AActor::A_Look` **1944–2054** |
| `Wander` | `A_Wander` **2283–2322** |


**Post-review ratio:** 225/179 managed/native ≈ 126% (src file LOC at HEAD; see tally).

## Tests

- `VmPEnemyLookWanderTests.LookForEnemiesEx_UsesFriendlyseeblocksWhenRangeSentinel`
- `VmPEnemyLookWanderTests.Look_SkipsWhenInConversation`
- `VmPEnemyLookWanderTests.Look_AmbushEnemyRequiresSight`
- `VmPEnemyLookWanderTests.Wander_DecrementsReactionTime`
- `VmPEnemyLookWanderTests.Wander_CallsNewChaseDirWhenMovecountExpires`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 802 --to 802` (0 hard / 0 soft).

## Seams

Blockmap iterators, `ValidEnemyInBlock`, `P_LookForPlayers`, `P_CheckSight`, `SetState`, `S_Sound`, and `P_SmartMove` are injected.

## Credit rationale

Faithful whole-function look/wake glue after look-core chunk721; retires slice overlap on chunk469.
