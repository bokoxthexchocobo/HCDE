# p_enemy.cpp chunk 1 audit

## Scope

Native `src/playsim/p_enemy.cpp` enemy action entry points not previously wired in managed dispatch:

- `A_Look2` (last-heard wake, ambush sight, friendly player search, spawn fidgets)
- `A_ClearLastHeard`
- `A_ChaseNative` / `A_DoChaseNative` flag passthrough (`CHF_*` → `MonsterChaseFlags`, native movement policy)
- `A_MonsterRail` (lead yaw, hitscan damage; no full rail pierce)
- `A_Pain` dispatch to represented pain sound
- `CheckBossDeath` / `A_BossDeath` exit subset (`ExitOnAllBossesDead`, speckill)
- `A_ClassBossHealth` co-op 5× health once

## Implementation

- `ActorLook2Actions.cs`, `ActorClearLastHeardActions.cs`, `ActorMonsterRailActions.cs`, `BossDeathActions.cs`, `ActorClassBossHealthActions.cs`
- `ActorChaseActions.cs`, `ActorActionDispatch.cs`, `ActorSimulation.NextLook2Byte`
- `PlayLevel` boss-exit metadata

## Tests

`PEnemyActionsLoadedMapTests` (look2, pain, boss exit, class boss health, chase native dont-move, clear last heard, dispatch).

## Verified

`dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green.
