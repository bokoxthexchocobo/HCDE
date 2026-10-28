# p_mobj SpawnMapThing placement and SpawnPlayer origin — chunk 29 audit

**Native:** `p_mobj.cpp` `SpawnMapThing` 6882–6941; `SpawnPlayer` 6340–6378.

**Managed:** `MapThingActorSpawnComposer.cs`, `PlayerSpawnPlacementPolicy.cs`, tests.

Verified: `dotnet build -warnaserror`; full `dotnet test`.
