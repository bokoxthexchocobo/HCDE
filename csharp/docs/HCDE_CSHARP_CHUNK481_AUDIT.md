# Chunk 481 — StaticSpawn, SpawnClientSide, ClassForSpawn, LevelSpawned, HandleSpawnFlags, BeginPlay

## Scope

StaticSpawn, SpawnClientSide, ClassForSpawn, LevelSpawned, HandleSpawnFlags, BeginPlay

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::StaticSpawn** 5630–5662 (33 lines)
- `src/playsim/p_mobj.cpp` **AActor::Spawn** 5664–5695 (32 lines)
- `src/playsim/p_mobj.cpp` **ClassForSpawn** 5697–5709 (13 lines)
- `src/playsim/p_mobj.cpp` **AActor::LevelSpawned** 5711–5725 (15 lines)
- `src/playsim/p_mobj.cpp` **AActor::HandleSpawnFlags** 5727–5794 (68 lines)
- `src/playsim/p_mobj.cpp` **AActor::BeginPlay** 5796–5823 (28 lines)

**Credited native LOC:** 189 (tally `nativeLoc` 189).

## Managed

`VmPMobjStaticSpawn.cs`, `VmPMobjStaticSpawnTests.cs`
Managed LOC (tally): 108.
Conversion commit: `afccc52f635d317268bb17733e0e84b0351d33cd`.

## Tests

- `VmPMobjStaticSpawnTests.StaticSpawn_RejectsNullType`
- `VmPMobjStaticSpawnTests.ClientSideSpawn_RequiresClientClass`
- `VmPMobjStaticSpawnTests.LevelSpawned_RandomizesUnsyncedTics`
- `VmPMobjStaticSpawnTests.HandleSpawnFlags_FriendlyClearsCountKill`
- `VmPMobjStaticSpawnTests.HandleSpawnFlags_SecretIncrements`
- `VmPMobjStaticSpawnTests.BeginPlay_ClearsDormantWhenSet`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 481 --to 481` (0 hard / 0 soft).

## Seams

`ConstructActor`, `CreateThinker`, `VMCall`, and `CallDeactivate` are not hosted.

## Credit rationale

Credits static spawn validation/replacement, clientside spawn gate, class lookup, mapthing tic jitter, spawn-flag mutations, and BeginPlay dormant handling from `p_mobj.cpp` (5630–5823).
