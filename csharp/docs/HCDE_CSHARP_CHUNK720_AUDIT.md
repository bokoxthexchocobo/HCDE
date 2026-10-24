# Chunk 720 — Whole `P_CheckMeleeRange`, `P_CheckMissileRange`, and `P_RandomChaseDir` (`p_enemy.cpp` 253–365, 1107–1257). `P_HitFriend` stays chunk508.

## Scope

Whole `P_CheckMeleeRange`, `P_CheckMissileRange`, and `P_RandomChaseDir` (`p_enemy.cpp` 253–365, 1107–1257). `P_HitFriend` stays chunk508.

## Native ranges

- `src/playsim/p_enemy.cpp` **P_CheckMeleeRange** 253–292 (40 lines)
- `src/playsim/p_enemy.cpp` **P_CheckMissileRange** 307–365 (59 lines)
- `src/playsim/p_enemy.cpp` **P_RandomChaseDir** 1107–1257 (151 lines)

**Credited native LOC:** 250 (tally `nativeLoc` 250).

## Managed

`VmPEnemyChaseRandom.cs`
Managed LOC (tally): 187.
Conversion commit: `561915316d570326890382aafbfd7451c73faa9f`.

## Padding review (2026-10-21)

| Managed | Native (`p_enemy.cpp`) |
|---------|----------------------|
| `CheckMeleeRange` | `P_CheckMeleeRange` 253–292 |
| `CheckMissileRange` | `P_CheckMissileRange` 307–365 |
| `RandomChaseDir` | `P_RandomChaseDir` 1107–1257 |


**Post-review ratio:** 187/250 managed/native ≈ 75% (src file LOC at HEAD; see tally).

## Tests

- `VmPEnemyChaseRandomTests.CheckMeleeRange_RejectsFriend`
- `VmPEnemyChaseRandomTests.CheckMissileRange_ClearsJustHitWhenFriendlyInfight`
- `VmPEnemyChaseRandomTests.RandomChaseDir_TurnsWhenWalkFails`
- `VmPEnemyChaseRandomTests.CheckMissileRange_BlocksDuringReactionTime`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 720 --to 720` (0 hard / 0 soft).

## Seams

`P_CheckSight`, player slot iteration, and `P_TryWalk` remain injected.

## Credit rationale

Melee/missile range gates and random friendly chase direction.
