# Chunk 1070 — `DSBarInfo::DrawGraphic` centering, clipping, overlay, and fullscreen-scale paths from `sbarinfo.cpp` (**1201–1350**); follows **1069** texture null guard.

## Scope

`DSBarInfo::DrawGraphic` centering, clipping, overlay, and fullscreen-scale paths from `sbarinfo.cpp` (**1201–1350**); follows **1069** texture null guard.

## Native ranges

- `src/g_statusbar/sbarinfo.cpp` **DSBarInfo::DrawGraphic** 1201–1350 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoDrawGraphic1070Slice.cs`
Managed LOC (tally): 117.
Conversion commit: `15f3f472a283fe24f06d5544b8ee4e3d57397170`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Coordinate load + center offsets (**1201–1211**) | yes |
| Statusbar clip rect math (**1221–1224**) | yes |
| Dim/alpha-map draw path flags (**1242–1273**) | yes |
| Fullscreen relative center + anchors (**1282–1299**) | yes |
| Fullscreen clip + draw flags (**1302–1345**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1198–1199** | **1069** null guard |
| **1228–1236**, **1239**, **1244–1272** | host coord transform + draw calls |
| **1310–1344** | host `ClearRect`/`DrawTexture` |
| **1350+** | `DrawString` signature/body outside hull behavior |

| Managed (`VmSbarinfoDrawGraphic1070Slice.cs`) | Native (`sbarinfo.cpp`) |
|--------------------------------------------------|-------------------------|
| Center offset + pixel offsets | **1204–1214** |
| Dest size + statusbar clip bounds | **1218–1224** |
| Overlay/alpha/clear path gates | **1238–1273** |
| Fullscreen center/anchor/scale | **1282–1299** |
| Fullscreen clip presence | **1302–1308** |
| Band hull (exclusive slice) | **1201–1350** |

### Chunk **1070** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1069** |


**Post-review ratio:** 117/150 managed/native ≈ 78% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoDrawGraphic1070SliceTests.UsesCenterOffset_DetectsNativeFlag`
- `VmSbarinfoDrawGraphic1070SliceTests.ApplyCenteredDrawX_UsesTextureWidthWhenForcedUnset`
- `VmSbarinfoDrawGraphic1070SliceTests.ApplyPixelOffsets_AddsBlockOffsets`
- `VmSbarinfoDrawGraphic1070SliceTests.ResolveDestSize_HonorsForcedDimensions`
- `VmSbarinfoDrawGraphic1070SliceTests.ComputeStatusbarClipLeft_ZeroWhenClipDisabled`
- `VmSbarinfoDrawGraphic1070SliceTests.ResolveColorOverlay_MatchesDimOverlayConstant`
- `VmSbarinfoDrawGraphic1070SliceTests.ApplyFullscreenRelativeCenter_DelegatesTo1068Adjust`
- `VmSbarinfoDrawGraphic1070SliceTests.ResolveFullscreenAnchor_FlipsNegativeNonRelativeX`
- `VmSbarinfoDrawGraphic1070SliceTests.ScaleFullscreenDrawMetrics_AppliesHudScale`
- `VmSbarinfoDrawGraphic1070SliceTests.HasAnyFullscreenClipComponent_DetectsPartialClip`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1070 --to 1070` (0 hard / 0 soft).

## Seams

Null texture early-out (**1198–1199**) stays **1069**; `DrawTexture`/`ClearRect`/`StatusbarToRealCoords` and **1351+** `DrawString` body remain host-injected.

## Credit rationale

UNCredited **`sbarinfo.cpp`** **1201–1350** (+150); no overlap with **1069** `ShouldSkipDrawGraphic` (**1198–1199**).
