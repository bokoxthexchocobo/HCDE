# Chunk 698 — Whole `FTraceInfo::LineCheck` (`p_trace.cpp` 374–659). `combat-trace-ordering` retired; `CombatTraceLineCheck` (separate file) calls `LineCheck`.

## Scope

Whole `FTraceInfo::LineCheck` (`p_trace.cpp` 374–659). `combat-trace-ordering` retired; `CombatTraceLineCheck` (separate file) calls `LineCheck`.

## Native ranges

- `src/playsim/p_trace.cpp` **FTraceInfo::LineCheck** 374–659 (286 lines)

**Credited native LOC:** 286 (tally `nativeLoc` 286).

## Managed

`VmPTraceLineCheck.cs`
Managed LOC (tally): 406.
Conversion commit: `37bdfe0bd8e18c20083d2bdf9f2b655117997089`.

## Padding review (2026-10-21)

| Managed | Native (`p_trace.cpp`) |
|---------|----------------------|
| `VmPTraceLineCheck` | `LineCheck` 374–659 |

### Out-of-body uncredit (698)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **286** |
| **Out-of-body removed** | **0** |
| **Honest** | **286** |


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
- `ActorPlaneTraversalOrderingTests.EarlierLineStillResolvesPlaneBeforeActorEntry`
- `ActorPlaneTraversalOrderingTests.OrdinaryPlaneIsFallbackAfterActorTraversal`
- `TraceSectorEntryTests.SectorEntryUsesNativeLineCheckOpening`
- `PlaneWallImpactOrderingTests.ClosePlaneAndWallIntersectionsUseActualDistance`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 698 --to 698` (0 hard / 0 soft).

## Seams

`P_PointOnLineSide`, `P_ActivateLine`, `CheckSectorPlane` / `CheckPlane`, `TraceCallback`, sector `ZatPoint` / textures / `GetHeightSec`, water cross, 3D-floor `ffloors` loop, `DummySector` copy, and `SetSourcePosition` are injected delegates on `LineCheckState`.

## Credit rationale

Line intercept classification after portal/setup chunks 688–692; completes the traverse `LineCheck` hook with a faithful branch tree. Absorbed pre-450 combat row managed `BlocksPick` duplicate (retired).
