# Chunk 508 — NoiseMarkSector, P_RecursiveSound, P_NoiseAlert, P_HitFriend, P_IsOnLift, P_IsUnderDamage, P_CheckTags Retired (2026-10-24): `--band` hull overlaps existing credit (p-enemy-chase-range-chunk720 src/playsim/p_enemy.cpp 253-292).

## Scope

NoiseMarkSector, P_RecursiveSound, P_NoiseAlert, P_HitFriend, P_IsOnLift, P_IsUnderDamage, P_CheckTags Retired (2026-10-24): `--band` hull overlaps existing credit (p-enemy-chase-range-chunk720 src/playsim/p_enemy.cpp 253-292).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `26f8f9fe712b96c40ffaa6fd36c139567b6b0028`.

## Padding review (2026-10-21)

| Managed | Native (`p_enemy.cpp`) |
|---------|-------------------|
| `NoiseSectorAlreadyFlooded` / `NoiseWithinMaxDistance` / `NoisePortalTraverse` / `NoiseLineTwoSided` / `HarmMonsterInSector` / `ShouldSkipNoiseFlood` | `NoiseMarkSector` 122–145 (125: `if (sec->validcount == validcount`; 128: `return; // already flooded`; 138: `if (actor != soundtarget && (!splash || !(actor->flags4 & MF4_NOSPLAS…`) |
| `NoiseSoundBlockLine` | `P_RecursiveSound` 147–219 (156: `if (checkabove)`; 161: `if (checkbelow)`; 169: `if (port && (port->mFlags & PORTF_SOUNDTRAVERSE))`) |
| `NoiseAlertNoTarget` / `NoiseSplashActorAlert` | `P_NoiseAlert` 230–245 (232: `if (emitter == NULL)`; 233: `return;`; 235: `if (target != NULL && target->player && (target->player->cheats & CF_…`) |
| `HitFriendRequiresFriendly` / `HitFriendBlocksShot` | `P_HitFriend` 384–399 (388: `if (self->flags&MF_FRIENDLY && self->target != NULL)`; 393: `if (t.linetarget != NULL && t.linetarget != self->target)`; 395: `return self->IsFriend (t.linetarget);`) |
| `OnLiftActivePlat` / `OnLiftSectorFlag` | `P_IsOnLift` 412–426 (418: `if (e && e->IsKindOf(RUNTIME_CLASS(DPlat)))`; 419: `return true;`; 425: `return !!(sec->MoreFlags & SECMF_LIFT);`) |
| `UnderDamageCrushDirection` / `SectorDamageHarmInAir` / `SectorDamageHurtMonsters` / `SkipSectorDamage` / `EvaluateUnderDamage` | `P_IsUnderDamage` 438–460 (445: `if (e && e->IsKindOf(RUNTIME_CLASS(DCeiling)))`; 448: `if (cl->getCrush() > 0) // unlike MBF we need to consider non-crushin…`; 453: `if (!(actor->flags9 & MF9_NOSECTORDAMAGE) && seclist->m_sector->damag…`) |
| `CheckTagsSameSector` / `CheckTagsPrimaryMatch` | `P_CheckTags` 467–474 (470: `if (!Level->SectorHasTags(sec1) || !Level->SectorHasTags(sec2)) retur…`; 471: `if (Level->GetFirstSectorTag(sec1) == Level->GetFirstSectorTag(sec2))…`; 473: `return false;`) |

### Out-of-body uncredit (508)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **175** |
| **Out-of-body removed** | **0** |
| **Honest** | **175** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 508 --to 508` (0 hard / 0 soft).

## Seams

`validcount`, `P_AimLineAttack`, sector portal displacement, thinker iterators, and `DSectorEffect` types are not hosted.

## Credit rationale

Credits sound-alert flooding, recursive sector traversal gates, friendly fire line tests, lift/damage AI probes, and sector tag matching from `p_enemy.cpp` (122–245, 384–474).
