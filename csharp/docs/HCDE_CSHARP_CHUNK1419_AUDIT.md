# Chunk 1419 — Capture clamp, mouse dispatch, viewport math, and crash text in d_main.cpp (3321-3642).

## Scope

Capture clamp, mouse dispatch, viewport math, and crash text in d_main.cpp (3321-3642).

## Native ranges

- `src/d_main.cpp` **mouse_capturemode** 3321–3642 (322 lines)

**Credited native LOC:** 322 (tally `nativeLoc` 322).

## Managed

`VmDMain1419Slice.cs`
Managed LOC (tally): 464.
Conversion commit: `5119623ab334ffa54bad48c28f1305c1090389d8`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| mouse_capturemode handler (**3321–3331**) | yes |
| vm_debug handler (**3333–3347**) | yes |
| vm_debug_port declaration (**3349**) | yes |
| Mlook_ReleaseHandler (**3351–3357**) | yes |
| StrTable_ValidFilter (**3359–3363**) | yes |
| System_WantGuiCapture (**3365–3383**) | yes |
| System_WantLeftButton (**3385–3388**) | yes |
| System_DispatchEvent (**3390–3430**) | yes |
| System_NetGame through System_PlayStartupSound (**3432–3460**) | yes |
| System_IsSpecialUI through System_OnScreenSizeChanged (**3462–3488**) | yes |
| System_GetSceneRect (**3490–3511**) | yes |
| System_GetLocationDescription through System_LanguageChanged (**3513–3541**) | yes |
| AppendCrashText and System_CrashInfo (**3551–3642**) | yes |
| Band hull (**3321–3642**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| DoomButtons | ends at line 3316 |
| lookspring and language declarations | lines 3318 and 3319 |
| System_M_Dim | declared at line 3644 |
| UI, sound, texture, automap, and debug-trace bodies | callees |

mouse_capturemode clamps a value below 0 to 0 and a value above 2 to 2. vm_debug, when cleared, stops a live debug server and drops it. When set, it prints the HCDE restart line and the JIT line. The debug port default is 19021. Mlook release writes center-view command 36 only when lookspring is set. A string filter matches strifeteaser for shareware Strife before it accepts game type 0 or a case-insensitive game name. A null name or a null string is a null dereference. GUI capture is true for a down or falling console or for chat while the menu is off, and for the on and no-pause menus otherwise. A waiting menu stays false unless a UI processor claims it. A missing level crashes that processor call. The left button is wanted on the demo screen and the title level. Mouse dispatch always records the event. The look and turn block runs only for a mouse event while the menu is off, the console is not down or falling, the responder did not take it, and the game is not paused. Look uses the raw axes, truncates y times pitch times 16 toward zero, and traces only in a net game with a positive movement debug value and a non-zero look. Turn does the same for x unless strafe or lookstrafe is set. Both axes landing on zero returns true. Net game and the native-mouse flag are returned directly. A missing level crashes the native-mouse query. In-game capture is false during demo playback or pause. Mode 0 and every other mode except 1 and 2 require the level. Mode 1 also accepts a cutscene. Mode 2 is always on. Startup sound uses body channel 4, the UI flag, and either rumble or no-rumble, at full volume with no attenuation. Special UI is generic UI, a log or dialog font, a console that is not up, multiplayer, or an open non-conversation menu. Texture filtering is disabled when the renderer is not hardware. Anisotropic filtering follows the Intel mipmap flag. A screen-size change notifies the status bar, reloads the crosshair, and refreshes the automap when a level has one. The scene rect uses the full screen at 10 or more blocks. Below that, height is the block count times the screen height over 10 with the low 3 bits cleared, and the unused block width is the same ratio of the screen width. The returned width is the view width. A missing view level yields an empty location. Otherwise the location uses the map, the level name, four-decimal coordinates, and the fps. Player name is the indexed name. Console toggle counts only a falling console while the hud toggle is set. Language change copies a level lookup when one exists and always updates the window title. Crash text refuses a null buffer or a zero length, clears the first byte, and returns when fewer than 3 bytes are available. The end pointer reserves two bytes. Append writes the formatted text, treats a null format as empty, and on a short buffer keeps the end pointer and a terminator. A negative format result clears the cursor and does not advance. The report is the HCDE version line, the command line, each wad, then either not-in-a-level or the current map, the inactive view, or the four view numbers. A missing map uses the unknown label. The debug-trace callee then runs, and a final line feed is appended.

| Managed (`VmDMain1419Slice.cs`) | Native (`d_main.cpp`) |
|----------------------------------|----------------------|
| Capture, mouse, viewport, and crash text | **3321–3642** |
| Band hull (exclusive slice) | **3321–3642** |

### Chunk **1419** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **322** |
| **Net headline** | **+322** vs the prior chunk |


**Post-review ratio:** 464/322 managed/native ≈ 144% (src file LOC at HEAD; see tally).

## Tests

- `VmDMain1419SliceTests.InputAndUi`
- `VmDMain1419SliceTests.CrashSceneAndText`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1419 --to 1419` (0 hard / 0 soft).

## Seams

UI processor, native-mouse, sound, texture, automap, player-name, and debug-trace calls stay callees. DoomButtons ends at line 3316. System_M_Dim is declared at line 3644.

## Credit rationale

UNCredited d_main.cpp 3321-3642 (+322). The span runs from the mouse_capturemode handler through System_CrashInfo. The vm_debug_port declaration and the crash-info banner sit inside the span. DoomButtons at 3283-3316 is a 34-line name table and stays uncredited. PatchTextures starts at line 3646. hq4x and hq4x_asm stay later because they are generated tables. The band is not sliced further.
