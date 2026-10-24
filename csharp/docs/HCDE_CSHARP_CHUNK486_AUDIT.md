# Chunk 486 — MorphPointerSubstitution completion and SpawnPlayer placement head

## Scope

MorphPointerSubstitution completion and SpawnPlayer placement head

## Native ranges

- `src/playsim/p_mobj.cpp` **MorphPointerSubstitution** 6191–6279 (89 lines)
- `src/playsim/p_mobj.cpp` **FLevelLocals::SpawnPlayer** 6314–6380 (67 lines)

**Credited native LOC:** 156 (tally `nativeLoc` 156).

## Managed

`VmPMobjMorphSpawn.cs`, `VmPMobjMorphSpawnTests.cs`
Managed LOC (tally): 95.
Conversion commit: `a659b30b9dbd517ffe5d24530d31a1d63865d15a`.

## Tests

- `VmPMobjMorphSpawnTests.Morph_UnmorphRunsPreUnmorph`
- `VmPMobjMorphSpawnTests.Morph_LinksDummyOnMorphIn`
- `VmPMobjMorphSpawnTests.Spawn_AllowsValidPlayer`
- `VmPMobjMorphSpawnTests.Spawn_SameSpotNeedsReborn`
- `VmPMobjMorphSpawnTests.SpawnZ_CeilingBeforeFloat`
- `VmPMobjMorphSpawnTests.Spawn_BadAngleCompatNudge`
- `VmPMobjMorphSpawnTests.Spawn_ApplyStartZOnFloor`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 486 --to 486` (0 hard / 0 soft).

## Seams

`VMCall` PreMorph/ObtainInventory, `Spawn`, and `P_FindFloorCeiling` are not hosted.

## Credit rationale

Credits morph unmorph hooks policy, morph link flags, and player spawn validation/safe-spot/Z/start-Z selection from `p_mobj.cpp` (6191–6380); avoids overlap with chunk36 pick-class and reborn tail.
