# Chunk 505 — AActor::Die part 1 (morph through kill/frag credit)

## Scope

AActor::Die part 1 (morph through kill/frag credit)

## Native ranges

- `src/playsim/p_interaction.cpp` **AActor::Die** 329–620 (292 lines)

**Credited native LOC:** 292 (tally `nativeLoc` 292).

## Managed

`VmPInteractionActorDieMorph.cs`, `VmPInteractionActorDieMorphTests.cs`
Managed LOC (tally): 149.
Conversion commit: `ff7ef8fa221df949f5466ce2ae3dacd4f143873e`.

## Tests

- `VmPInteractionActorDieMorphTests.Morph_KillDummyWhenAltPlayerNull`
- `VmPInteractionActorDieMorphTests.Missile_Explodes`
- `VmPInteractionActorDieMorphTests.Corpse_WhenMonster`
- `VmPInteractionActorDieMorphTests.Fraglimit_Hits`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 505 --to 505` (0 hard / 0 soft).

## Seams

`VMCall`, `P_ExplodeMissile`, `P_ActivateThingSpecial`, `StatusBar`, and `Level->ExitLevel` are not hosted.

## Credit rationale

Split **AActor::Die** at the player-death block (`if (player)` at line 622). Part 1 credits morph death handoff, inventory OwnerDied walk, missile explode, corpse flags, specials, killcount/frags/spree/multikill/fraglimit from `p_interaction.cpp` (329–620).
