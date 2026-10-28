# Chunk 475 — aim_t Clear, AimTraverse3DFloors, EnterSectorPortal, EnterLinePortal

## Scope

aim_t Clear, AimTraverse3DFloors, EnterSectorPortal, EnterLinePortal

## Native ranges

- `src/playsim/p_map.cpp` **AimTarget::Clear** 3902–3906 (5 lines)
- `src/playsim/p_map.cpp` **AimTarget::Clear** 4015–4109 (95 lines)
- `src/playsim/p_map.cpp` **AimTarget::Clear** 4117–4140 (24 lines)
- `src/playsim/p_map.cpp` **AimTarget::Clear** 4148–4177 (30 lines)
- `src/playsim/p_map.cpp` **AimTarget::Clear** 4261–4286 (26 lines)

**Credited native LOC:** 180 (tally `nativeLoc` 180).

## Managed

`VmPMapAimTraverse.cs`
Managed LOC (tally): 260.
Conversion commit: `3d425e688bb4ec4ac7be3e12a9f5c28778869c5e`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `ApplyLowerBlockAdjust` / `ApplyUpperBlockAdjust` / `BlocksCompletely` / `BlocksLowerView` / `BlocksUpperView` / `CeilingPortalBelowLimit` | `AimTarget::Clear` 3902–3907 |
| `CeilingPortalState` | `aim_t::AimTraverse` 4261–4285 |
| `ClampInterceptFrac` / `ClearPlaneCheckFlag` / `DivlineSidesDiffer` / `Evaluate3dFloorBlock` / `EvaluateInitialCeilingPortal` / `EvaluateInitialFloorPortal` | `AimTarget::Clear` 3902–3907 |
| `EvaluateLinePortalEntry` / `EvaluateLinePortalTrace` | `aim_t::EnterLinePortal` 4148–4177 |
| `EvaluateSectorPortalEntry` / `EvaluateSectorPortalTrace` | `aim_t::EnterSectorPortal` 4117–4140 |
| `FloorAboveActor` / `FloorBelowActor` / `FloorPortalAboveLimit` | `AimTarget::Clear` 3902–3907 |
| `FloorPortalState` | `aim_t::AimTraverse` 4262–4286 |
| `LeavingSectorLowerSlope` / `LeavingSectorMatches` | `AimTarget::Clear` 3902–3907 |
| `LeavingSectorUpperSlope` | `aim_t::AimTraverse3DFloors` 4015–4109 |
| `LinePortalRestricted` | `aim_t::EnterLinePortal` 4148–4177 |
| `MergeAimResults` / `PitchRangeClosed` / `PortalPositionIsCeiling` / `Requires3dCheck` | `AimTarget::Clear` 3902–3907 |
| `SectorHas3dFloors` | `aim_t::AimTraverse3DFloors` 4015 |
| `SectorPortalAimDir` | `aim_t::EnterSectorPortal` 4117–4140 |
| `SetResultWhenCloser` / `ShootthroughFloor` | `AimTarget::Clear` 3902–3907 |
| `SkipPortalFracBias` | `aim_t::EnterSectorPortal` 4117–4140 |
| `SkipShootthroughFloor` | `AimTarget::Clear` 3902–3907 |
| `SuppressCeilingPortalUnder3dFloor` / `SuppressFloorPortalAbove3dFloor` | `aim_t::AimTraverse3DFloors` 4015–4028 |
| `TracePastBottomEdge` / `TracePastLeftEdge` / `TracePastRightEdge` | `AimTarget::Clear` 3902–3907 |

### Out-of-body uncredit (475)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **181** |
| **Out-of-body removed** | **1** |
| **Honest** | **180** |


**Post-review ratio:** 260/180 managed/native ≈ 144% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapAimTraverseTests.AimTraverseInit_CeilingPortal`
- `VmPMapAimTraverseTests.MergeAimResults_PrefersCloserFrac`
- `VmPMapAimTraverseTests.ThreeDFloor_BlocksWhenPitchRangeClosed`
- `VmPMapAimTraverseTests.SectorPortal_SkipsHighCeilingPortal`
- `VmPMapAimTraverseTests.LinePortal_BlocksWhenRestricted`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 475 --to 475` (0 hard / 0 soft).

## Seams

`aim_t::AimTraverse`, `P_TranslatePortalXY`, `Printf`, and `FPathTraverse` are not hosted.

## Credit rationale

Credits aim target reset, 3D-floor pitch clipping, portal trace setup, ExitPoint edges, AimTraverse portal priming from `p_map.cpp` (3902–4286; bounce actor band is chunk461).
