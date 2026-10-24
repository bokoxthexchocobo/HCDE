# Chunk 369 — Automap scale restore, bounds, panning, and parchment scroll policy

## Scope

Automap scale restore, bounds, panning, and parchment scroll policy

## Native ranges

- `src/am_map.cpp` **DAutomap::activateNewScale** 1111–1121 (11 lines)
- `src/am_map.cpp` **DAutomap::restoreScaleAndLoc** 1143–1163 (21 lines)
- `src/am_map.cpp` **DAutomap::addMark** 1171–1184 (14 lines)
- `src/am_map.cpp` **DAutomap::findMinMaxBoundaries** 1193–1218 (26 lines)
- `src/am_map.cpp` **DAutomap::calcMinMaxMtoF** 1226–1234 (9 lines)
- `src/am_map.cpp` **DAutomap::ClipRotatedExtents** 1242–1259 (18 lines)
- `src/am_map.cpp` **DAutomap::ScrollParchment** 1267–1294 (28 lines)
- `src/am_map.cpp` **DAutomap::changeWindowLoc** 1302–1328 (27 lines)

**Credited native LOC:** 154 (tally `nativeLoc` 154).

## Managed

`VmAutomapViewWindow.cs`, `VmAutomapViewWindowTests.cs`
Managed LOC (tally): 148.
Conversion commit: `78fc952a19c98006fde39cffd29f162c056150a0`.

## Tests

- `VmAutomapViewWindowTests.ActivateNewScale_ReanchorsWindow`
- `VmAutomapViewWindowTests.RestoreScaleAndLoc_FollowUsesCamera`
- `VmAutomapViewWindowTests.TryAddMarkIndex_RequiresFontOrTexture`
- `VmAutomapViewWindowTests.FindVertexBounds_TracksExtents`
- `VmAutomapViewWindowTests.CalcMinMaxMapToFrame_PicksSmallerAxisScale`
- `VmAutomapViewWindowTests.ClampParchmentScroll_BoundsMagnitude`
- `VmAutomapViewWindowTests.PanningDisablesFollow_WhenEitherAxisNonZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 369 --to 369` (0 hard / 0 soft).

## Seams

Live `DAutomap` fields, `rotate`, and texture-backed parchment tiling are not hosted; managed code mirrors numeric window/scale rules.

## Credit rationale

Credits `activateNewScale`, `restoreScaleAndLoc`, `addMark`, `findMinMaxBoundaries`, `calcMinMaxMtoF`, `ClipRotatedExtents`, `ScrollParchment`, and `changeWindowLoc` from `am_map.cpp`.
