# Chunk 442 — DoSectorDamage filters and P_SectorDamage 3D-floor loop

## Scope

DoSectorDamage filters and P_SectorDamage 3D-floor loop

## Native ranges

- `src/playsim/p_spec.cpp` **DoSectorDamage** 509–531 (23 lines)
- `src/playsim/p_spec.cpp` **P_SectorDamage** 533–585 (53 lines)
- `src/playsim/p_spec.cpp` **showsecretsector cvars** 593–594 (2 lines)
- `src/playsim/p_spec.cpp` **P_GiveSecret** 596–627 (32 lines)
- `src/playsim/p_spec.cpp` **P_ActorOnSpecialFlat terrain damage** 645–675 (31 lines)
- `src/playsim/p_spec.cpp` **P_ActorInSpecialSector secret** 496–500 (5 lines)
- `src/playsim/p_spec.cpp` **DEFINE_ACTION_FUNCTION GiveSecret** 629–637 (9 lines)

**Credited native LOC:** 155 (tally `nativeLoc` 155).

## Managed

`VmPSpecSectorDamage.cs`, `VmPSpecSectorDamageTests.cs`
Managed LOC (tally): 78.
Conversion commit: `17e494cbdab7d1983356d49349a13972945fc025`.

## Tests

- `VmPSpecSectorDamageTests.SkipNonShootable`
- `VmPSpecSectorDamageTests.VavoomSwapHeights_Swaps`
- `VmPSpecSectorDamageTests.DamageInAir3dFloor_WhenFlagSet`
- `VmPSpecSectorDamageTests.PrintSecretSector`
- `VmPSpecSectorDamageTests.ShowSecretMessage_WhenLocal`
- `VmPSpecSectorDamageTests.TerrainDamageTick`
- `VmPSpecSectorDamageTests.PlaySplashWhenDamagePositive`
- `VmPSpecSectorDamageTests.ActorOnSecretSector`
- `VmPSpecSectorDamageTests.CountFoundSecret`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 442 --to 442` (0 hard / 0 soft).

## Seams

Sector tag iterators and `P_DamageMobj` are not hosted.

## Credit rationale

Credits shootable/player/air/protection gates, 3D-floor height swap, in-air damage policy, and terrain damage tick masks from `p_spec.cpp`.
