# Chunk 364 — Automap framebuffer clear policy, Cohen-Sutherland clipMline, drawMline thickness/AA plan

## Scope

Automap framebuffer clear policy, Cohen-Sutherland clipMline, drawMline thickness/AA plan

## Native ranges

- `src/am_map.cpp` **DAutomap::clearFB** 1618–1653 (36 lines)
- `src/am_map.cpp` **DAutomap::clipMline** 1666–1785 (120 lines)
- `src/am_map.cpp` **DAutomap::drawMline** 1793–1864 (72 lines)

**Credited native LOC:** 228 (tally `nativeLoc` 228).

## Managed

`VmAutomapDrawGeometry.cs`, `VmAutomapDrawGeometryTests.cs`
Managed LOC (tally): 335.
Conversion commit: `e8c45bcd831b018e763e3bd3f965187a8e511595`.

## Tests

- `VmAutomapDrawGeometryTests.ResolveFramebufferClearMode_Mode2_RavenColorset_UsesBackground`
- `VmAutomapDrawGeometryTests.ResolveFramebufferClearMode_Mode2_CustomMod_UsesBackground`
- `VmAutomapDrawGeometryTests.ResolveFramebufferClearMode_Mode2_NoQualifier_Solid`
- `VmAutomapDrawGeometryTests.TryClipMapLine_FullyInside_ReturnsFramePoints`
- `VmAutomapDrawGeometryTests.TryClipMapLine_TrivialOutsideY_Rejects`
- `VmAutomapDrawGeometryTests.TryClipMapLine_CrossingLeftEdge_Clips`
- `VmAutomapDrawGeometryTests.PlanDrawLine_Antialiasing_Thick_UsesAaDivideThree`
- `VmAutomapDrawGeometryTests.PlanDrawLine_NoAa_UsesThickWhenThicknessAtLeastTwo`
- `VmAutomapDrawGeometryTests.PlanDrawLine_ClippedAway_ReturnsDefault`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 364 --to 364` (0 hard / 0 soft).

## Seams

`twod` draw calls, `TexMan`, and live `DAutomap` member state are not hosted; managed code mirrors backdrop gating, map-to-frame clipping, and line draw parameters.

## Credit rationale

Credits `DAutomap::clearFB`, `clipMline`, and the clipped `drawMline` thickness/antialiasing policy from `am_map.cpp`.
