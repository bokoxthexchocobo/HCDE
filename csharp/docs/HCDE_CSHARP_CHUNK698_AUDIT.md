# Chunk 698 — `FTraceInfo::LineCheck` (`p_trace.cpp` 374–659)

## Combat trace swap

- Retired pre-450 `combat-trace-ordering` (`nativeLoc` 0, `managedLoc` 0, `retired: true`).
- Removed duplicate `CombatTrace.BlocksPick`; `TraceCore` line intercepts call `VmPTraceLineCheck.CombatLineBlocks` → `LineCheck` with no combat-only Z sampling (native uses `hit.Z` at `p_trace.cpp` 452–466).

## Native reference

Opening vs wall uses intercept height directly:

```464:466:src/playsim/p_trace.cpp
	else if (entersector == NULL ||
		hit.Z < bf || hit.Z > bc ||
		((in->d.line->flags & WallMask) && !special3dpass))
```

## Test migrations (former combat-trace-ordering / BlocksPick parity)

| Test | Change | Native basis |
|------|--------|----------------|
| `TraceSectorEntryTests` | Renamed to `SectorEntryUsesNativeLineCheckOpening`. Expect wall on line 0 (`ML_BLOCKHITSCAN` inner line) and no actor hit in all four geometry variants; first portal line passes when `hit.Z` remains inside the far opening. | `LineCheck` pass branch 479–480; wall mask branch 464–466 |
| `PitchedPlaneOpeningFallbackTests` | Renamed method to `PitchedTraceUsesNativeLineCheckAtPortal`; always expect wall hit at the portal (no actor) for these fixtures under `hit.Z` floor/ceiling/wall rules. | 452–466 |
| `TraceParallelPlaneOpeningTests` | Renamed to `HorizontalTraceUsesNativeLineCheckAtPortal`; always expect wall, no actor. | 452–466 |
| `ActorPlaneTraversalOrderingTests` | Unchanged (still passes with native `LineCheck`). | — |
| `PlaneWallImpactOrderingTests` | Unchanged (still passes with native `LineCheck`). | — |

## Tests

`VmPTraceLineCheckTests` plus combat ordering regressions listed on chunk 698 `nativeRanges`.
