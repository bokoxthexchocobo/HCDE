# Chunk 512 — P_FallingDamage, player_t::Serialize, P_IsPlayerTotallyFrozen Merged: [510]. Padding trimmed: 1682–1912 player-think band credited on chunk717 only. Retired (2026-10-24): `--band` hull overlaps existing credit (p-user-predict-client-chunk509 src/playsim/p_user.cpp 2021-2394).

## Scope

P_FallingDamage, player_t::Serialize, P_IsPlayerTotallyFrozen Merged: [510]. Padding trimmed: 1682–1912 player-think band credited on chunk717 only. Retired (2026-10-24): `--band` hull overlaps existing credit (p-user-predict-client-chunk509 src/playsim/p_user.cpp 2021-2394).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `ce12cbe376f9698b6c98bc70ec33206232b07252`.

## Padding review (2026-10-21)

| Managed | Native (`p_user.cpp`) |
|---------|-------------------|
| `FallingDamageLevelFlag` / `FallingDamageActorFlag` / `FallingDamageSectorBlocks` / `FallingDamageTelefrag` / `FallingDamageScalar` / `UseProperMonsterDamage` / `PickFallingDamage` / `FallingDamageRequiresMonsterLevel` / `ShouldApplyFallingDamage` / `ComputeFallingDamage` / `FallingDamagePositive` / `InfightingMonstersIgnore` / `InfightingFromMisc` / `ExplosionStyleValid` / `TryMiscBfgCells` / `TryMiscRocketStyle` / `FallingDamageAppliesToMonster` / `EvaluateMonsterFall` | `P_FallingDamage` 1586–1681 (1595: `if (damagestyle == 0)`; 1596: `return;`; 1598: `if (actor->floorsector->Flags & SECF_NOFALLINGDAMAGE)`) |
| `SerializePersistsCamera` / `SerializePersistsInventoryTics` / `SerializeFullbrightModeRoundTrip` / `SerializePspritesPersist` / `SerializeSafePosHook` / `ShouldClearOldButtons` / `ShouldRestoreFullbright` / `ShouldApplySkin` / `TitleLevelFreezes` / `PlayerMoFreeze` / `AnyFreezeReason` / `SerializeCmdRoundTrip` / `SerializePlayerStateField` / `RollbackTracksButtons` / `MaxHealthClamp` | `Serialize` 2476–2607 (2485: `if (!arc.IsRollback())`; 2491: `if (arc.isReading())`; 2589: `if (!arc.IsRollback())`) |
| `TotallyFrozenTitle` / `TotallyFrozenCheat` / `EvaluateTotallyFrozen` | `P_IsPlayerTotallyFrozen` 2609–2615 (2611: `return`) |

<!-- merged from chunk510 -->
| Managed | Native (p_enemy.cpp, p_user.cpp) |
|---------|-------------------|
| `UnpredictAlreadyDisabled` / `ClearPredictingCheatInRange` / `RestoreWeaponAfterRollback` / `MassacreSkipsDormant` / `MassacreRequiresMonster` / `MassacreFriendlyFilter` / `MassacreActorEligible` / `ClearPredictionLastTicFromBase` / `ResolveLastPredictedTic` / `SubtitleTick` / `ClearInterpCheats` / `BobWeapon2D` / `BobWeapon3D` / `RecordVelocityWhenNotPredicting` / `VelocityUsesFull3D` / `ClearInterpFlag` / `UseXYVelocity` / `EnvironmentTriggersFx` / `WeaponBobBranch` / `LevelVelocitySlotValid` / `StripPresentationCheats` | `P_UnPredictClient` 2396–2430 (2398: `if (!NetworkEntityManager::IsPredicting())`; 2406: `if (consoleplayer >= 0 && consoleplayer < MAXPLAYERS)`; 2408: `return;`) |
| `BossExitSpeckillMonsters` / `BossExitMap07Special` / `BossMap07Boss1` / `BossMap07Boss2` / `BossSamereplacement` / `BossNoExitDmflags` / `BossSpecActionsMask` / `TryPickBossSpecAction` | `A_BossDeath` 3314–3360 (3323: `if (type != mytype) flags8 |= ((AActor*)replacee->Defaults)->flags8;`; 3333: `if (type == sa->Type || mytype == sa->Type)`; 3335: `if (!checked && !CheckBossDeath(self))`) |
| Managed | Native (`p_user.cpp`) |
|---------|-------------------|
| `FallingDamageLevelFlag` / `FallingDamageActorFlag` / `FallingDamageSectorBlocks` / `FallingDamageTelefrag` / `FallingDamageScalar` / `UseProperMonsterDamage` / `PickFallingDamage` / `FallingDamageRequiresMonsterLevel` / `ShouldApplyFallingDamage` / `ComputeFallingDamage` / `FallingDamagePositive` / `InfightingMonstersIgnore` / `InfightingFromMisc` / `ExplosionStyleValid` / `TryMiscBfgCells` / `TryMiscRocketStyle` / `FallingDamageAppliesToMonster` / `EvaluateMonsterFall` | `P_FallingDamage` 1586–1681 (1595: `if (damagestyle == 0)`; 1596: `return;`; 1598: `if (actor->floorsector->Flags & SECF_NOFALLINGDAMAGE)`) |
| `SerializePersistsCamera` / `SerializePersistsInventoryTics` / `SerializeFullbrightModeRoundTrip` / `SerializePspritesPersist` / `SerializeSafePosHook` / `ShouldClearOldButtons` / `ShouldRestoreFullbright` / `ShouldApplySkin` / `TitleLevelFreezes` / `PlayerMoFreeze` / `AnyFreezeReason` / `SerializeCmdRoundTrip` / `SerializePlayerStateField` / `RollbackTracksButtons` / `MaxHealthClamp` | `Serialize` 2476–2607 (2485: `if (!arc.IsRollback())`; 2491: `if (arc.isReading())`; 2589: `if (!arc.IsRollback())`) |
| `TotallyFrozenTitle` / `TotallyFrozenCheat` / `EvaluateTotallyFrozen` | `P_IsPlayerTotallyFrozen` 2609–2615 (2611: `return`) |

