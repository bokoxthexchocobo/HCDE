# Chunk 576 — Range/inventory through wolf ballistics, warp success, RGF flags (consolidated chunk576–577,582–585)

## Scope

Range/inventory through wolf ballistics, warp success, RGF flags (consolidated chunk576–577,582–585)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **CheckRange** 1960–1960 (1 lines)
- `src/playsim/p_actionfunctions.cpp` **CheckRange** 1962–1977 (16 lines)
- `src/playsim/p_actionfunctions.cpp` **A_DropInventory** 1995–2001 (7 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetBlend** 2021–2026 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **A_CountdownArg** 2047–2048 (2 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Burst** 2081–2084 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Burst** 2094–2100 (7 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Burst** 2105–2107 (3 lines)
- `src/playsim/p_actionfunctions.cpp` **RS_Flags** 2132–2137 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Respawn** 2147–2149 (3 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Respawn** 2152–2157 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Respawn** 2164–2165 (2 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Respawn** 2169–2181 (13 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Respawn** 2184–2194 (11 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Weave** 3484–3525 (42 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Warp** 3689–3730 (42 lines)
- `src/playsim/p_actionfunctions.cpp` **A_LineEffect** 3549–3571 (23 lines)
- `src/playsim/p_actionfunctions.cpp` **A_WolfAttack** 3584–3602 (19 lines)
- `src/playsim/p_actionfunctions.cpp` **A_WolfAttack** 3603–3680 (78 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Warp** 3732–3747 (16 lines)
- `src/playsim/p_actionfunctions.cpp` **RadiusGiveFlags** 3774–3805 (32 lines)

**Credited native LOC:** 339 (tally `nativeLoc` 339).

## Managed

`VmActionRangeInventory.cs`, `VmActionRespawn.cs`, `VmActionWeave.cs`, `VmActionWarp.cs`, `VmActionLineEffect.cs`, `VmActionWolfAttackBallistics.cs`, `VmActionWarpSuccess.cs`, `VmActionRadiusGiveEligibility.cs`
Managed LOC (tally): 277.
Conversion commit: `1e0b968f1b292b03349b9d79b3093347c83a02e5`.

## Padding review (2026-10-21)

**Superseded chunk numbers:** 577, 582, 583, 584, 585 (retired; credit lives only in chunk576.json / tally row `action-range-through-wolf-chunk576`).

**Fidelity review (2026-10-21):** `BurstShardVelocityZ` is the faithful `mo->Vel.Z = 4 * (mo->Z() - self->Z()) / self->Height` at `A_Burst` **2105** (not a ratio-only helper). Excluded `Spawn`/`Destroy`/`A_BossDeath` (`2101`, `2114–2121`), `P_ExplodeMissile`/`P_DamageMobj`/`SetState` (`2050–2061`), `CreateThinker` (`2028–2030`).

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `SquaredRange` / `CheckRangeLoop` / `ShouldCheckCamera` | `CheckRange` 1960, 1962–1977 |
| `ShouldDropInventory` | `A_DropInventory` 1995–2001 |
| `NormalizeBlendColor` | `A_SetBlend` 2021–2026 |
| `CountdownArgInRange` / `DecrementCountdownArg` / `ShouldJumpWhenCountdownZero` | `A_CountdownArg` 2047–2048 |
| `BurstChunkNull` / `BurstChunkCount` / `BurstOffsetFromPr` / `BurstZFromPr` / `BurstShardVelocityZ` / `BurstVelocityFromPr` | `A_Burst` 2081–2084, 2094–2100, 2105–2107 |

**Fidelity review (2026-10-21):** Excluded `RestoreSpecialPosition` (`2150`), `P_TeleportMove`/`P_CheckPosition` (`2155–2159`), fog (`2196–2200`), kill count (`2201–2204`), failure path (`2206–2208`).

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `UseTelefragMove` / `ShouldSpawnFog` | `RS_Flags` / branch `2152–2157` |
| `ApplySolidFlag` / `RestoreDefaultHeight` / `RestoreDefaultRadius` | `A_Respawn` 2147–2149 |
| `ShouldClearTarget` / `ShouldClearSelfTarget` / `ClearTargetReference` | `A_Respawn` 2169–2181 |
| `MergeFriendlyFlags` / `CopyDefFlags3` / `CopyDefFlags4` / `ClearInvisibleRenderFlag` / `ApplyRespawnSuccess` | `A_Respawn` 2184–2194 |

**Fidelity review:** Credits `3491–3492`, `3495–3496`, `3502–3515`, `3518–3523`. `DEFINE_ACTION_FUNCTION` wrapper is a seam.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `WeaveIndexXY` / `WeaveIndexZ` | 3491–3492 |
| `ShouldWeaveXY` / `ShouldWeaveZ` | 3495–3496, 3518–3519 |
| `AdvanceWeaveIndex` | 3499–3500, 3521–3522 |
| `UseTryMoveWhenInteractive` | 3502–3515 |

**Fidelity review:** Credits `3707–3715`, `3717–3724`, `3727–3730`. Offset math and warp move are seams.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `PrimeWarpChainResult` / `ClearWarpStatePointer` | 3707–3715 |
| `UseTidLookup` | 3717–3724 |
| `WarpFailsWithoutReference` | 3727–3730 |

**Fidelity review:** `A_LineEffect` credits `3558–3567`. `A_WolfAttack` credits `3597–3602`. PARAM/junk setup lines are seams.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `SkipWhenLinedone` / `MarkLinedoneWhenNonRepeating` | `A_LineEffect` 3558–3567 |
| `WolfAttackAbortsWithoutTarget` / `WolfAttackAbortsWithoutSight` | `A_WolfAttack` 3597–3602 |

**Fidelity review (chunk585 merge):** Wolf ballistics `3603–3680`; warp success `3732–3747`; `RadiusGiveFlags` `3774–3805` only (`DoRadiusGive` on `action-explode-radius-give`).

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `VmActionWolfAttackBallistics` | `A_WolfAttack` 3603–3680 |
| `VmActionWarpSuccess` | `A_Warp` 3732–3747 |
| `VmActionRadiusGiveEligibility` | `RadiusGiveFlags` 3774–3805 |

**Post-review ratio:** 277/339 managed/native ≈ 82% (src file LOC at HEAD; see tally).

## Tests

- `VmActionRangeInventoryTests.SquaredRange_Multiplies`
- `VmActionRangeInventoryTests.CheckRangeLoop_ReturnsTrueWhenAnyPlayerInRange`
- `VmActionRangeInventoryTests.ShouldCheckCamera_WhenNonPlayerCamera`
- `VmActionRangeInventoryTests.ShouldDropInventory_WhenFound`
- `VmActionRangeInventoryTests.NormalizeBlendColor_ClearsWhite`
- `VmActionRangeInventoryTests.CountdownArgInRange_RejectsFive`
- `VmActionRangeInventoryTests.DecrementCountdownArg_SubtractsOne`
- `VmActionRangeInventoryTests.BurstChunkNull`
- `VmActionRangeInventoryTests.BurstChunkCount_ScalesWithRadius`
- `VmActionRangeInventoryTests.BurstOffsetFromPr_ScalesRadius`
- `VmActionRangeInventoryTests.BurstZFromPr_ScalesHeight`
- `VmActionRangeInventoryTests.BurstShardVelocityZ_MatchesNative2105`
- `VmActionRangeInventoryTests.BurstVelocityFromPr_ScalesRandom2`
- `VmActionRespawnTests.UseTelefragMove_WhenFlagSet`
- `VmActionRespawnTests.ShouldSpawnFog_WhenFlagSet`
- `VmActionRespawnTests.ApplySolidFlag_SetsSolid`
- `VmActionRespawnTests.RestoreDefaultHeight_Passthrough`
- `VmActionRespawnTests.ApplyRespawnSuccess_RestoresHealthWhenOk`
- `VmActionRespawnTests.ShouldClearTarget_WhenKeepTargetUnset`
- `VmActionRespawnTests.ShouldClearSelfTarget_WhenSameReference`
- `VmActionRespawnTests.MergeFriendlyFlags_PreservesFriendlyBit`
- `VmActionRespawnTests.ApplyRespawnSuccess_ClearsInvisible`
- `VmActionWeaveTests.WeaveIndexXY_MasksToSixBits`
- `VmActionWeaveTests.ShouldWeaveXY_WhenDistAndSpeedNonZero`
- `VmActionWeaveTests.AdvanceWeaveIndex_WrapsAt64`
- `VmActionWeaveTests.UseTryMoveWhenInteractive_WithoutNointeraction`
- `VmActionWarpTests.WarpFailsWithoutReference`
- `VmActionWarpTests.UseTidLookup_WhenFlagged`
- `VmActionWarpTests.ShouldPrimeChainResult_WhenSlotPresent`
- `VmActionLineEffectTests.SkipWhenLinedone_IsSet`
- `VmActionLineEffectTests.MarkLinedoneWhenNonRepeating`
- `VmActionLineEffectTests.WolfAttackAbortsWithoutTarget`
- `VmActionWolfAttackBallisticsTests.HitchanceFromSpeed_Uses256WhenSlow`
- `VmActionWolfAttackBallisticsTests.AdjustHitchanceForDistance_SubtractsDodgePenalty`
- `VmActionWolfAttackBallisticsTests.DamageFromFlags_UsesRandomWhenAllowed`
- `VmActionWarpSuccessTests.ShouldSetSuccessStatePointer_WhenWarpOk`
- `VmActionWarpSuccessTests.ShouldSetChainSuccess_WhenNoStateJump`
- `VmActionRadiusGiveEligibilityTests.FilterSpeciesCombined_EitherMode`
- `VmActionRadiusGiveEligibilityTests.OutsideCube_WhenAxisBeyondDistance`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 576 --to 576` (0 hard / 0 soft).

## Seams

`DoCheckSightOrRange`/`P_CheckSight` in player loops, `DropInventory`, `CreateThinker`, explode/damage/state jumps, `Spawn`, boss/unblock/destroy. `P_TeleportMove`, `P_CheckPosition`, `RestoreSpecialPosition`, `P_SpawnTeleportFog`, monster counters, and failed-path solid clear. `BobSin`, `P_TryMove`, `UnlinkFromWorld`, and `SetXY` are not hosted. `COPY_AAPTR`, `SingleActorFromTID`, and `P_Thing_Warp` are not hosted. `P_ExecuteSpecial`, `P_CheckSight`, `A_FaceTarget`, and hitscan damage are not hosted.

## Credit rationale

Squared range, inventory/blend guards, countdown bounds, and burst PR/velocity formulas are ported; engine calls are seams. Flag constants and target/flag merge/clear decisions are ported; movement and fog are seams. Credits weave index masking, non-zero dist/speed gates, index advance, and MF5_NOINTERACTION branch selection. Credits dual return priming, TID selector branch flag, and null reference early exit. Credits one-shot linedone merge and wolf attack early exits before facing/damage.
