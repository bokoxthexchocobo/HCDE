# p_enemy.cpp move / blocked chunk 2 audit

## Scope

Native `src/playsim/p_enemy.cpp` `P_Move` subset (**22** lines behavior-credited):

- **518–524** `MF6_JUMPDOWN` drop-off assist (`LEVEL3_NOJUMPDOWN` on `PlayLevel`)
- **697–710** blocked spechit door/push activation (`MF4_CANUSEWALLS`, `MF2_PUSHWALL`, `pr_opendoor`)

`P_SmartMove` / `P_TryWalk` blocking-line reporting lives in `ActorWanderActions` / `ActorPhysics.TryWanderMove` and is credited under `p-enemy-chase-core-chunk3`.

## Implementation

- `MonsterBlockedMoveActions`, `ActorWanderActions.MoveCore` integration
- Actor flags `CanUseWalls`, `PushWall`, `JumpDown`; `NextMonsterOpenDoorByte`, `NextDropoffByte`

## Tests

`MonsterBlockedMoveTests`; existing wander/chase/drop-off suites unchanged.

## Verified

`dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green (17,019).
