# Chunk 1410 — GetCaps, DrawPaletteTester, CalcFps, the fps stat, DrawRateStuff, DrawHCDELagHUD, DrawOverlays, and End2DAndUpdate from d_main.cpp (914-1182).

## Scope

GetCaps, DrawPaletteTester, CalcFps, the fps stat, DrawRateStuff, DrawHCDELagHUD, DrawOverlays, and End2DAndUpdate from d_main.cpp (914-1182).

## Native ranges

- `src/d_main.cpp` **GetCaps** 914–1182 (269 lines)

**Credited native LOC:** 269 (tally `nativeLoc` 269).

## Managed

`VmDMain1410Slice.cs`
Managed LOC (tally): 261.
Conversion commit: `e37087f19e3f42d95acda2741f0031844bcfb065`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| GetCaps (**974–1000**) | yes |
| DrawPaletteTester (**1008–1029**) | yes |
| CalcFps (**1041–1060**) | yes |
| fps stat (**1062–1066**) | yes |
| DrawRateStuff (**1068–1111**) | yes |
| DrawHCDELagHUD (**1113–1164**) | yes |
| DrawOverlays (**1166–1174**) | yes |
| End2DAndUpdate (**1176–1182**) | yes |
| Band hull (**914–1182**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| D_Display | starts at line 1192 |
| V_IsHardwareRenderer, V_IsTrueColor, screen draws, I_GetTime, Net_GetLagHUDMetrics, CheckBench | callees |

 GetCaps uses the unclipped texture flag for a software renderer, plus true color or a colormap. A hardware renderer starts from the flat-sprite, model, slope, tilt, roll, material, post, brightmap, and polygonal flags, and adds true color unless the tone map is 5. Voxels are added for either renderer. DrawPaletteTester divides the height by 50 and draws a 16 by 16 grid. A palette number above 1 copies that translation. A number of 1 or below reads the base color and does not store it. CalcFps divides the millisecond delta by the time scale and ignores a non-positive truncated result. A new second stores the integer frame count divided by the elapsed seconds, times the time scale, then counts the current frame in the next window. The fps stat formats the millisecond count and the fps. DrawRateStuff draws that text only when vid_fps is set, then the ticker dots (at most 20 lit dots, the rest cleared), then the palette tester when the palette cvar is non-zero. The lag HUD is skipped when it should not draw. Red wins when the tic gate is stalled or the backlog is above 35. Gold wins when the backlog is above twice the tic dup. Otherwise the line is green. The invasion line uses the idle sentence only for the exact word disabled. DrawOverlays draws the console, the menu, the rate block, and the lag HUD, and prints the stat unless the HUD is toggled off. End2DAndUpdate ends the 2D pass, runs the bench, updates the screen, and marks the frame done. A bench failure skips the update.

| Managed (`VmDMain1410Slice.cs`) | Native (`d_main.cpp`) |
|----------------------------------|----------------------|
| Caps, fps, lag HUD, and the overlay close | **914–1182** |
| Band hull (exclusive slice) | **914–1182** |

### Chunk **1410** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **269** |
| **Net headline** | **+269** vs the prior chunk |


**Post-review ratio:** 261/269 managed/native ≈ 97% (src file LOC at HEAD; see tally).

## Tests

- `VmDMain1410SliceTests.CapsPaletteAndFpsMatchTheRendererAndTheSecondBoundary`
- `VmDMain1410SliceTests.RateTickerLagAndOverlayOrder`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1410 --to 1410` (0 hard / 0 soft).

## Seams

Screen scale, font width, the clock, and the bench check stay callees. D_Display starts at line 1192.

## Credit rationale

UNCredited d_main.cpp 914-1182 (+269). This continues the d_main.cpp hole after dmflags and compatmode. The compat flag registrations sit in front of GetCaps and stay inside this contiguous slice. hq4x and hq4x_asm stay later because they are generated tables. The band is not sliced further.
