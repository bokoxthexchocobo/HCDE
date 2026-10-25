# Chunk 461 — P_BounceWall, P_BounceActor, P_ReflectOffActor

## Scope

P_BounceWall, P_BounceActor, P_ReflectOffActor

## Native ranges

- `src/playsim/p_map.cpp` **P_BounceWall** 3709–3713 (5 lines)
- `src/playsim/p_map.cpp` **P_BounceActor** 3722–3845 (124 lines)
- `src/playsim/p_map.cpp` **P_ReflectOffActor** 3847–3887 (41 lines)

**Credited native LOC:** 170 (tally `nativeLoc` 170).

## Managed

`VmPMapBounceReflect.cs`
Managed LOC (tally): 156.
Conversion commit: `13c265a858a009503b629060c5506f80b56bd698`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `AimReflect` | `P_ReflectOffActor` 3861 |
| `AllActorsBounce` | `P_BounceActor` 3746–3815 |
| `AssignYawAfterBounce` | `P_BounceWall` 3709–3713 |
| `AutoOffBounce` | `P_BounceActor` 3746–3815 |
| `AutoOffMaskClear` | `P_BounceWall` 3709–3713 |
| `BossRipExempt` | `P_BounceActor` 3746–3815 |
| `BounceAllOrReflective` / `BounceCallDie` / `BounceCountExplode` / `BounceFactorScale` | `P_BounceWall` 3709–3713 |
| `BounceNotOnShootablesFlag` | `P_BounceActor` 3746–3815 |
| `CreatureBounceState` / `DefaultSpecialBounce` / `DotProductBounce` / `EvaluateBounceActorBody` / `EvaluateBounceActorEarly` / `ExplodeWhenCountZero` / `HalfSpeedReflect` | `P_BounceWall` 3709–3713 |
| `HereticBounceDeathFlag` | `P_BounceActor` 3746–3815 |
| `HereticDeathOnTop` | `P_BounceActor` 3742–3849 |
| `KeepAngleFlag` | `P_BounceWall` 3709–3713 |
| `MbfBounceFlag` | `P_BounceActor` 3746–3815 |
| `MbfZBounce` | `P_BounceActor` 3742–3849 |
| `MissileReflectCombo` | `P_BounceActor` 3746–3815 |
| `MissileSpecialHitBlock` / `MissileSpecialHitPass` / `ModifyPitchAfterBounce` / `MonsterBounceEligible` | `P_BounceWall` 3709–3713 |
| `NonMonsterReflect` | `P_BounceActor` 3746–3815 |
| `NotOnShootablesExplode` / `RandomAngleJitter` / `ReflectAngleAdjust` | `P_BounceWall` 3709–3713 |
| `ReflectOffActor` | `P_ReflectOffActor` 3847 |
| `ReflectRequiresReflective` | `P_ReflectOffActor` 3847–3887 |
| `ResizedVelToTarget` / `RestVelocityMbf` | `P_BounceWall` 3709–3713 |
| `RipBounceOnUnrips` | `P_BounceActor` 3746–3815 |
| `RipperNoBounceOnShootable` | `P_BounceWall` 3709–3713 |
| `RipperSkipsShootable` | `P_BounceActor` 3722–3845 |
| `ShootableRipBlock` | `P_BounceActor` 3746–3815 |

### Out-of-body uncredit (461)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **170** |
| **Out-of-body removed** | **0** |
| **Honest** | **170** |


**Post-review ratio:** 156/170 managed/native ≈ 92% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapBounceReflectTests.ThrureflectPass_WhenReflective`
- `VmPMapBounceReflectTests.RipperSkipsShootable_WhenRip`
- `VmPMapBounceReflectTests.ReflectRequiresReflective`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 461 --to 461` (0 hard / 0 soft).

## Seams

`FSlide::BounceWall`, `P_ExplodeMissile`, `PlayBounceSound`, and `VelFromAngle` are not hosted.

## Credit rationale

Credits wall bounce delegate, actor bounce rip/reflect paths, and reflective deflection from `p_map.cpp`.
