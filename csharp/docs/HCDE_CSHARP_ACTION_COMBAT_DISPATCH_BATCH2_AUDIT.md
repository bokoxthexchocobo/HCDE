# Action combat dispatch batch 2 audit

## Scope

`p_actionfunctions.cpp` / ZScript attacks subset: `A_CustomMissile`, `A_CustomBulletAttack`, `A_Chase` entry, expanded `ActorActionDispatch` wiring for jump/inventory/explode names used in regression tests.

## Implementation

- `ActorCombatActions.CustomMissile`, `CustomBulletAttack` (+ `CustomBulletFlags`), `TryClassNameForProjectileType`
- `ActorActionDispatch.cs`, `ActorChaseActions.Chase`
- `ActionCombatLoadedMapTests` (+6 cases)

## Tests

Loaded-map UDMF two-room geometry; dispatch smoke for `A_CustomBulletAttack`, `A_CustomMissile`, `A_Chase`, `A_Look`.

## Verified

- `dotnet build -warnaserror`, focused action tests green; full `HCDE.Playsim.Tests` (17005).
