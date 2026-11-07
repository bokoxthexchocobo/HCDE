# Chunk 1411 — The first half of D_Display, through the wipe-start choice, from d_main.cpp (1192-1391).

## Scope

The first half of D_Display, through the wipe-start choice, from d_main.cpp (1192-1391).

## Native ranges

- `src/d_main.cpp` **D_Display** 1192–1391 (200 lines)

**Credited native LOC:** 200 (tally `nativeLoc` 200).

## Managed

`VmDMain1411Slice.cs`
Managed LOC (tally): 267.
Conversion commit: `a36f3a2533ee48f152cc7ed4275ce99d284cc89d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| D_Display locals and the collector (**1192–1200**) | yes |
| Early return (**1202–1203**) | yes |
| Startup trace, focus, and activation (**1205–1272**) | yes |
| Inactive block (**1274–1291**) | yes |
| Passed-gate trace (**1293–1303**) | yes |
| Cycle clock and renderer caps (**1305–1326**) | yes |
| Mode change and view size (**1328–1349**) | yes |
| Wipe-start choice (**1351–1391**) | yes |
| Band hull (**1192–1391**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| d_main.cpp **1183–1191** | comment banner before D_Display |
| D_Display frame draw | starts at line 1392 and runs through line 1610 |
| GC::CheckGC, SetDeltaTime, I_ForceWindowFocus, GetCaps, GetFOV, ToggleFullscreen, V_OutputResized, R_ExecuteSetViewSize, CanWipe, WipeStartScreen | callees |

D_Display returns before any static startup state when drawers are disabled, the screen is missing, or either dedicated-server check is set. The collector and the time step still run. The startup window is the trace frame below 350. A pre-gate line is written when that frame is a multiple of 5, using the frame value from before this call increments it. The first frame forces window focus once. A frame that has not yet seen the application active, and is currently inactive, forces focus and marks the application active. Seeing the application active is recorded after that promotion, so the waiting-activation branch does not run on the same call. The trace frame advances while it is inside the startup window. An inactive user game then returns, after at most 12 blocked lines, unless a mode change is pending, background play is allowed, the game is waiting for its first activation, or the state is startup, the title level, or the demo screen. The passed-gate line uses the frame after the increment and the window flag from before it. The cycle counter is clocked only after those gates. Renderer caps are stored from GetCaps. A missing console-player camera is replaced with that player's actor. An active view uses that actor's field of view at the current fraction, or 90 degrees when the actor is missing. A requested mode change clears the request, toggles fullscreen from the cvar, then records the size after that toggle. A requested view-size change copies the screen size and clears the request when there is no status bar. With a status bar it calls the view-size executor and leaves the request set. A net game, a non-zero wipe block, or a cutscene that cannot wipe clears the wipe start, copies the game state, and decrements a positive wipe block. Otherwise a game state other than the fullscreen console and the title level, and different from the wipe state, captures a start screen when VR mode is 0 or the renderer is not hardware mode 4. The wipe type is fade, burn, or melt for the three forced states, and the wipe cvar for every other previous state, including the generic force. The wipe state is then copied even when VR mode suppressed the capture. The same state, the fullscreen console, and the title level leave the wipe start empty.

| Managed (`VmDMain1411Slice.cs`) | Native (`d_main.cpp`) |
|----------------------------------|----------------------|
| Display gate and the wipe-start choice | **1192–1391** |
| Band hull (exclusive slice) | **1192–1391** |

### Chunk **1411** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **200** |
| **Net headline** | **+200** vs the prior chunk |


**Post-review ratio:** 267/200 managed/native ≈ 134% (src file LOC at HEAD; see tally).

## Tests

- `VmDMain1411SliceTests.SkipFocusAndInactiveBlock`
- `VmDMain1411SliceTests.CameraSizeAndWipeChoice`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1411 --to 1411` (0 hard / 0 soft).

## Seams

The collector, the time step, focus, field of view, the mode change, the view-size executor, and the wipe-start capture stay callees. The rest of D_Display starts at line 1392.

## Credit rationale

UNCredited d_main.cpp 1192-1391 (+200). D_Display runs from line 1192 through line 1610, which is 419 lines, so this slice stops at the wipe-start else. The frame draw stays for the following slice. hq4x and hq4x_asm stay later because they are generated tables. The band is not sliced further.
