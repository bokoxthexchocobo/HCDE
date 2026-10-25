# Chunk 576 — Range/inventory through wolf ballistics, warp success, RGF flags (consolidated chunk576–577,582–585) Merged: [578]. Merged: [573]. Retired (2026-10-24): `--band` hull overlaps existing credit (p-action-teleport-chunk526 src/playsim/p_actionfunctions.cpp 3255-3422).

## Scope

Range/inventory through wolf ballistics, warp success, RGF flags (consolidated chunk576–577,582–585) Merged: [578]. Merged: [573]. Retired (2026-10-24): `--band` hull overlaps existing credit (p-action-teleport-chunk526 src/playsim/p_actionfunctions.cpp 3255-3422).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `26b567523e00bb6d23dee5b1bf05e47e299862c1`.

## Padding review (2026-10-21)

### Manifest padding fix (2026-10-23)

| Removed from `managedSourceFiles` | Reason |
|-----------------------------------|--------|
| `VmActionPrintLogRecoil.cs` / `VmActionFadeSpawn.cs` / `VmActionSightParticle.cs` | **No** lines in **576** `nativeRanges` (**886** retire); off-tally test helpers only — must not inflate `managedLoc` |
| `VmActionJumpRadiusRail.cs` | **No** lines in **576** `nativeRanges` (merged **572** / retired **886**); off-tally only — manifest padding removed **2026-10-23** |

**Gross vs net (576 `managedLoc` hygiene):** tally **711** → **511** → **408** after VmAction manifest removals; **268** native unchanged; ratio **~152%** (floor **134**).

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

<!-- merged from chunk578 -->
**Superseded chunk numbers:** 580 (retired; credit lives only in chunk578.json / tally row `player-skin-userfield-chunk578`).

**Fidelity review:** `PlayerSkinCheck` excludes `Skins[...]` (`2224–2225`). `CheckLOFTraceFunc` credits `2286–2356` (full callback decision tree). `CLOF_flags` enum credited as constants used by the trace helper.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `PlayerSkinIsOtherGame` | `PlayerSkinCheck` 2224–2225 |
| `EvaluateHitActor` | `CheckLOFTraceFunc` 2291–2355 |

**Fidelity review:** `SetViewRollNative` credits `3099–3102`. `GetVar`/`GetArrayVar` credit rejection branches. Setter credits mirror `if (var != nullptr)` blocks in `A_SetUserVar*` / `A_SetUserArray*`.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `SetViewRollOnRef` | `SetViewRollNative` 3099–3102 |
| `IsUserScalarField` | `GetVar` 3127–3133 |
| `IsUserArrayField` / `ArrayIndexInBounds` | `GetArrayVar` 3176–3188 |
| `ShouldSetUserScalar` | `A_SetUserVar` / `A_SetUserVarFloat` 3144–3147, 3159–3162 |
| `ShouldSetUserArrayElement` | `A_SetUserArray` / `A_SetUserArrayFloat` 3201–3205, 3218–3222 |

### Out-of-body uncredit (576)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **268** |
| **Out-of-body removed** | **32** |
| **Honest** | **236** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 576 --to 576` (0 hard / 0 soft).

## Seams

`DoCheckSightOrRange`/`P_CheckSight` in player loops, `DropInventory`, `CreateThinker`, explode/damage/state jumps, `Spawn`, boss/unblock/destroy. `P_TeleportMove`, `P_CheckPosition`, `RestoreSpecialPosition`, `P_SpawnTeleportFog`, monster counters, and failed-path solid clear. `BobSin`, `P_TryMove`, `UnlinkFromWorld`, and `SetXY` are not hosted. `COPY_AAPTR`, `SingleActorFromTID`, and `P_Thing_Warp` are not hosted. `P_ExecuteSpecial`, `P_CheckSight`, `A_FaceTarget`, and hitscan damage are not hosted.

## Credit rationale

Squared range, inventory/blend guards, countdown bounds, and burst PR/velocity formulas are ported; engine calls are seams. Flag constants and target/flag merge/clear decisions are ported; movement and fog are seams. Credits weave index masking, non-zero dist/speed gates, index advance, and MF5_NOINTERACTION branch selection. Credits dual return priming, TID selector branch flag, and null reference early exit. Credits one-shot linedone merge and wolf attack early exits before facing/damage.
