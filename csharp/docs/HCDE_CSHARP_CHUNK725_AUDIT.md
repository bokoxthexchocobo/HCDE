# Chunk 725 — Whole static `P_ZMovement` (`p_mobj.cpp` 2955–3212).

## Scope

Whole static `P_ZMovement` (`p_mobj.cpp` 2955–3212).

## Native ranges

- `src/playsim/p_mobj.cpp` **P_ZMovement** 2955–3212 (258 lines)

**Credited native LOC:** 258 (tally `nativeLoc` 258).

## Managed

`VmPMobjZMovement.cs`
Managed LOC (tally): 294.
Conversion commit: `6799e8f28875f104c7d6f6d2f7de0b0c3c00de57`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `ZMovement` | `P_ZMovement` 2955–3212 |
| `HandleFloorClip` / `HandleCeilingClip` | floor/ceiling blocks 3046–3208 |
| `FloatTowardTarget` | float chase 2993–3003 |
| `SmoothStepUpApplies` / `MissileFloorExplode` | 2965–2968; missile floor 3061–3080 |

### Out-of-body uncredit (725)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **258** |
| **Out-of-body removed** | **0** |
| **Honest** | **258** |


**Post-review ratio:** 294/258 managed/native ≈ 114% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjZMovementTests.ZMovement_MissileHitsFloor_Explodes`
- `VmPMobjZMovementTests.ZMovement_SmoothStepUp_AdjustsView`
- `VmPMobjZMovementTests.MissileFloorExplode_TrueForPlainMissile`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 725 --to 725` (0 hard / 0 soft).

## Seams

`CallFallAndSink`, `FloorBounceMissile`, `P_HitFloor`, `P_ExplodeMissile`, `P_ProjectileHitPlane`, sector actions, `P_MonsterFallingDamage`, `P_DamageMobj`, `PlayerLandedOnThing`, `Crash`, and `P_CheckFakeFloorTriggers` are injected.

## Credit rationale

Vertical movement: float bob, floor/ceiling clip, missile explode, landing.
