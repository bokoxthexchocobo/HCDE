# p_map.cpp radius chunk 1 audit

## Scope

Native `src/playsim/p_map.cpp` `P_GetRadiusDamage`, `GetRadiusDamage`, `GetOldRadiusDamage`, and `P_RadiusAttack` flat-map subset:

- Modern square falloff with Z adjustment and circular option
- Old-radius Chebyshev falloff with per-actor sight
- Splash-group immunity, `DONTHARMCLASS` / `DONTHARMSPECIES` via `RadiusSourceImmune`
- `RadiusDamageFactor`, `SplashFactor`, rocket owner splash (`RADF_HURTSOURCE`)

## Implementation

- `MapRadiusAttack.cs` shared by `ActorExplodeActions` and `ProjectileActor` blast

## Tests

`MapRadiusAttackTests`; regression on `DontHarmClassTests`, `DontHarmSpeciesTests`, `CombatAiTests` rocket splash.

## Verified

`dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green.
