# Chunk 688 — Whole `GetPortalTransition`, `isLiquid`, `Trace` portal-report setup, `EnterSectorPortal`, and `EnterLinePortal` (`p_trace.cpp` 99–271).

## Scope

Whole `GetPortalTransition`, `isLiquid`, `Trace` portal-report setup, `EnterSectorPortal`, and `EnterLinePortal` (`p_trace.cpp` 99–271).

## Native ranges

- `src/playsim/p_trace.cpp` **GetPortalTransition** 99–126 (28 lines)
- `src/playsim/p_trace.cpp` **isLiquid** 128–133 (6 lines)
- `src/playsim/p_trace.cpp` **Trace** 141–197 (57 lines)
- `src/playsim/p_trace.cpp` **FTraceInfo::EnterSectorPortal** 206–231 (26 lines)
- `src/playsim/p_trace.cpp` **FTraceInfo::EnterLinePortal** 239–271 (33 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPTracePortalEntry.cs`
Managed LOC (tally): 217.
Conversion commit: `357f12d9573b4790a500487ae3159a87a1a28567`.

## Padding review (2026-10-21)

| Managed | Native (`p_trace.cpp`) |
|---------|----------------------|
| `VmPTracePortalEntry` | `GetPortalTransition` 99–126; `isLiquid` 128–133; `Trace` 141–197; `EnterSectorPortal` 206–231; `EnterLinePortal` 239–271 |


**Post-review ratio:** 217/150 managed/native ≈ 145% (src file LOC at HEAD; see tally).

## Tests

- `VmPTracePortalEntryTests.GetPortalTransition_MovesThroughCeilingPortal`
- `VmPTracePortalEntryTests.IsLiquid_TrueForSwimmableOrTerrainSplash`
- `VmPTracePortalEntryTests.EnterSectorPortal_AppliesDisplacement`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 688 --to 688` (0 hard / 0 soft).

## Seams

`FTraceInfo::TraceTraverse`, `Setup3DFloors` body, and `EditTraceResult` remain native.

## Credit rationale

Trace portal transitions after playsim portal translation in chunk 686.
