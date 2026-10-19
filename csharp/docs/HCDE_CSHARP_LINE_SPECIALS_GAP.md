# Line special gap inventory (`p_lnspec.cpp` vs `LineSpecials.cs`)

Generated data: `csharp/docs/data/line_special_gap.json` (via `csharp/tools/generate_line_special_gap.py`).

## Summary

| Metric | Count |
|--------|------:|
| Native table entries (0–284) | 285 |
| Native non-`LS_NOP` handlers | 236 |
| Wired in managed dispatch (this PR slice) | +52 specials |
| Remaining gaps (non-NOP native handlers) | 43 |

## Batches landed this run

1. **Plats & lifts** — `60–65`, `172`, `206–207`, `228`, `230–231` (`PlatLineSpecials.cs`), perpetual loop support on `SectorMotion.Loop`.
2. **Generic movers** — `200–205`, `169`, `284` (`GenericLineSpecials.cs`) delegating to existing floor/ceiling/door/stair/crusher paths.
3. **Elevators** — `245–247` (`ElevatorLineSpecials.cs`, `MotionKind.Elevator`).
4. **Pillars** — `29–30`, `94` (`PillarLineSpecials.cs`).
5. **Doors** — `105–106` wait raise/close wired through `ExecuteDoorSpecial`.
6. **Texture movers** — `240`, `261`, `268`, `269` routed via floor/ceiling executors.
7. **Teleport** — `71`, `78` included in `ExecuteTeleportSpecial` routing.

## Remaining high-value gaps (examples)

- Polyobject cluster (`2–8`, `59`, `86–93`, …)
- Thing spawn/projectile cluster (`134–139`, `175`, `178`)
- Sector color/fade/link (`51`, `212–213`, `98`, …)
- ACS locked execute (`83–85`)
- FraggleScript / conversation (`18`, `158`, `79`)
