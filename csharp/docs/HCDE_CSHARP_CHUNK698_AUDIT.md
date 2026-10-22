# Chunk 698 — Whole `FTraceInfo::LineCheck` (`p_trace.cpp` 374–659). Pre-450 `combat-trace-ordering` row keeps 366–479 credit for `CombatTrace.cs` `BlocksPick` parity; this chunk credits full `LineCheck` without removing that combat row (parallel until `CombatTrace` delegates here).

## Scope

Whole `FTraceInfo::LineCheck` (`p_trace.cpp` 374–659). Pre-450 `combat-trace-ordering` row keeps 366–479 credit for `CombatTrace.cs` `BlocksPick` parity; this chunk credits full `LineCheck` without removing that combat row (parallel until `CombatTrace` delegates here).

## Native ranges

- `src/playsim/p_trace.cpp` **FTraceInfo::LineCheck** 374–659 (286 lines)

**Credited native LOC:** 286 (tally `nativeLoc` 286).

## Managed

`VmPTraceLineCheck.cs`
Managed LOC (tally): 406.
Conversion commit: `cb6fe500d29c172275014a1b0654de89029a5afe`.

## Padding review (2026-10-21)

| Managed | Native (`p_trace.cpp`) |
|---------|----------------------|
| `VmPTraceLineCheck` | `LineCheck` 374–659 |


**Post-review ratio:** 406/286 managed/native ≈ 142% (src file LOC at HEAD; see tally).

## Tests

- `VmPTraceLineCheckTests.LineCheck_OneSidedLineHitsMiddleWall`
- `VmPTraceLineCheckTests.LineCheck_WallMaskBlocksShotInOpening`
- `VmPTraceLineCheckTests.LineCheck_PassesIntoBackSectorWhenOpeningClear`
- `VmPTraceLineCheckTests.LineCheck_CompatSameSectorActivatesAndContinues`
- `VmPTraceLineCheckTests.LineCheck_3DFloorEdgeHitsFFloorTier`
- `VmPTraceLineCheckTests.LineCheck_HitsFloorBeforeWallWhenBelowFloorPlane`
- `VmPTraceLineCheckTests.LineCheck_TraceSkipRecursesWithSpecial3DPass`
- `VmPTraceLineCheckTests.LineCheck_WaterCrossRecordsHeightSecWhenPlaneValid`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 698 --to 698` (0 hard / 0 soft).

## Seams

`P_PointOnLineSide`, `P_ActivateLine`, `CheckSectorPlane` / `CheckPlane`, `TraceCallback`, sector `ZatPoint` / textures / `GetHeightSec`, water cross, 3D-floor `ffloors` loop, `DummySector` copy, and `SetSourcePosition` are injected delegates on `LineCheckState`.

## Credit rationale

Line intercept classification after portal/setup chunks 688–692; completes the traverse `LineCheck` hook with a faithful branch tree.
