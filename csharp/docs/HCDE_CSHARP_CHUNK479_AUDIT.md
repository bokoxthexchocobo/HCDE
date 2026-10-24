# Chunk 479 — SplashCheck, UpdateWaterLevel, ConstructActor spawn setup

## Scope

SplashCheck, UpdateWaterLevel, ConstructActor spawn setup

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::SplashCheck** 5380–5392 (13 lines)
- `src/playsim/p_mobj.cpp` **AActor::PlayDiveOrSurfaceSounds** 5402–5409 (8 lines)
- `src/playsim/p_mobj.cpp` **AActor::UpdateWaterLevel** 5419–5445 (27 lines)
- `src/playsim/p_mobj.cpp` **ConstructActor** 5453–5527 (75 lines)
- `src/playsim/p_mobj.cpp` **ConstructActor** 5528–5628 (101 lines)

**Credited native LOC:** 224 (tally `nativeLoc` 224).

## Managed

`VmPMobjWaterSpawnConstruct.cs`, `VmPMobjWaterSpawnConstructTests.cs`
Managed LOC (tally): 153.
Conversion commit: `e797ed16bb6d7f5a4fac69d15e9abab8b3042fed`.

## Tests

- `VmPMobjWaterSpawnConstructTests.SplashCheck_BoomTransition`
- `VmPMobjWaterSpawnConstructTests.UpdateWaterLevel_RunsSplashWhenRequested`
- `VmPMobjWaterSpawnConstructTests.InstantReaction_ZeroesMonsterReaction`
- `VmPMobjWaterSpawnConstructTests.ResolveSpawnZ_FloatRandzUsesSpace`
- `VmPMobjWaterSpawnConstructTests.ConstructTail_StripsFriendlyOnNoAllies`
- `VmPMobjWaterSpawnConstructTests.ConstructTail_ForcesScrollerFlag`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 479 --to 479` (0 hard / 0 soft).

## Seams

`UpdateWaterDepth`, `P_HitWater`, `LinkToWorld`, and `CallBeginPlay` are not hosted.

## Credit rationale

Credits boom splash transition, player dive virtual hook, water-level refresh flow, and ConstructActor placement/state/Z/floorclip/counter policy from `p_mobj.cpp` (5380–5628).
