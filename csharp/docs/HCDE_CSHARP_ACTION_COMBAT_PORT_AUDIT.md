# Action combat port audit

## Scope

`p_actionfunctions.cpp` combat pointers: `A_CustomMeleeAttack`, `A_CustomComboAttack`, `A_BulletAttack`, `A_SpawnProjectile` (aim modes / pitch flags), `A_MonsterRefire`.

## Implementation

- `ActorCombatActions.cs`, `ActorRefireActions.cs`, `ActorActionDispatch.cs`
- `MonsterBrain` respects `SECF_NOATTACK` via `LevelSector.RuntimeFlags`

## Tests

- `ActionCombatLoadedMapTests` (8 cases) on UDMF two-room geometry with spawned monsters.

## Verified

- `dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green.
