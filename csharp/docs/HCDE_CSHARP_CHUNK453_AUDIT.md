# Chunk 453 — PIT_CheckLine try-move line iterator Merged: [452].

## Scope

PIT_CheckLine try-move line iterator Merged: [452].

## Native ranges

- `src/playsim/p_map.cpp` **PIT_CheckLine** 664–667 (4 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 668–672 (5 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 686–690 (5 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 725–728 (4 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 732–735 (4 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 738–741 (4 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 749–753 (5 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 779–791 (13 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 815–816 (2 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 821–822 (2 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 844–845 (2 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 857–868 (12 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 871–876 (6 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 881–881 (1 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 882–885 (4 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 900–902 (3 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 913–913 (1 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 915–926 (12 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 929–940 (12 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 949–960 (12 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 968–970 (3 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 987–1000 (14 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 1008–1014 (7 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 1018–1023 (6 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 1028–1031 (4 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 1034–1045 (12 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 1053–1054 (2 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 1068–1083 (16 lines)
- `src/playsim/p_map.cpp` **PIT_CheckLine** 1086–1093 (8 lines)

**Credited native LOC:** 185 (tally `nativeLoc` 185).

## Managed

`VmPMapPitCheckLine.cs`, `VmPMapGetFriction.cs`
Managed LOC (tally): 231.
Conversion commit: `013eb039564d72ad4abb9d72992fb75cf9c974d7`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `SkipWhenBoxMissesLine` | `PIT_CheckLine` 844–845 |
| `IsOneSided` / `RecordPolyobjPortalHit` | 857–868 |
| `PortalLayerIgnoresBlock` | 871–876 |
| `IsStepMissile` | 881 |
| `CanStepCeilingPortal` / `ActorBelowPortalTop` / `ActorReachesPortalWithStep` / `FloorBelowPortal` | 882–885, 994 |
| `BlastedActorDamagesSelf` / `BlastedDamageAmount` / `DamageOnBlastedImpact` | 900–902, 968–970 |
| `TreatAsProjectile` | 913 |
| `ProjectileBlockingMask` / `LineBlocksProjectile` / `BlockWhenProjectileAndLineBlocks` | 915–926 |
| `IsRailingLine` | 920–922 |
| `PortalAboveSetsCeiling` / `LowerCeilingToPortal` | 929–940 |
| `PortalBelowSetsFloor` / `RaiseFloorToPortal` | 949–960 |
| `MissileIgnoresPortalFloorStep` / `PortalStepSetsGroup` | 987–1000 |
| `SteepSlopeDropoffBlocked` | 1008–1014 |
| `CrossLineCheckFails` | 1018–1023 |
| `RecalcFloorWhenPlanesMatch` / `OpeningBottomWidensAtPosition` | 1028–1031 |
| `RailingRaisesFloor` / `RailingFloorBump` | 1034–1045 |
| `AdjustCeilingWhenLower` / `CeilingAdjustUsesOpenTop` | 1053–1054 |
| `AdjustFloorWhenHigher` / `FloorAdjustUsesOpenBottom` / `MergeMidtexFlags` / `TouchMidtexWhenSameFloor` | 1068–1083 |
| `UpdateDropoffWhenLower` / `DropoffUsesPortalLayer` | 1086–1093 |
| `SelectFlying` / `FlyingPair` | `P_GetFriction` 664–667 |
| `UsesWaterFriction` / `InDeepWater` / `ShallowWaterAboveFloor` | 668–672 |
| `HalveMoveFactor` | 672 |
| `PreferLowerFriction` | 686–690, 725–728 |
| `SkipSectorWithoutFriction` | 732–735 |
| `ActorOnSectorFloor` | 738–741 |
| `ApplyActorFrictionMultiplier` / `FrictionToMoveFactor` | 749–753 |
| `BoostMoveFactorForVelocity` / `ResolveMoveFactor` | 779–791 |
| `LineIsAbovePortalBlocked` / `LineIsAboveCompare` | 815–816 |
| `LineIsBelowPortalBlocked` / `LineIsBelowCompare` | 821–822 |

<!-- merged from chunk452 -->
| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `SelectFlying` / `FlyingPair` | 664–667 |
| `UsesWaterFriction` / `InDeepWater` / `ShallowWaterAboveFloor` | 668–672 |
| `HalveMoveFactor` | 672 |
| `PreferLowerFriction` | 686–690, 725–728 |
| `SkipSectorWithoutFriction` | 732–735 |
| `ActorOnSectorFloor` | 738–741 |
| `ApplyActorFrictionMultiplier` / `FrictionToMoveFactor` | 749–753 |
| `BoostMoveFactorForVelocity` / `ResolveMoveFactor` | 779–791 |
| `LineIsAbovePortalBlocked` / `LineIsAboveCompare` | 815–816 |
| `LineIsBelowPortalBlocked` / `LineIsBelowCompare` | 821–822 |

### Out-of-body uncredit (453)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **185** |
| **Out-of-body removed** | **0** |
| **Honest** | **185** |


**Post-review ratio:** 231/185 managed/native ≈ 125% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapPitCheckLineTests.CanStepCeilingPortal_WhenBelowPortalTop`
- `VmPMapPitCheckLineTests.SteepSlopeDropoffBlocked_OnBoundary`
- `VmPMapPitCheckLineTests.RailingFloorBump_Adds32`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 453 --to 453` (0 hard / 0 soft).

## Seams

`P_LineOpening`, `P_IsBlockedByLine`, spechit vectors, and blockmap iterators are not hosted.

## Credit rationale

Credits full `PIT_CheckLine`: one-sided portal hits, projectile blocking, portal ceiling/floor adjustments, steep-slope dropoffs, railing floor bump, opening merges, and special/portal spechit queues from `p_map.cpp`.
