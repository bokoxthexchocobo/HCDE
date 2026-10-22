# Chunk 688 — Whole portal trace entry and `Setup3DFloors` (`p_trace.cpp` 99–271, 279–365). `isLiquid` credited as whole function used by setup.

## Scope

Whole portal trace entry and `Setup3DFloors` (`p_trace.cpp` 99–271, 279–365). `isLiquid` credited as whole function used by setup.

## Native ranges

- `src/playsim/p_trace.cpp` **GetPortalTransition** 99–126 (28 lines)
- `src/playsim/p_trace.cpp` **isLiquid** 128–133 (6 lines)
- `src/playsim/p_trace.cpp` **Trace** 141–197 (57 lines)
- `src/playsim/p_trace.cpp` **FTraceInfo::EnterSectorPortal** 206–231 (26 lines)
- `src/playsim/p_trace.cpp` **FTraceInfo::EnterLinePortal** 239–271 (33 lines)
- `src/playsim/p_trace.cpp` **FTraceInfo::Setup3DFloors** 279–365 (87 lines)

**Credited native LOC:** 237 (tally `nativeLoc` 237).

## Managed

`VmPTracePortalEntry.cs`, `VmPTraceSetup3DFloors.cs`
Managed LOC (tally): 305.
Conversion commit: `abfdff248f597dd645f377c977b837adf9d928d5`.

## Padding review (2026-10-21)

| Managed | Native (`p_trace.cpp`) |
|---------|----------------------|
| `VmPTracePortalEntry` | `GetPortalTransition` 99–126; `isLiquid` 128–133; `Trace` 141–197; `EnterSectorPortal` 206–231; `EnterLinePortal` 239–271 |
| `VmPTraceSetup3DFloors` | `Setup3DFloors` 279–365 |


**Post-review ratio:** 305/237 managed/native ≈ 129% (src file LOC at HEAD; see tally).

## Tests

- `VmPTracePortalEntryTests.GetPortalTransition_MovesThroughCeilingPortal`
- `VmPTracePortalEntryTests.IsLiquid_TrueForSwimmableOrTerrainSplash`
- `VmPTracePortalEntryTests.EnterSectorPortal_AppliesDisplacement`
- `VmPTraceSetup3DFloorsTests.Setup3DFloors_ClipsInsideSolid3DFloor`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 688 --to 688` (0 hard / 0 soft).

## Seams

`FTraceInfo::TraceTraverse`, `LineCheck`, and `EditTraceResult` remain native (chunk 692/693).

## Credit rationale

Portal trace entry after chunk 686; setup band merged here so credit is not exactly 150 LOC.