<!-- merged from chunk510 -->
| Managed | Native (p_enemy.cpp, p_user.cpp) |
|---------|-------------------|
| `UnpredictAlreadyDisabled` / `ClearPredictingCheatInRange` / `RestoreWeaponAfterRollback` / `MassacreSkipsDormant` / `MassacreRequiresMonster` / `MassacreFriendlyFilter` / `MassacreActorEligible` / `ClearPredictionLastTicFromBase` / `ResolveLastPredictedTic` / `SubtitleTick` / `ClearInterpCheats` / `BobWeapon2D` / `BobWeapon3D` / `RecordVelocityWhenNotPredicting` / `VelocityUsesFull3D` / `ClearInterpFlag` / `UseXYVelocity` / `EnvironmentTriggersFx` / `WeaponBobBranch` / `LevelVelocitySlotValid` / `StripPresentationCheats` | `P_UnPredictClient` 2396–2430 (2398: `if (!NetworkEntityManager::IsPredicting())`; 2406: `if (consoleplayer >= 0 && consoleplayer < MAXPLAYERS)`; 2408: `return;`) |
| `BossExitSpeckillMonsters` / `BossExitMap07Special` / `BossMap07Boss1` / `BossMap07Boss2` / `BossSamereplacement` / `BossNoExitDmflags` / `BossSpecActionsMask` / `TryPickBossSpecAction` | `A_BossDeath` 3314–3360 (3323: `if (type != mytype) flags8 |= ((AActor*)replacee->Defaults)->flags8;`; 3333: `if (type == sa->Type || mytype == sa->Type)`; 3335: `if (!checked && !CheckBossDeath(self))`) |
| `ApplyAngleOffset` / `DecrementSubtitle` / `UpdateSafePosWhenLive` / `SkipBobTimerWhenPredicting` / `BotFrozenSkip` / `DebugLogTick` / `ApplyAngleOffsetValue` / `UseHoldButton` / `SafePosTracksLive` / `ShouldRecordVelocity` / `ShouldSkipThink` / `ProcessAngleSlot` | `P_PlayerThink` 1830–1880 (1834: `if (player->mo == NULL)`; 1836: `I_Error ("No player %td start\n", player - players + 1);`; 1841: `if (fabs(player->angleOffsetTargets[i].Degrees()) >= EQUAL_EPSILON)`) |

### Out-of-body uncredit (512)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **235** |
| **Out-of-body removed** | **7** |
| **Honest** | **228** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 512 --to 512` (0 hard / 0 soft).

## Seams

`FSerializer`, `ReadUserInfo`, `P_DamageMobj`, `LastSafePos::Serialize`, and `DEFINE_FIELD` blocks are not hosted.

## Credit rationale

Credits falling-damage policy, savegame player serialization branches, and total-freeze gating from `p_user.cpp` (1586–1681, 2476–2607, 2609–2615).
