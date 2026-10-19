# p_mobj SpawnMapThing spawn filters — chunk 23 audit

## Scope

Port native map-thing spawn eligibility (game-mode, skill, and class masks; coop/deathmatch late rules) and player-start registration from `FLevelLocals::SpawnMapThing` without actor spawning, DoomEd lookup, or invasion metadata.

**Native:** `p_mobj.cpp` `SpawnMapThing` 6664–6727, 6729–6760, 6834–6870.

**Managed:** `MapThingSpawnFlags.cs`, `MapThingSpawnFilter.cs`, `MapThingPlayerStartRegistrar.cs`, tests.

## Verified

`dotnet build -warnaserror`; full `dotnet test`.
