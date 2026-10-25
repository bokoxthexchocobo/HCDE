# Chunk 486 — MorphPointerSubstitution completion and SpawnPlayer head/tail Retired (2026-10-24): `--band` hull overlaps existing credit (p-mobj-spawn-compose-chunk29 src/playsim/p_mobj.cpp 6340-6378).

## Scope

MorphPointerSubstitution completion and SpawnPlayer head/tail Retired (2026-10-24): `--band` hull overlaps existing credit (p-mobj-spawn-compose-chunk29 src/playsim/p_mobj.cpp 6340-6378).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `64482482fd6584c470287122f40532c94e442b57`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `ClearUnmorphedOnUnmorph` / `SetUnmorphedDummyLink` / `RedirectCamera` / `LinkMorphPartners` / `ApplyUnmorphFlags` / `SubstitutePlayerPointers` | `MorphPointerSubstitution` 6191–6279 (6225: `if (!from->Level->PlayerInGame(i))`; 6230: `if (p->mo == from)`; 6232: `if (p->poisoner == from)`) |
| `TransferPlayerBody` / `SpawnPlayerAllowed` | `SpawnPlayer` 6314–6339 (6322: `if (mthing == NULL)`; 6324: `return NULL;`; 6327: `if ((unsigned)playernum >= (unsigned)MAXPLAYERS || !PlayerInGame(play…`) |
| `ApplySkinSprite` / `EnableChasecam` / `SetupPspritesEligible` / `FilterCoopRespawnInventory` / `DestroyLeftoverInventory` / `CallOnRespawn` / `AttachStatusBar` / `SinkBelowCeiling` / `SinkZ` / `RunEnterScripts` / `RunRebornScripts` / `ScriptEligible` / `HeldWeaponUsable` / `PspriteWeaponFullyUp` / `StopOldBehaviors` / `ClearBehaviorsOnDestroy` / `ClearDummyUnmorphFlag` / `PassHeldWeaponToCoopFilter` / `PassNullHeldWeapon` / `ClearActorLastHeard` / `ClearSectorTargets` / `FireRespawnEvent` / `AttachBarForConsole` / `SkipDestroyWhenCoopFiltered` | `SpawnPlayer` 6423–6560 (6460: `if (PlayerInGame(ii) && Players[ii]->camera == oldactor)`; 6467: `if ((demoplayback || demonew) && chasedemo)`; 6471: `if (!(flags & SPF_TEMPPLAYER) || oldactor == nullptr)`) |

### Out-of-body uncredit (486)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **253** |
| **Out-of-body removed** | **0** |
| **Honest** | **253** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 486 --to 486` (0 hard / 0 soft).

## Seams

`VMCall`, `Spawn`, `DestroyAllInventory`, `Behaviors`, and `PlayerPointerSubstitution` are not hosted.

## Credit rationale

Credits morph unmorph hooks policy, spawn gate/lerp head, and player spawn state/script tail from `p_mobj.cpp` (6191–6339, 6423–6560); skips ranges credited in chunks 29/36.
