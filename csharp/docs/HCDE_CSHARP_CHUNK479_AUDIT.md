# Chunk 479 — SplashCheck, UpdateWaterLevel, ConstructActor spawn setup Retired (2026-10-24): `--band` hull overlaps existing credit (p-mobj-bounce-seek-matrix-water-chunk753 src/playsim/p_mobj.cpp 5402-5409).

## Scope

SplashCheck, UpdateWaterLevel, ConstructActor spawn setup Retired (2026-10-24): `--band` hull overlaps existing credit (p-mobj-bounce-seek-matrix-water-chunk753 src/playsim/p_mobj.cpp 5402-5409).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `e1df9edf3c47542e37d373ece255be8d822adafc`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `AssignFloatBobPhase` / `CallBeginPlayAfterConstruct` / `ClearFriendlyFlag` / `DelayFloorFind` / `EuthanizeAfterBeginPlay` | `AActor::SplashCheck` 5380–5392 |
| `EvaluateConstructTail` | `ConstructActor` 5528–5628 |
| `FastMonstersSpeed` / `FloorClipOrZero` | `AActor::SplashCheck` 5380–5392 |
| `ForceScrollerCheckFlag` / `IncrementItemCount` / `IncrementSecretCount` | `ConstructActor` 5501–5627 |
| `InstantReactionApplies` | `ConstructActor` 5462–5589 |
| `IsMonster` | `ConstructActor` 5482–5485 |
| `MonsterReactionTime` | `ConstructActor` 5462–5589 |
| `NointeractionFloorCache` | `ConstructActor` 5574–5577 |
| `PickSpawnRng` | `ConstructActor` 5462–5589 |
| `PlayDiveSoundsForPlayer` / `PuffTypeLookupFailed` | `AActor::SplashCheck` 5380–5392 |
| `ResolveSpawnZ` | `ConstructActor` 5453–5527 |
| `RunSplashCheck` / `ShouldSplashOnBoomTransition` / `StripFriendlyOnNoAllies` / `UseFloorClip` | `AActor::SplashCheck` 5380–5392 |
| `ZeroGravitySetsFlag` | `ConstructActor` 5462–5589 |

### Out-of-body uncredit (479)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **189** |
| **Out-of-body removed** | **0** |
| **Honest** | **189** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 479 --to 479` (0 hard / 0 soft).

## Seams

`UpdateWaterDepth`, `P_HitWater`, `LinkToWorld`, and `CallBeginPlay` are not hosted.

## Credit rationale

Credits boom splash transition, player dive virtual hook, water-level refresh flow, and ConstructActor placement/state/Z/floorclip/counter policy from `p_mobj.cpp` (5380–5628).
