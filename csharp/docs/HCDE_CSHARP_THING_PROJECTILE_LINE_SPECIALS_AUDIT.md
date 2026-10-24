# Thing projectile line specials 175 / 178 audit

## Scope

`p_lnspec.cpp` `LS_Thing_ProjectileIntercept` (175) and `LS_Thing_ProjectileAimed` (178); native `EV_Thing_Projectile` lead-target aiming.

## Implementation

- `ThingProjectileLineSpecials.cs`, `LineSpecials.cs` UDMF switch arms
- Spawnable-type resolution via `ActorCombatActions.TryClassNameForProjectileType`
- `ThingProjectileLineSpecialsTests` (2 cases)
- `line_special_gap.json` regenerated (175/178 wired; 1 poly gap remains)

## Tests

UDMF use lines with `PlayerUse`; imp spot TID + player dest TID; intercept vs aimed spawn.

## Verified

- `dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green.
