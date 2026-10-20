# Chunk 365 — Automap drawGrid block-aligned lines and AM_FakeFlat heightsec routing

## Scope

Automap drawGrid block-aligned lines and AM_FakeFlat heightsec routing

## Native ranges

- `src/am_map.cpp` **DAutomap::drawGrid** 1877–1936 (60 lines)
- `src/am_map.cpp` **AM_FakeFlat** 1947–2054 (108 lines)

**Credited native LOC:** 168 (tally `nativeLoc` 168).

## Managed

`VmAutomapFakeFlat.cs`, `VmAutomapFakeFlatTests.cs`, `VmAutomapGridDraw.cs`, `VmAutomapGridDrawTests.cs`
Managed LOC (tally): 232.
Conversion commit: `b35a4ff801293e8c996ad84d709d92d3a5e64ac2`.

## Tests

- `VmAutomapGridDrawTests.ShouldRotateGridLines_Rotate2RequiresViewActive`
- `VmAutomapGridDrawTests.EnumerateVerticalGridLines_StepsByMapBlockUnits`
- `VmAutomapGridDrawTests.EnumerateHorizontalGridLines_ProducesConstantY`
- `VmAutomapGridDrawTests.AlignGridStart_SnapsToBlockmapGrid`
- `VmAutomapFakeFlatTests.ShouldPassThrough_WithoutHeightSec`
- `VmAutomapFakeFlatTests.ResolveViewerArea_BelowFloor_IsNegativeOne`
- `VmAutomapFakeFlatTests.PlanFakeFlat_FakeFloorOnly_AboveUsesOriginal`
- `VmAutomapFakeFlatTests.PickFloorTextureSource_Above_NonSkyUsesHeightSecFloor`
- `VmAutomapFakeFlatTests.ShouldApplyFakeLight_RespectsNoFakeLightFlag`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 365 --to 365` (0 hard / 0 soft).

## Seams

`rotatePoint`, `drawMline`, and live `sector_t` mutation are not hosted; managed code mirrors grid iteration math and fake-flat area/texture policy.

## Credit rationale

Credits `DAutomap::drawGrid` and free function `AM_FakeFlat` from `am_map.cpp`.
