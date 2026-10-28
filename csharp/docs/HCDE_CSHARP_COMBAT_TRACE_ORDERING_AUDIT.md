# Principal audit — CombatTrace plane/wall/actor ordering

**Date:** 2026-10-19  
**Scope:** Native `p_trace.cpp` / `p_map.cpp` combat trace intercept ordering vs `CombatTrace.cs` (merged from `cursor/combat-trace-plane-ordering-bea5` with regression fixes).  
**Result:** Accepted. All nine previously failing Playsim ordering tests pass without reintroducing box-trace, refire, pitched-plane, weapon-spread, or chainsaw regressions.

## Native behavior

- Line, plane, and actor intercepts are collected and traversed in deterministic distance order for `FTrace` / line attacks.
- `BlocksPick` must respect near-plane crossing and closed openings (pitched view through a closed portal).
- Box traces must not apply an extra XY radius gate on non-Z-adjusted actor entry that wholesale cherry-pick introduced.

## Managed implementation

- `CombatTrace.cs` merges intercept lists with stable ordering parity.
- Removed the in-box XY radius check from the cherry-picked branch.
- After the near plane is crossed, `BlocksPick` compares sample Z against far floor/ceiling instead of unconditionally allowing passage.

## Divergences

- Full `p_trace.cpp` feature surface (3D midtex, portals, cameras) remains partially modeled; this slice only covers behaviors exercised by Playsim regression tests.
- No new opcodes or line specials in this chunk.

## Test evidence

Full Playsim suite after the merge commit:

```text
dotnet test csharp/tests/HCDE.Playsim.Tests/HCDE.Playsim.Tests.csproj -c Release
```

`16955` passed, `0` failed (includes `ActorPlaneTraversalOrderingTests`, `TraceSectorEntryTests`, `PlaneWallImpactOrderingTests`).

## LOC tally

`LOC tally: converted this run = 280 native lines (85 C# lines changed); native remaining = 507,768`

Native lines attributed: `p_trace.cpp` intercept collection/traversal/`BlocksPick` region (~lines 200–520) now covered by green ordering regressions.
