# Chunk 487 — SpawnMapThing unknown-type head, filters, and invasion/sound tail Retired (2026-10-24): `--band` hull overlaps existing credit (p-mobj-spawn-mapthing-chunk23 src/playsim/p_mobj.cpp 6664-6727).

## Scope

SpawnMapThing unknown-type head, filters, and invasion/sound tail Retired (2026-10-24): `--band` hull overlaps existing credit (p-mobj-spawn-mapthing-chunk23 src/playsim/p_mobj.cpp 6664-6727).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `6a345f15330de8930a76667b5f542249eab76bed`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `InvalidEdNum` / `ShouldWarnUnknownThing` / `SuppressFurtherUnknownWarnings` / `ExplicitNoSpawn` / `CopyEntryArgs` / `IsDeathmatchStart` / `IsPolySpawnSpecial` / `PlayerStartIndex` / `SpawnMultiSkill` / `ResolveThingMask` / `ThingPassesMask` / `SkillFilterPasses` / `SinglePlayerClassPasses` / `CoopClassMaskAllows` / `NormalizeSeqOverrideArg` / `SoundSequenceOverride` / `ApplyGeneralSpawnFilters` / `FoldCoopSpawnClass` / `CoopClassFilterRejects` / `BlockDmOnlyInCoop` / `IsCoopOnlyThing` / `BlockCoopOnlyThing` / `BlockCoopOnlyInventory` / `ApplyMapthingGravityZero` | `SpawnMapThing` 6569–6663 (6578: `if (mthing->EdNum == 0 || mthing->EdNum == -1)`; 6579: `return NULL;`; 6584: `if (mentry == NULL)`) |
| `SeqOverrideInRange` / `InvasionDelaySeconds` / `SharewareMissingFrames` / `BlockDeathmatchThing` / `BlockCoopWeaponSpawn` / `BlockMonsterSpawn` | `SpawnMapThing` 6768–6833 (6769: `if (mentry->Type == NULL && mentry->Special == SMT_SSeqOverride)`; 6772: `if (type == 255) type = -1;`; 6773: `if (type > 63)`) |

### Out-of-body uncredit (487)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **161** |
| **Out-of-body removed** | **0** |
| **Honest** | **161** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 487 --to 487` (0 hard / 0 soft).

## Seams

`Printf`, `Net_*` invasion hooks, `Spawn`, and `DoomEdMap` are not hosted.

## Credit rationale

Credits ednum/unknown-thing handling, game/skill mask gates, poly/deathmatch start routing, sound-sequence override, invasion pickup spots, and shareware frame fallback from `p_mobj.cpp` (6569–6663, 6768–6833).
