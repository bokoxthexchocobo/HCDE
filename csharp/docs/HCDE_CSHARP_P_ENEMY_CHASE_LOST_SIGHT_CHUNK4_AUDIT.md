# p_enemy.cpp lost-sight retarget chunk 4 audit

## Scope

Native `A_DoChase` (**2694–2716**): when chase loses sight of the current target, temporarily clear `MF3_NOSIGHTCHECK` only when it was already set (`lookForBetter`), then restore after `P_LookForPlayers`.

**Behavior native LOC credited:** 23.

## Tests

`ChaseLostSightNoSightCheckTests.LostSightRetargetPreservesNoSightCheckForHateSearch`.

## Verified

`dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green.
