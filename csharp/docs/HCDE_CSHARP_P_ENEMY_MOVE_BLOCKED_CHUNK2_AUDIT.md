# p_enemy.cpp move / blocked chunk 2 audit

## Scope

Native `src/playsim/p_enemy.cpp` / movement stack subset:

- `P_Move` blocked spechit door/push activation (`MF4_CANUSEWALLS`, `MF2_PUSHWALL`, `pr_opendoor` gate)
- `P_SmartMove` / `P_TryWalk` blocking-line reporting via `TryWanderMove`
- `MF6_JUMPDOWN` drop-off assist during `P_Move` steps (`LEVEL3_NOJUMPDOWN` on `PlayLevel`)

## Implementation

- `MonsterBlockedMoveActions`, `ActorWanderActions.MoveCore` integration
- Actor flags `CanUseWalls`, `PushWall`, `JumpDown`; `NextMonsterOpenDoorByte`, `NextDropoffByte`

## Tests

`MonsterBlockedMoveTests`; existing wander/chase/drop-off suites unchanged.

## Verified

`dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green (17,019).
