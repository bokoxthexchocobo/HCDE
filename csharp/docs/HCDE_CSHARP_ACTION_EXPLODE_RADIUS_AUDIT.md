# Action explode / radius audit

## Scope

ZScript `A_Explode`, `A_Detonate`, `A_RadiusThrust`, `A_RadiusGive`; native radius splash-group immunity (`AActor::SplashImmune` / `p_mobj.cpp`).

## Implementation

- `ActorExplodeActions.cs`, `ActorRadiusGiveActions.cs`
- `ActorActionDispatch` hooks for explode/thrust/give
- `ActionExplodeLoadedMapTests` (5 cases)

## Tests

Damage, thrust velocity, splash-group block, radius give stimpack, dispatch invoke.

## Verified

- `dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green.
