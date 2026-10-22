# Chunk 816 — `FInterpolator` list ops, `DInterpolation` refcount/unlink, and `DSectorPlaneInterpolation` from `r_interpolate.cpp` (170–533).

## Scope

`FInterpolator` list ops, `DInterpolation` refcount/unlink, and `DSectorPlaneInterpolation` from `r_interpolate.cpp` (170–533).

## Native ranges

- `src/r_data/r_interpolate.cpp` **FInterpolator::CountInterpolations** 170–178 (9 lines)
- `src/r_data/r_interpolate.cpp` **FInterpolator::UpdateInterpolations** 186–192 (7 lines)
- `src/r_data/r_interpolate.cpp` **FInterpolator::AddInterpolation** 200–206 (7 lines)
- `src/r_data/r_interpolate.cpp` **FInterpolator::RemoveInterpolation** 214–228 (15 lines)
- `src/r_data/r_interpolate.cpp` **FInterpolator::DoInterpolations** 236–253 (18 lines)
- `src/r_data/r_interpolate.cpp` **FInterpolator::RestoreInterpolations** 261–271 (11 lines)
- `src/r_data/r_interpolate.cpp` **FInterpolator::ClearInterpolations** 279–293 (15 lines)
- `src/r_data/r_interpolate.cpp` **DInterpolation::AddRef** 317–320 (4 lines)
- `src/r_data/r_interpolate.cpp` **DInterpolation::DelRef** 328–337 (10 lines)
- `src/r_data/r_interpolate.cpp` **DInterpolation::UnlinkFromMap** 345–350 (6 lines)
- `src/r_data/r_interpolate.cpp` **DSectorPlaneInterpolation::DSectorPlaneInterpolation** 379–392 (14 lines)
- `src/r_data/r_interpolate.cpp` **DSectorPlaneInterpolation::UnlinkFromMap** 400–420 (21 lines)
- `src/r_data/r_interpolate.cpp` **DSectorPlaneInterpolation::UpdateInterpolation** 428–440 (13 lines)
- `src/r_data/r_interpolate.cpp` **DSectorPlaneInterpolation::Restore** 448–462 (15 lines)
- `src/r_data/r_interpolate.cpp` **DSectorPlaneInterpolation::Interpolate** 470–501 (32 lines)
- `src/r_data/r_interpolate.cpp` **DSectorPlaneInterpolation::Serialize** 509–517 (9 lines)
- `src/r_data/r_interpolate.cpp` **DSectorPlaneInterpolation::PropagateMark** 526–533 (8 lines)

**Credited native LOC:** 214 (tally `nativeLoc` 214).

## Managed

`VmRInterpolatorCore.cs`
Managed LOC (tally): 249.
Conversion commit: `1fd45c5c12ab9d164ab676f2806532c36b4dc580`.

## Padding review (2026-10-21)

| Managed | Native (`r_interpolate.cpp`) |
|---------|-------------------------------|
| `InterpolatorList` | `FInterpolator` **170–293** |
| `AddRef` / `DelRef` / `UnlinkFromMap` | `DInterpolation` **317–350** |
| `SectorPlaneInterp` ctor / unlink / serialize / mark | **379–392**, **400–420**, **509–517**, **526–533** |
| `SectorPlaneInterp` update / restore / interpolate | **428–501** |

**Post-review ratio:** 249/214 managed/native ≈ 116% (src file LOC at HEAD; see tally).

## Tests

- `VmRInterpolatorCoreTests.CountInterpolations_WalksList`
- `VmRInterpolatorCoreTests.RemoveInterpolation_UnlinksHead`
- `VmRInterpolatorCoreTests.DoInterpolations_SkipsWhenRatioOne`
- `VmRInterpolatorCoreTests.DelRef_ForcesUnlinkAtZero`
- `VmRInterpolatorCoreTests.PlaneCtor_AttachesAndRegisters`
- `VmRInterpolatorCoreTests.PlaneUnlink_ClearsFloorSlotAndAttached`
- `VmRInterpolatorCoreTests.PlaneInterp_LerpsFloorHeight`
- `VmRInterpolatorCoreTests.PlaneSerialize_CapturesFields`
- `VmRInterpolatorCoreTests.PlanePropagateMark_WalksAttached`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 816 --to 816` (0 hard / 0 soft).

## Seams

`P_Start3dMidtexInterpolations`, `P_RecalculateAttached3DFloors`, `GC::Mark`, and live `sector_t` planes are injected.

## Credit rationale

First tally credit for renderer movement interpolation list and sector plane scroll backup.
