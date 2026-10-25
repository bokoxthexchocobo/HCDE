# Chunk 483 — PostBeginPlay, isFast/isSlow, Activate/Deactivate, OnDestroy (merged chunk 484) (merged chunk 484) (merged chunk 484) Retired (2026-10-24): `--band` hull overlaps existing credit ((multiple native files; --band hull undefined)).

## Scope

PostBeginPlay, isFast/isSlow, Activate/Deactivate, OnDestroy (merged chunk 484) (merged chunk 484) (merged chunk 484) Retired (2026-10-24): `--band` hull overlaps existing credit ((multiple native files; --band hull undefined)).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `cf6274ec1945252cf3e78f9db615eec4e18ffce0`.

## Padding review (2026-10-21)

| Managed | Native (p_map.cpp, p_mobj.cpp) |
|---------|-------------------|
| `PostBeginPlayFlags` / `ShouldRecreateLights` / `ApplyRecreateLightsFlag` / `ApplyLevelLightCreated` / `ActorPuffFlags` / `BuildDamageFlags` / `ForceDecalSpawn` / `DeepSplash` | `PostBeginPlay` 5825–5840 (5828: `flags7 |= MF7_HANDLENODELAY;`; 5829: `if (GetInfo()->LightAssociations.Size() || (state && state->Light > 0))`; 5831: `flags8 |= MF8_RECREATELIGHTS;`) |
| `IsFastMonster` | `isFast` 5842–5852 (5844: `if (flags5&MF5_ALWAYSFAST) return true;`; 5845: `if (flags5&MF5_NEVERFAST) return false;`; 5846: `return !!G_SkillProperty(SKILLP_FastMonsters);`) |
| `ActivateEligible` / `WakeFromDormant` / `ClearDormant` / `SetDormant` / `ActivateTicsWhenNoState` / `AlreadyDormant` / `EvaluateActivate` | `Activate` 5860–5897 (5862: `if ((flags3 & MF3_ISMONSTER) && (health > 0 || (flags & MF_ICECORPSE)))`; 5864: `if (flags2 & MF2_DORMANT)`; 5866: `flags2 &= ~MF2_DORMANT;`) |
| `DeactivateTicsWhenNoState` / `DeactivateEligible` / `EvaluateDeactivate` | `Deactivate` 5905–5942 (5907: `if ((flags3 & MF3_ISMONSTER) && (health > 0 || (flags & MF_ICECORPSE)))`; 5909: `if (!(flags2 & MF2_DORMANT))`; 5911: `flags2 |= MF2_DORMANT;`) |
| `ShouldDestroyPremorph` / `WorldThingDestroyedWhenLevel` / `OrphanSectorBlockmapFlags` / `DestroyHorizonPuff` / `SkyFloorDestroy` | `OnDestroy` 5950–5998 (5954: `if (alternative != nullptr && !(flags & MF_UNMORPHED))`; 5966: `if (Level != nullptr)`; 5986: `flags |= MF_NOSECTOR|MF_NOBLOCKMAP;`) |
| `AlwaysPuffSpawn` / `ShouldBleed` / `FallbackPuff` / `DamageForBleed` / `PoisonOnHit` / `HitWaterOnFloor` | `P_RailAttack` 5586–5610 (5597: `if ((hitactor->flags & MF_NOBLOOD) ||`; 5606: `if (!(puffDefaults->flags3 & MF3_BLOODLESSIMPACT))`) |
| `CanTalk` | `CanTalk` 5755–5758 (5757: `return self->Conversation != nullptr && self->health > 0 && !(self->f…`) |
| `HasConversation` | `HasConversation` 5766–5769 (5768: `return self->Conversation != nullptr;`) |

### Out-of-body uncredit (483)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **185** |
| **Out-of-body removed** | **6** |
| **Honest** | **179** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 483 --to 483` (0 hard / 0 soft).

## Seams

`VMCall`, `FindState`, `DestroyAllInventory`, and hash unlink are not hosted.

## Credit rationale

Credits post-spawn light flags, skill fast/slow queries, dormant activate/deactivate flows, and destroy premorph/event teardown from `p_mobj.cpp` (5825–5998).
