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
Managed LOC (tally): 87.
Conversion commit: `a8b458b8c057373a65ef9b4d06c91e73d1d413da`.

## Padding review (2026-10-21)

| Managed | Native (`p_trace.cpp`) |
|---------|----------------------|
| `VmPTraceThingPlaneEdit` | `ThingCheck` 668–768; `CheckPlane` 952–972; `EditTraceResult` 980–1025 |


**Post-review ratio:** 87/168 managed/native ≈ 52% (src file LOC at HEAD; see tally).

## Tests

- `VmPTraceThingPlaneEditTests.ThingCheckPassesTopEntry_WhenBelowTopOrMovingDown`
- `VmPTraceThingPlaneEditTests.EditTraceResult_NoSkyClearsSkyFlatHit`
- `VmPTraceThingPlaneEditTests.CheckPlaneHits_WhenRayIntersectsPlaneSegment`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 693 --to 693` (0 hard / 0 soft).

## Seams

3D-floor early outs in `ThingCheck` use injected callbacks in full sim.

## Credit rationale

Thing/plane intercept and trace flag editing after traverse chunk 692.
