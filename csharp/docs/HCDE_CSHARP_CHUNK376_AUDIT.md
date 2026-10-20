# Chunk 376 — drawSeg/drawPolySeg rotation policy, AM_Check3DFloors, and automap special boundary helpers

## Scope

drawSeg/drawPolySeg rotation policy, AM_Check3DFloors, and automap special boundary helpers

## Native ranges

- `src/am_map.cpp` **DAutomap::drawSeg** 2346–2360 (15 lines)
- `src/am_map.cpp` **DAutomap::drawPolySeg** 2362–2376 (15 lines)
- `src/am_map.cpp` **DAutomap::showSS** 2378–2410 (33 lines)
- `src/am_map.cpp` **AM_Check3DFloors** 2418–2474 (57 lines)
- `src/am_map.cpp` **AM_checkSectorActions** 2479–2622 (144 lines)

**Credited native LOC:** 264 (tally `nativeLoc` 264).

## Managed

`VmAutomapBoundaryHelpers.cs`, `VmAutomapBoundaryHelpersTests.cs`, `VmAutomapDrawSeg.cs`
Managed LOC (tally): 306.
Conversion commit: `a7f6d032d240f9bda90b9bff6ef09be6fd967e97`.

## Tests

- `VmAutomapDrawSegTests.ShouldApplyMapRotation_MatchesNativeModes`
- `VmAutomapDrawSegTests.RotateEndpointsIfNeeded_CallsDelegateWhenEnabled`
- `VmAutomapBoundaryHelpersTests.Check3DFloors_ReturnsFalse_WhenBothEmpty`
- `VmAutomapBoundaryHelpersTests.Check3DFloors_ReturnsTrue_WhenCountsMismatch`
- `VmAutomapBoundaryHelpersTests.Check3DFloors_ReturnsTrue_WhenFrontFloorUnmatched`
- `VmAutomapBoundaryHelpersTests.CheckSpecialBoundary_LineActivation_Wins`
- `VmAutomapBoundaryHelpersTests.IsTeleportSpecial_MatchesNativeSet`
- `VmAutomapBoundaryHelpersTests.TryResolveLockBoundary_UsesLineLockNumber`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 376 --to 376` (0 hard / 0 soft).

## Seams

`drawMline`, polyobject iterators, and `P_GetLineSpecialInfo` tables are not hosted.

## Credit rationale

Credits `DAutomap::drawSeg`, `drawPolySeg`, `showSS` seg path, `AM_Check3DFloors`, and `AM_checkSectorActions` through `AM_isLockBoundary` from `am_map.cpp`.
