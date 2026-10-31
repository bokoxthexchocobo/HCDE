# Chunk 1272 — Striped wall bands and the normal wall drawer setup from r_walldraw.cpp (1-226).

## Scope

Striped wall bands and the normal wall drawer setup from r_walldraw.cpp (1-226).

## Native ranges

- `src/rendering/swrenderer/line/r_walldraw.cpp` **RenderWallPart** 1–226 (226 lines)

**Credited native LOC:** 226 (tally `nativeLoc` 226).

## Managed

`VmWall1272Slice.cs`
Managed LOC (tally): 182.
Conversion commit: `464a135fc3d3670d56cf053ea432c7555bf13a4e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| RenderWallPart (**56–59**) | yes |
| Render (**61–91**) | yes |
| ProcessStripedWall (**93–119**) | yes |
| ProcessNormalWall (**121–205**) | yes |
| GetLightList (**207–225**) | yes |
| Band hull (**1–226**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Plane projection | host wall setup |
| Texture pixels | host picture |

A missing picture returns before any setup. A missing thread faults. A fixed colormap, a fixed light level at or above zero, or an empty light list draws one ordinary wall. Otherwise each light plane is visited. A plane culled above the wall does not draw and does not swap the ping-pong rows, but it still installs its colormap. A visible plane writes max of the bottom and the minimum of the projected row and the current top, draws that band with the colormap already installed, and then swaps the two scratch rows. The colormap for a plane is installed after its band, so the first band keeps the base colormap and the closing band uses the last plane. The closing band draws down to the original bottom. An ordinary wall promotes an unmasked, non-additive, fully opaque masked request to an opaque style. A lower alpha, a mask request that is false, an additive blend, or a masked picture keeps the caller style. A fixed light level wins over a fixed colormap. Ignoring the sector color selects the fullbright colormap on that path. Otherwise the fixed colormap is used, and with neither the light varies and can be foggy. Paletted frac bits are 32 minus the height bits, and a one-pixel texture stores zero. A BGRA texture stores 32 frac bits and mipmaps only when both the cvar and the picture ask for them. The top and bottom copies share one buffer of twice the span, addressed minus x1. A fixed light or a fixed colormap drops the dynamic light list. Otherwise the list is kept only when the sidedef index is inside it.

| Managed (`VmWall1272Slice.cs`) | Native (`r_walldraw.cpp`) |
|--------------------------------|---------------------------|
| Stripes, style, and clip copy | **1–226** |
| Band hull (exclusive slice) | **1–226** |

### Chunk **1272** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **226** |
| **Net headline** | **+226** vs post-**1271** |


**Post-review ratio:** 182/226 managed/native ≈ 81% (src file LOC at HEAD; see tally).

## Tests

- `VmWall1272SliceTests.Stripes_PingPongTheBandsAndKeepTheLastColormap`
- `VmWall1272SliceTests.Styles_PromoteOpaqueAndPreferAFixedLight`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1272 --to 1272` (0 hard / 0 soft).

## Seams

The projected light planes, the texture, and the viewport are host inputs. The slice keeps the band clamp, the colormap delay, the shared clip allocation, and the fixed-light priority.

## Credit rationale

UNCredited r_walldraw.cpp 1-226 (+226). A full tally line scan shows no overlap. The file ends at line 226.
