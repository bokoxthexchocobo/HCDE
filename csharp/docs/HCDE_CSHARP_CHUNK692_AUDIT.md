# Chunk 692 — Whole `FTraceInfo::TraceTraverse` (`p_trace.cpp` 776–944).

## Scope

Whole `FTraceInfo::TraceTraverse` (`p_trace.cpp` 776–944).

## Native ranges

- `src/playsim/p_trace.cpp` **FTraceInfo::TraceTraverse** 776–944 (169 lines)

**Credited native LOC:** 169 (tally `nativeLoc` 169).

## Managed

`VmPTraceTraverse.cs`
Managed LOC (tally): 315.
Conversion commit: `d9552d76a44591b47c64acc55e3f6f179b90bb38`.

## Padding review (2026-10-21)

| Managed | Native (`p_trace.cpp`) |
|---------|----------------------|
| `TraceTraverse` | `TraceTraverse` **776–944** |
| `TryRecord3DWaterSplash` / `RunInterceptTraverseLoop` / `RunInterceptStep` | intercept loop **785–859** |
| `TryEnterFloorPortal` / `TryEnterCeilingPortal` | sector portals **812–835** |
| `ShouldTryLinePortal` / `LinePortalAllowed` | line portal **837–851** |
| `RunFinishSectorPlanes` | floor/ceiling plane finish **861–873** |
| `RunHeightSecWaterCross` | heightsec water **878–895** |
| `RunFinalizeMissWhenNoHit` | miss completion **896–907** |
| `RunAssignFfloorOnPlaneHit` / `FindFfloorOnFloorHit` / `FindFfloorOnCeilingHit` | 3D floor on plane hit **911–941** |

### Out-of-body uncredit (692)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **169** |
| **Out-of-body removed** | **0** |
| **Honest** | **169** |


**Post-review ratio:** 315/169 managed/native ≈ 186% (src file LOC at HEAD; see tally).

## Tests

- `VmPTraceTraverseTests.TraceTraverse_EntersLinePortalAndSkipsLineCheck`
- `VmPTraceTraverseTests.TraceTraverse_FinalizeMissWhenNoHit`
- `VmPTraceTraverseTests.TraceTraverse_Records3DWaterSplashOncePerSector`
- `VmPTraceTraverseTests.TraceTraverse_AssignsFfloorOnFloorHit`
- `VmPTraceTraverseTests.LinePortalAllowed_RespectsPortalRestrict`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 692 --to 692` (0 hard / 0 soft).

## Seams

`FPathTraverse`, `LineCheck`, `ThingCheck`, and plane finish helpers invoked via delegates.

## Credit rationale

Main trace path loop after portal entry chunk 688.
