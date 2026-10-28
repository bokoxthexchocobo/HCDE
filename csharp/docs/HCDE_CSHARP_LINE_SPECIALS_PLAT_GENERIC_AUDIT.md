# Principal audit — line specials (plats, generic movers, elevators, pillars)

**Date:** 2026-10-19  
**Scope:** Native `p_lnspec.cpp` `LS_Plat_*`, `LS_Generic_*`, `LS_Elevator_*`, `LS_Pillar_*`, door wait (`105–106`), texture floor/ceiling movers, vs `LineSpecials.cs` + new `*LineSpecials.cs` helpers.  
**Result:** Accepted for this slice with documented simplifications.

## Native behavior

- Plats use `EV_DoPlat` types (perpetual, down-wait-up, by-value, stop, toggle).
- Generic floor/ceiling pack target mode + crush/change flags into `EV_DoFloor` / `EV_DoCeiling`.
- Elevators move floor and ceiling together to the next neighbor height.
- Pillars combine floor raise and ceiling motion (`EV_DoPillar`).

## Managed implementation

- `PlatLineSpecials` maps plat specials onto existing `StartMapFloor` lift motions; `Loop` re-arms perpetual plats.
- `GenericLineSpecials` decodes generic args and dispatches to `ExecuteFloorSpecial` / `ExecuteCeilingSpecial` / `ExecuteDoorSpecial` / `ExecuteStairSpecial` / `PlatLineSpecials` / `CeilingActions`.
- `ElevatorLineSpecials` adds `MotionKind.Elevator` tick path moving both planes in lockstep.
- `PillarLineSpecials` uses combined floor/ceiling executor calls (build/open/crush subset).

## Divergences

- Plat stop/remove modes only approximate Hexen vs Doom defaults (`arg3` on `Plat_Stop`).
- Texture-by-line (`floorRaiseByTexture` / `ceilRaiseByTexture`) uses `NextHigher` heuristics, not sidedef texture height.
- Pillars do not model full `EV_DoPillar` crush/build state machine.
- Polyobjects, thing spawn/projectile, sector color/fade, and locked ACS execute remain gaps.

## Test evidence

`LineSpecialBatchTests` (5 cases) plus full Playsim suite:

```text
dotnet test csharp/tests/HCDE.Playsim.Tests/HCDE.Playsim.Tests.csproj -c Release --filter FullyQualifiedName~LineSpecialBatchTests
dotnet test csharp/tests/HCDE.Playsim.Tests/HCDE.Playsim.Tests.csproj -c Release
```

`16967` Playsim tests passed.

## LOC tally

`LOC tally: converted this run = 1,285 native lines (530 C# lines added); native remaining = 506,322`

Native lines attributed: `p_lnspec.cpp` plat (`~937–1038`), generic (`~532–650`, `~884–928`, `~1040–1065`), elevator (`~2048–2064`), pillar (`~652–668`), door wait (`~278–288`), texture movers (`~472–476`, `~836–876`), plus dispatch glue in the native table.
