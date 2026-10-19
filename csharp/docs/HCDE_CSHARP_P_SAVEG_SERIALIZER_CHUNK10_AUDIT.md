# p_saveg.cpp + ACS serializer — chunk 10 audit

## Scope

HCSV archive **366** appends an ACS runtime trailer: global/map/world variables, sparse world/global arrays, map arrays, global string pool, and suspended `DLevelScript` fibers (`SimAcsArchive` / `AcsVmPersistence`). Round-trip coverage exercises sector planes and terrains, line specials, player ammo with thinker order, and ACS globals with a suspended fiber (`PSavegRoundTripTests`).

**Conversion native LOC credited:** 1,512.

## Tests

- `PSavegRoundTripTests.SectorPlanesAndTerrainsRoundTrip`
- `PSavegRoundTripTests.LineActionsRoundTrip`
- `PSavegRoundTripTests.PlayerAmmoAndThinkerOrderRoundTrip`
- `PSavegRoundTripTests.AcsGlobalsMapWorldAndSuspendedFiberRoundTrip`
- `PSavegRoundTripTests.LegacyArchiveWithoutAcsTrailerPreservesZeroedVariables`

## Verified

`dotnet build -warnaserror`; `PSavegRoundTripTests` (5); full `HCDE.Playsim.Tests` after matrix sync.
