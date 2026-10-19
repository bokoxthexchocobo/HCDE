# Principal audit — line-special gap closure

**Date:** 2026-10-19  
**Scope:** Remaining `p_lnspec.cpp` handlers vs managed dispatch (`SectorEnvironmentLineSpecials`, `FloorCombinedLineSpecials`, `ThingSpawnLineSpecials`, `LineSpecialMisc`, `PolyLineSpecials`).  
**Result:** Accepted; 19 native non-NOP handlers remain (mostly thing/projectile variants and poly motion).

## Implemented

- Sector environment: translucency, sound, reflection, friction, push/current, glow, lightning, radius quake.
- Combined floor/ceiling movers, donut, transfer floors, texture-lowest.
- Thing move/spawn/projectile stubs, puzzle use, ACS locked execute, translucent line, teleport line, camera, automap, portal target, poly stop/OR stubs.

## Divergences

- Polyobject OR moves are acknowledged stubs until poly simulation exists.
- Thing spawn uses `Unknown{type}` class names; projectiles use simplified velocity.
- FraggleScript (`158`) returns false.

## Tests

`LineSpecialClosureTests` (3) + full Playsim suite green.

## LOC tally

`LOC tally: converted this run = 1,850 native lines (920 C# lines added); native remaining = 504,330`
