# Chunk 475 — aim_t Clear, AimTraverse3DFloors, EnterSectorPortal, EnterLinePortal

## Scope

aim_t Clear, AimTraverse3DFloors, EnterSectorPortal, EnterLinePortal

## Native ranges

- `src/playsim/p_map.cpp` **AimTarget::Clear** 3902–3907 (6 lines)
- `src/playsim/p_map.cpp` **aim_t::AimTraverse3DFloors** 4015–4109 (95 lines)
- `src/playsim/p_map.cpp` **aim_t::EnterSectorPortal** 4117–4140 (24 lines)
- `src/playsim/p_map.cpp` **aim_t::EnterLinePortal** 4148–4177 (30 lines)
- `src/playsim/p_map.cpp` **aim_t::ExitPoint** 4185–4242 (58 lines)
- `src/playsim/p_map.cpp` **aim_t::AimTraverse** 4251–4286 (36 lines)

**Credited native LOC:** 249 (tally `nativeLoc` 249).

## Managed

`VmPInteractionDamageLateTests.cs`, `VmPMapAimTraverse.cs`, `VmPMapAimTraverseTests.cs`
Managed LOC (tally): 176.
Conversion commit: `e262efe19e12beb689f484b53a9bf2a8d3d84366`.

## Tests

- `VmPMapAimTraverseTests.MergeAimResults_PrefersCloserFrac`
- `VmPMapAimTraverseTests.ThreeDFloor_BlocksWhenPitchRangeClosed`
- `VmPMapAimTraverseTests.ThreeDFloor_PassesWithout3dSectors`
- `VmPMapAimTraverseTests.LeavingSectorUpperSlope_RaisesTopPitch`
- `VmPMapAimTraverseTests.SectorPortal_SkipsHighCeilingPortal`
- `VmPMapAimTraverseTests.SectorPortalTrace_SetsAimUpOnCeiling`
- `VmPMapAimTraverseTests.SkipPortalFrac_BiasesPastTransition`
- `VmPMapAimTraverseTests.LinePortal_BlocksWhenRestricted`
- `VmPMapAimTraverseTests.LinePortalTrace_ComputesTranslatedPosition`
- `VmPMapAimTraverseTests.ExitPoint_HitsIntercept`
- `VmPMapAimTraverseTests.ExitPoint_SkipsWithout3d`
- `VmPMapAimTraverseTests.AimTraverseInit_CeilingPortal`
- `VmPMapAimTraverseTests.AimTraverseInit_SuppressesCeilingUnder3dFloor`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 475 --to 475` (0 hard / 0 soft).

## Seams

`aim_t::AimTraverse`, `P_TranslatePortalXY`, `Printf`, and `FPathTraverse` are not hosted.

## Credit rationale

Credits aim target reset, 3D-floor pitch clipping, portal trace setup, ExitPoint edges, AimTraverse portal priming from `p_map.cpp` (3902–4286; bounce actor band is chunk461).
