# p_trace portal entry — tally merge (not chunk 688 land)

## Why there is no `p-trace-portal-entry-chunk688` row

`VmPTracePortalEntry` ports whole functions in `p_trace.cpp` 99–271 (excluding `isLiquid` 128–133, still native). That band is **144 native LOC**, below the 150-line active-chunk floor. Floor-driven `isLiquid` credit was reverted; credit is merged into the pre-450 **`combat-trace-ordering`** row on whole functions instead of a padded chunk land.

## Combat-trace band narrow (double-count fix only)

`combat-trace-ordering` previously credited `p_trace.cpp` **200–479** as one intercept region. Portal entry functions **99–271** are now credited separately on the same tally row. The intercept band was narrowed to **279–479** (`Setup3DFloors` onward) so lines **200–278** are not double-counted. No other lines were removed from combat credit.

| Change | Before | After |
|--------|--------|-------|
| Intercept region | 200–479 | 279–479 |
| Portal entry (merged) | (uncredited) | 99–126, 141–197, 206–231, 239–271 |

## Managed

- `CombatTrace.cs` — intercept / BlocksPick parity (existing).
- `VmPTracePortalEntry.cs` — portal transition, trace setup hooks, enter sector/line portal (commit `14481966`).

## Tests

- `VmPTracePortalEntryTests.*` — portal entry bands.
- `ActorPlaneTraversalOrderingTests`, `TraceSectorEntryTests`, `PlaneWallImpactOrderingTests` — intercept band.

## Verified

`dotnet test csharp/HCDE.sln`; `reaudit_chunks.py audit --from 450 --to 687` (chunk 688 tally row removed).
