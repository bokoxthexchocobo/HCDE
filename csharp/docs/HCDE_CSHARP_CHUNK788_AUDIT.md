# Chunk 788 — `Serialize(secspecial_t)`, plane copy, flat align, texture replace, poly BSP hook, line adjust, side lighting, and vertex heights (`p_sectors.cpp` 1370–1672).

## Scope

`Serialize(secspecial_t)`, plane copy, flat align, texture replace, poly BSP hook, line adjust, side lighting, and vertex heights (`p_sectors.cpp` 1370–1672).

## Native ranges

- `src/playsim/p_sectors.cpp` **Serialize(secspecial_t)** 1370–1383 (14 lines)
- `src/playsim/p_sectors.cpp` **secplane_t::CopyPlaneIfValid** 1392–1420 (29 lines)
- `src/playsim/p_sectors.cpp` **FLevelLocals::AlignFlat** 1428–1448 (21 lines)
- `src/playsim/p_sectors.cpp` **FLevelLocals::ReplaceTextures** 1458–1495 (38 lines)
- `src/playsim/p_sectors.cpp` **subsector_t::BuildPolyBSP** 1505–1539 (35 lines)
- `src/playsim/p_sectors.cpp` **line_t::AdjustLine** 1547–1572 (26 lines)
- `src/playsim/p_sectors.cpp` **side_t::GetLightLevel** 1580–1635 (56 lines)
- `src/playsim/p_sectors.cpp` **vertex_t::RecalcVertexHeights** 1643–1672 (30 lines)

**Credited native LOC:** 249 (tally `nativeLoc` 249).

## Managed

`VmPSectorSerializeAlign.cs`
Managed LOC (tally): 482.
Conversion commit: `ab591396bac6f3ecb3d4ffb802a5446f8eb36caf`.

## Padding review (2026-10-21)

| Managed | Native (`p_sectors.cpp`) |
|---------|---------------------------|
| `SerializeSectorSpecial` / `ReadSectorSpecial` | `Serialize(secspecial_t)` **1370–1383** |
| `CopyPlaneIfValid` | `secplane_t::CopyPlaneIfValid` **1392–1420** |
| `AlignFlat` | `FLevelLocals::AlignFlat` **1428–1448** |
| `ReplaceTextures` | `FLevelLocals::ReplaceTextures` **1458–1495** |
| `BuildPolyBsp` | `subsector_t::BuildPolyBSP` **1505–1539** |
| `AdjustLine` | `line_t::AdjustLine` **1547–1572** |
| `GetLightLevel` | `side_t::GetLightLevel` **1580–1635** |
| `RecalcVertexHeights` | `vertex_t::RecalcVertexHeights` **1643–1672** |


**Post-review ratio:** 482/249 managed/native ≈ 194% (src file LOC at HEAD; see tally).

## Tests

- `VmPSectorSerializeAlignTests.SerializeSectorSpecial_RoundTripsFields`
- `VmPSectorSerializeAlignTests.CopyPlaneIfValid_CopiesWhenNormalsDiffer`
- `VmPSectorSerializeAlignTests.AlignFlat_SetsSectorBaseFromFrontSide`
- `VmPSectorSerializeAlignTests.ReplaceTextures_SwapsMatchingWallTier`
- `VmPSectorSerializeAlignTests.BuildPolyBsp_InvokesNodeBuilderHooks`
- `VmPSectorSerializeAlignTests.AdjustLine_FillsBoundingBox`
- `VmPSectorSerializeAlignTests.GetLightLevel_AppliesHorizFakeContrast`
- `VmPSectorSerializeAlignTests.RecalcVertexHeights_SortsDistinctHeights`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 788 --to 788` (0 hard / 0 soft).

## Seams

`FNodeBuilder` / `PolyNodeBuilder`, `TexMan`, and live `secplane_t::Normal` math are injected or stubbed.

## Credit rationale

Whole-function sector maintenance band after glow/overlap chunk784.
