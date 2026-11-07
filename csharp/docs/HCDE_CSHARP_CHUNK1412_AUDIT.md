# Chunk 1412 — The rest of D_Display, from the frame time through the cycle unclock, in d_main.cpp (1392-1610).

## Scope

The rest of D_Display, from the frame time through the cycle unclock, in d_main.cpp (1392-1610).

## Native ranges

- `src/d_main.cpp` **D_Display** 1392–1610 (219 lines)

**Credited native LOC:** 219 (tally `nativeLoc` 219).

## Managed

`VmDMain1412Slice.cs`
Managed LOC (tally): 293.
Conversion commit: `024ff8447b7a527f10ade4c86209339f331b210e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Frame preamble (**1393–1397**) | yes |
| World render (**1398–1428**) | yes |
| World HUD (**1430–1473**) | yes |
| Console, demo, and cutscene switch (**1475–1498**) | yes |
| Chat, pause, icon, and sound debug (**1500–1581**) | yes |
| Direct update, wipe, and the cycle unclock (**1583–1610**) | yes |
| Band hull (**1392–1610**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| D_Display wipe-start choice | ends at line 1391 |
| D_ErrorCleanup | starts at line 1619 |
| The commented E_RenderFrame call | stays compiled out |
| I_msTimeFS, texture animation, sky, D_Render, RenderView, status-bar draws, page, console, menu, break-lines, PerformWipe, DrawOverlays, End2DAndUpdate | callees |

The frame stores the millisecond time, advances texture animation with that time, updates the sky with the time fraction, begins the frame, and clears the 2D clip. A level or title level with a non-zero tic renders through D_Render with interpolation on, then begins 2D. The world and post-view traces use the existing startup window and the current frame, and only when that frame is a multiple of 10. A toggled HUD skips the blend, the automap, and the status bar, but the render still runs. The automap bottom is the view height when the alt HUD is on and the view is full-screen, and the status-bar top otherwise. The border refreshes when the automap is off or the view is active. The alt HUD is chosen before the fullscreen HUD, and only when the view is full-screen and the screen blocks are above 10. The fullscreen path uses the fullscreen state when the full HUD is on and the none state otherwise. Every other combination uses the status bar. The alt HUD draws its own layer when the full HUD is on or the automap is up, and the crosshair only when the camera actor has a player and the automap is off. The fullscreen console draws the page, the console, and the menu, ends 2D, and returns before the chat, the icon, the wipe, and the cycle unclock. The demo screen draws the page and continues. A cutscene or intro draws the screen job and continues. Every other state falls through. Chat, the pause pic, the icon, and the sound debug run only while the HUD is visible. The pause pic requires a pause or the extended pause, a closed menu, and a status bar. The paused virtual receives paused minus one, and a non-zero result skips the picture. The string path starts at y 4 and advances by the big-font height times the clean factor. The sign path uses the screen width. The by-line is drawn only when paused is non-zero and the game is multiplayer, with the new small font when the generic UI is on. An icon clears its name before the texture check, draws a valid patch at the 320 by 200 center, and sets the wipe block to 10 even when the patch is missing. A direct present releases a captured wipe image, draws the overlays, and ends 2D. The wipe type is read only when a wipe image exists and the wipe block is not negative. Otherwise PerformWipe runs with sound left on, and the overlays are the wipe drawer. The cycle counter is unclocked only when the function does not return from the fullscreen console.

| Managed (`VmDMain1412Slice.cs`) | Native (`d_main.cpp`) |
|----------------------------------|----------------------|
| Display present, pause, and wipe | **1392–1610** |
| Band hull (exclusive slice) | **1392–1610** |

### Chunk **1412** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **219** |
| **Net headline** | **+219** vs the prior chunk |


**Post-review ratio:** 293/219 managed/native ≈ 134% (src file LOC at HEAD; see tally).

## Tests

- `VmDMain1412SliceTests.WorldBranchChoosesTheHudAndSkipsAZeroTic`
- `VmDMain1412SliceTests.ScreensPauseIconAndWipe`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1412 --to 1412` (0 hard / 0 soft).

## Seams

Frame time, animation, the sky, the render wrapper, status-bar draws, menus, and the wipe stay callees. D_ErrorCleanup starts at line 1619.

## Credit rationale

UNCredited d_main.cpp 1392-1610 (+219). This is the remainder of D_Display after the wipe-start choice at line 1391. The function closes at line 1610. hq4x and hq4x_asm stay later because they are generated tables. The band is not sliced further.
