# Chunk 693 — Whole `ThingCheck`, `CheckPlane`, and `EditTraceResult` (`p_trace.cpp` 668–768, 952–1025).

## Scope

Whole `ThingCheck`, `CheckPlane`, and `EditTraceResult` (`p_trace.cpp` 668–768, 952–1025).

## Native ranges

- `src/playsim/p_trace.cpp` **FTraceInfo::ThingCheck** 668–768 (101 lines)
- `src/playsim/p_trace.cpp` **FTraceInfo::CheckPlane** 952–972 (21 lines)
- `src/playsim/p_trace.cpp` **EditTraceResult** 980–1025 (46 lines)

**Credited native LOC:** 168 (tally `nativeLoc` 168).

## Managed

`VmPTraceThingPlaneEdit.cs`
Managed LOC (tally): 255.
Conversion commit: `592b9bb56ad224fed06400cba202d8c88ae75de4`.

## Padding review (2026-10-21)

| Managed | Native (`p_trace.cpp`) |
|---------|----------------------|
| `VmPTraceThingPlaneEdit` | `ThingCheck` 668–768; `CheckPlane` 952–972; `EditTraceResult` 980–1025 |


**Post-review ratio:** 255/168 managed/native ≈ 152% (src file LOC at HEAD; see tally).

## Tests

- `VmPTraceThingPlaneEditTests.ThingCheck_HitsActorWhenNo3DFloors`
- `VmPTraceThingPlaneEditTests.ThingCheck_3DFloorCeilingBlocksBeforeActor`
- `VmPTraceThingPlaneEditTests.CheckPlane_RequiresEnterDistWindow`
- `VmPTraceThingPlaneEditTests.EditTraceResult_NoSkyClearsSkyFlatHit`
- `VmPTraceThingPlaneEditTests.EditTraceResult_HitSkyMarksHasHitSky`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 693 --to 693` (0 hard / 0 soft).

## Seams

3D-floor early outs in `ThingCheck` use injected callbacks in full sim.

## Credit rationale

Thing/plane intercept and trace flag editing after traverse chunk 692.
