# p_map.cpp try-move / use / line attack chunk 3 audit

## Scope

Flat-sector subsets of `src/playsim/p_map.cpp` already implemented in `ActorPhysics`, `LineSpecials`, `CombatTrace`, `LineAttackImpact`, and `AcsLineAttack`. This chunk credits tested behavior and adds `P_NoWayTraverse` / `P_UseLines` use-fail rumble when a non-special wall blocks the use ray.

| Symbol | Lines | Managed |
| --- | --- | --- |
| `P_CheckPosition` | 1817–2036 | `ActorPhysics.TryMove` position checks (flat subset) |
| `P_TryMove` | 2368–2889 | `ActorPhysics.TryMove` / `TryWanderMove` |
| `FSlide::SlideMove` | 3018–3371 | `ActorPhysics.SlideFromBlock` / player slide |
| `P_UseTraverse` | 5829–5957 | `LineSpecials.ActivateUses` ordered trace |
| `P_NoWayTraverse` | 5972–5993 | `LineSpecials.NoWayTraverse` (new) |
| `P_UseLines` | 6003–6027 | `LineSpecials.ActivateUses` + `*usefail` rumble |
| `P_LineAttack` | 4699–5050 | `CombatTrace.TraceLineAttack`, `AcsLineAttack`, `LineAttackImpact` |

**Behavior native LOC credited:** 1602.

## Tests

`UseTraceTests`, `UseRumbleTests.BlockedNonSpecialLineEmitsUseFailPattern`, `GeometryLineAttackTests`, `HitscanPlaneDamageTests`, `ActorPhysics` / `TryMove` / slide / dropoff suites, `MapRadiusAttack` regressions.

## Verified

`dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green.
