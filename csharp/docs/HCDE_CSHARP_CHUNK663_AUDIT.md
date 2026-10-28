# Chunk 663 — Whole `SightCheck::PTR_SightTraverse` (`p_sight.cpp` 210–384).

## Scope

Whole `SightCheck::PTR_SightTraverse` (`p_sight.cpp` 210–384).

## Native ranges

- `src/playsim/p_sight.cpp` **SightCheck::PTR_SightTraverse** 210–384 (175 lines)

**Credited native LOC:** 175 (tally `nativeLoc` 175).

## Managed

`VmPSightPtrSightTraverse.cs`
Managed LOC (tally): 281.
Conversion commit: `fb19249b7bc817ef76aee6e34864ca51df092761`.

## Padding review (2026-10-21)

| Managed | Native (`p_sight.cpp`) |
|---------|----------------------|
| `PtrSightTraverse` + opening/portal/3D-floor phases | `PTR_SightTraverse` 210–384 |
| `EvaluateSight3dFloors` / `SightFFloorAtHit` | 3D-floor loop 297–377 |
| `SwapPortalFlagsSides` | `SightOpening::SwapSides` 73–76 |

### Out-of-body uncredit (663)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **175** |
| **Out-of-body removed** | **0** |
| **Honest** | **175** |


**Post-review ratio:** 281/175 managed/native ≈ 161% (src file LOC at HEAD; see tally).

## Tests

- `VmPSightPtrSightTraverseTests.PtrSightTraverse_ClosedDoorStops`
- `VmPSightPtrSightTraverseTests.PtrSightTraverse_UpdatesSlopes`
- `VmPSightPtrSightTraverseTests.PtrSightTraverse_CompatSameSectorSkips`
- `VmPSightPtrSightTraverseTests.PtrSightTraverse_FracZeroContinues`
- `VmPSightPtrSightTraverseTests.PtrSightTraverse_LinkedPortalQueuesAndStops`
- `VmPSightPtrSightTraverseTests.PtrSightTraverse_LineBlocksSightCollapsesOpening`
- `VmPSightPtrSightTraverseTests.PtrSightTraverse_3dFloorBlocksViewCone`
- `VmPSightPtrSightTraverseTests.PtrSightTraverse_SwapPortalFlagsSides`
- `VmPSightPtrSightTraverseTests.PtrSightTraverse_FinalizesLastZ`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 663 --to 663` (0 hard / 0 soft).

## Seams

`P_SightOpening`, 3D-floor evaluation, and intercept lists stay native.

## Credit rationale

Fresh sight TU; traverse slope/portal logic hosted.
