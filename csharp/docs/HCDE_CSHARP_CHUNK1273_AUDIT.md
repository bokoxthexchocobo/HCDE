# Chunk 1273 — Decal walk, relative wall Z, and the clipped sprite draw from r_decal.cpp (1-255).

## Scope

Decal walk, relative wall Z, and the clipped sprite draw from r_decal.cpp (1-255).

## Native ranges

- `src/rendering/swrenderer/things/r_decal.cpp` **RenderDecal** 1–255 (255 lines)

**Credited native LOC:** 255 (tally `nativeLoc` 255).

## Managed

`VmDecal1273Slice.cs`
Managed LOC (tally): 249.
Conversion commit: `0048f5a122429f249605f891eaa05d203ffb1e89`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| RenderDecals (**57–63**) | yes |
| Early return (**76–77**) | yes |
| Relative Z (**79–122**) | yes |
| Texture reject (**124–128**) | yes |
| Edges and projection (**130–155**) | yes |
| Draw-segment gate (**160–181**) | yes |
| Tier switch (**182–219**) | yes |
| Fade and draw loop (**222–253**) | yes |
| Band hull (**1–255**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Wall projection | host wall setup |
| Texture lookup | host picture |

An invisible decal, an inactive view, or an invalid picture returns before Z is used. A missing picture returns after Z and before the wall edges. The edges use the linedef direction, not a decal angle. A zero-length seg contributes a zero unit vector, so both edges sit on the decal point minus the view. A missing thread faults at projection. A rejected projection returns. A screen span that starts at or past the clipper's right, or ends at or before the clipper's left, returns. A draw-segment pass keeps only mid and full clip modes and always uses the wall rows. The full-height sprite clip stays unused. Any other pass returns on mid clip. A one-sided full clip uses the wall rows once. A two-sided full clip uses the wall top against the ceiling clip, then draws again with the floor clip against the wall bottom, and stops. An upper clip uses the top tier and the ceiling clip. A lower clip uses the bottom tier and the floor clip. The default clip arm returns. Mid clip takes its front planes from the decal sector. Upper and lower relative Z follow the peg flags, and a missing back sector uses the front planes. An additive style with a nonzero fade replaces the colormap with a black fade. A fade of zero, or any other style, keeps the base colormap. The drawer receives the float-narrowed alpha, the flip bits, and the fullbright bit. A hidden style still lets the next decal draw.

| Managed (`VmDecal1273Slice.cs`) | Native (`r_decal.cpp`) |
|---------------------------------|------------------------|
| Walk, Z, clips, and the repeat | **1–255** |
| Band hull (exclusive slice) | **1–255** |

### Chunk **1273** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **255** |
| **Net headline** | **+255** vs post-**1272** |


**Post-review ratio:** 249/255 managed/native ≈ 98% (src file LOC at HEAD; see tally).

## Tests

- `VmDecal1273SliceTests.Heights_UseThePegAndTheMidSector`
- `VmDecal1273SliceTests.Clips_RepeatOnlyTheTwoSidedFullWall`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1273 --to 1273` (0 hard / 0 soft).

## Seams

Wall projection, the texture lookup, and the drawer are host inputs. The slice keeps the pegged Z switch, the draw-segment gate, the two-sided repeat, and the additive fade swap.

## Credit rationale

UNCredited r_decal.cpp 1-255 (+255). A full tally line scan shows no overlap. The file ends at line 255.
