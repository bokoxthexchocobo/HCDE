# Chunk 475 — aim_t Clear, AimTraverse3DFloors, EnterSectorPortal, EnterLinePortal

## Scope

aim_t Clear, AimTraverse3DFloors, EnterSectorPortal, EnterLinePortal

## Native ranges

- `src/playsim/p_map.cpp` **AimTarget::Clear** 3902–3907 (6 lines)
- `src/playsim/p_map.cpp` **aim_t::AimTraverse3DFloors** 4015–4109 (95 lines)
- `src/playsim/p_map.cpp` **aim_t::EnterSectorPortal** 4117–4140 (24 lines)
- `src/playsim/p_map.cpp` **aim_t::EnterLinePortal** 4148–4177 (30 lines)

**Credited native LOC:** 155 (tally `nativeLoc` 155).

## Managed

`VmPMapAimTraverse.cs`, `VmPMapAimTraverseTests.cs`, `VmPMapBounceActor.cs`, `VmPMapBounceActorTests.cs`
Managed LOC (tally): 89.
Conversion commit: `1ed2d2b5712b0aebfe789b0d2ba8d5f1c4ed7ad9`.

## Tests

- `VmPMapAimTraverseTests.MergeAimResults_PrefersCloserFrac`
- `VmPMapAimTraverseTests.ThreeDFloor_BlocksWhenPitchRangeClosed`
- `VmPMapAimTraverseTests.ThreeDFloor_PassesWithout3dSectors`
- `VmPMapAimTraverseTests.SectorPortal_SkipsHighCeilingPortal`
- `VmPMapAimTraverseTests.LinePortal_BlocksWhenRestricted`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 475 --to 475` (0 hard / 0 soft).

## Seams

`aim_t::AimTraverse`, `P_TranslatePortalXY`, `Printf`, and `FPathTraverse` are not hosted.

## Credit rationale

Credits aim target reset, 3D-floor pitch clipping, sector portal limits, and line portal restriction from `p_map.cpp` (3902–4177; bounce actor band is chunk461).
