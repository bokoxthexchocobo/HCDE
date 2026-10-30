# Chunk 1257 — Fog-boundary span walk and the colormap split from r_fogboundary.cpp (1-157).

## Scope

Fog-boundary span walk and the colormap split from r_fogboundary.cpp (1-157).

## Native ranges

- `src/rendering/swrenderer/line/r_fogboundary.cpp` **Render** 1–157 (157 lines)

**Credited native LOC:** 157 (tally `nativeLoc` 157).

## Managed

`VmFogBoundary1257Slice.cs`
Managed LOC (tally): 163.
Conversion commit: `a37e640923ad2a5d00cadb0ea09f624f78364196`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Render (**54–148**) | yes |
| RenderSection (**150–156**) | yes |
| Band hull (**1–157**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Light level to shade | host step |
| Drawer light and the fog row | host steps |

The walk starts at the right column. A taller span is filled with that column. Each step left subtracts the light step and recomputes the colormap. Column 0 is the identity map, so a change away from it does not draw. A change away from any other column draws the open rows, then grows the span and fills it. While the colormap stays off the identity map, a row that just closed is drawn from the stored right edge, and a row that just opened is marked. While it stays on the identity map, the open span shrinks and nothing is drawn. The rows still open at the left edge are drawn only when that last colormap is not the identity map.

| Managed (`VmFogBoundary1257Slice.cs`) | Native (`r_fogboundary.cpp`) |
|----------------------------------------|------------------------------|
| Span walk and the section draw | **1–157** |
| Band hull (exclusive slice) | **1–157** |

### Chunk **1257** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **157** |
| **Net headline** | **+157** vs post-**1256** |


**Post-review ratio:** 163/157 managed/native ≈ 104% (src file LOC at HEAD; see tally).

## Tests

- `VmFogBoundary1257SliceTests.Split_SkipsTheIdentityMapAndDrawsOnAColormapChange`
- `VmFogBoundary1257SliceTests.Span_DrawsAClosedRowAndClampsTheLookup`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1257 --to 1257` (0 hard / 0 soft).

## Seams

The wall shade is a host step. Drawing a fog row and storing the drawer light are host steps. The colormap index is the call-site clamp: visibility is capped at 24, converted with half-even, subtracted from the shade, shifted 16 bits, and clamped to 0..31.

## Credit rationale

UNCredited r_fogboundary.cpp 1-157 (+157). A full tally line scan shows no overlap. The file ends at line 157.
