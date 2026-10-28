# Chunk 499 — P_DoCrunch blood tail, SpawnShootDecal, SpawnDeepSplash, P_ActivateThingSpecial Retired (2026-10-23): out-of-body credit removed; honest **149** < **150**.

## Scope

P_DoCrunch blood tail, SpawnShootDecal, SpawnDeepSplash, P_ActivateThingSpecial Retired (2026-10-23): out-of-body credit removed; honest **149** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `28e33c7c08a31544990a86380d2848740aec22af`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `RunDeactivateBranch` | `P_DoCrunch` 6679–6685 |
| `CrushDamageTick` / `SkipBloodWhenInvuln` / `CrushBleedDamage` / `ApplyBloodTranslation` / `PuffGetsOwner` / `HideBloodParticle` / `PlayCrushPain` / `MapTimeQuarter` | `P_DoCrunch` 6701–6752 (6704: `if ((cpos->crushchange > 0) && !(thing->Level->maptime & 3))`; 6709: `if (!(thing->flags2&(MF2_INVULNERABLE | MF2_DORMANT)))`; 6711: `if (!(thing->flags&MF_NOBLOOD))`) |
| `P_PushUp` | `P_PushUp` 6822–6830 |
| `P_PushDown` | `P_PushDown` 6886–6892 |
| `StateChangeWhenAlive` | `P_ChangeSector` 7107–7115 |
| `P_ChangeSector` | `P_ChangeSector` 7238–7238 |
| `AllowBloodSpawn` / `UseWeaponDecal` | `SpawnShootDecal` 7272–7303 (7283: `if (defaults->player != nullptr && defaults->player->ReadyWeapon != n…`; 7291: `if (decalbase != nullptr)`) |
| `DrawSplash2` / `DeepSplashHeightsec` | `SpawnDeepSplash` 7304–7318 (7307: `if (trace.Crossed3DWater)`; 7311: `else if (trace.CrossedWater && trace.CrossedWater->heightsec)`; 7315: `else return;`) |
| `ThingTargetsTrigger` / `TriggerTargetsThing` / `DefaultSwitchToActivate` / `RunActivateBranch` / `ToggleSwitchAfterActivate` / `ToggleSwitchAfterDeactivate` / `SpecialUsesThingAsActivator` / `ClearSpecialOnDeath` / `ClearSpecialOnSuccess` / `ShouldEnterStateMachine` / `AfterActivateSetsDeactivate` / `AfterDeactivateSetsActivate` / `HasThingSpecial` | `P_ActivateThingSpecial` 7319–7391 (7333: `if (thing->activationtype & THINGSPEC_ThingTargets) thing->target = t…`; 7334: `if (thing->activationtype & THINGSPEC_TriggerTargets) trigger->target…`; 7337: `if (!death && (thing->activationtype & (THINGSPEC_Activate | THINGSPE…`) |

### Out-of-body uncredit (499)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **205** |
| **Out-of-body removed** | **56** |
| **Honest** | **149** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 499 --to 499` (0 hard / 0 soft).

## Seams

`Spawn`, `P_DamageMobj`, `DImpactDecal::StaticCreate`, `P_HitWater`, and `P_ExecuteSpecial` are not hosted.

## Credit rationale

Credits crusher blood/splash policy, weapon-vs-actor decal selection, deep splash trace choice, and thing activation/switch/special dispatch from `p_map.cpp` (6679–7391) including EOF tail after `P_ChangeSector`.
