# p_map.cpp TryMove line-cross chunk 4 audit

## Scope

Native `src/playsim/p_map.cpp` `P_TryMove` spechit activation (**2752–2794**) on successful moves: collect crossed specials, dispatch `SPAC_Cross` / `SPAC_MCross` / `SPAC_PCross` / teleport parity via `MapMoveLineCrossings`, `LineSpecials.ActivateCrossLine`, and `ActorPhysics.TryMove`.

**Behavior native LOC credited:** 43.

## Tests

`TryMoveLineCrossTests` (player cross, monster cross, projectile without missile cross).

## Verified

`dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green.
