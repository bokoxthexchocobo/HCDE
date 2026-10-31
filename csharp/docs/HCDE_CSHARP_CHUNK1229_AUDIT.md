# Chunk 1229 — BOOTLOGO blit, accent progress notches, and the 8-line console overlay from startscreen_generic.cpp (1-184).

## Scope

BOOTLOGO blit, accent progress notches, and the 8-line console overlay from startscreen_generic.cpp (1-184).

## Native ranges

- `src/common/startscreen/startscreen_generic.cpp` **CreateGenericStartScreen** 1–184 (184 lines)

**Credited native LOC:** 184 (tally `nativeLoc` 184).

## Managed

`VmStartScreen1229Slice.cs`
Managed LOC (tally): 196.
Conversion commit: `175dfd4413f0225a257aeef03489b98fde0d2de2`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| BOOTLOGO blit (**68–90**) | yes |
| Accent notches (**100–129**) | yes |
| Status lines (**131–145**) | yes |
| Console overlay (**147–176**) | yes |
| Factory (**179–184**) | yes |
| Band hull (**1–184**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

A logo that is wide but short is centered. A notch that would shrink after the budget stretch stays where it was painted. The last eight console lines are the ones drawn.

| Managed (`VmStartScreen1229Slice.cs`) | Native (`startscreen_generic.cpp`) |
|----------------------------------------|-------------------------------------|
| Blit, notches, status, and overlay | **1–184** |
| Band hull (exclusive slice) | **1–184** |

### Chunk **1229** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **184** |
| **Net headline** | **+184** vs post-**1228** |


**Post-review ratio:** 196/184 managed/native ≈ 107% (src file LOC at HEAD; see tally).

## Tests

- `VmStartScreen1229SliceTests.Create_CentersASmallLogoAndPaintsTheNotch`
- `VmStartScreen1229SliceTests.Create_HoldsTheBarAndDrawsTheLastConsoleLines`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1229 --to 1229` (0 hard / 0 soft).

## Seams

BOOTLOGO is looked up in namespace 15. The bitmap is 1280 by 960 and cleared opaque black. A missing lump skips the image. A null image is recorded and not blitted. Either side under 1280 or 960 centers the bitmap at (640 - width/2, 440 - height/2); both sides at least that size crop from the origin. The accent is fetched once while the latch is zero and the bar alpha is still nonzero, then stored as BGRA bytes. A zero accent clears the alpha and blocks a later fetch. DoProgress applies the base budget stretch when advance is 1 and the cursor is inside the headroom, clamps CurPos to MaxPos, then paints a notch only when the new 400-notch position is past the previous one. The bar is 8 pixels tall at (440, 930). Hardware data is dropped only when a texture exists. Status lines print the message; an empty loading phase does not replace the phase text. The overlay returns false without a buffer, a font, lines, or a line count. Otherwise it clears a panel of fontHeight*8+8 and draws the last eight lines in light gray on black at 180 alpha. Create begins the countdown after the bitmap is built.

## Credit rationale

UNCredited startscreen_generic.cpp 1-184 (+184). The file ends at 184. A full tally line scan shows no overlap.
