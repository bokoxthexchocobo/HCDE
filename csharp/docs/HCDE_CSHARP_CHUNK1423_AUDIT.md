# Chunk 1423 — D_InitGame launch tail in d_main.cpp (4189-4499).

## Scope

D_InitGame launch tail in d_main.cpp (4189-4499).

## Native ranges

- `src/d_main.cpp` **D_InitGame** 4189–4499 (311 lines)

**Credited native LOC:** 311 (tally `nativeLoc` 311).

## Managed

`VmDMain1423Slice.cs`
Managed LOC (tally): 364.
Conversion commit: `745f9b163795c91bd7b116ffdb79c6e5534d9fc1`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| GL definitions, refresh, and decals (**4190–4207**) | yes |
| Dehacked lumps, config patches, and sounds (**4208–4242**) | yes |
| Menus, symbols, actors, and bots (**4244–4267**) | yes |
| Play loop, status bar, and startup strings (**4269–4294**) | yes |
| Network check, cvars, and late video (**4296–4358**) | yes |
| Delayed commands and invulnerability (**4361–4372**) | yes |
| Demo, save, warp, title, and restart (**4374–4499**) | yes |
| Band hull (**4189–4499**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Resource load | ends at line 4188 |
| D_DoomMain | starts at line 4507 |
| Refresh, dehacked, menu, actor, net, and session bodies | callees |

A start screen enters the GL phase and receives the graphics, engine, and network status lines. A missing config crashes on the refresh status. The window advances once before refresh and crashes when it is missing. Batch runs skip the decal, menu, startup-string, and screen prints. IWAD dehacked uses the skip-bex flag only when the IWAD asks for it. PWAD dehacked always uses flags 0. Doom and only Doom reads Doom.DefaultDehacked when neither command-line deh nor bex was found and the section opens. Path matches without case. A null key or a null path value crashes. A missing file is skipped. Each existing path is applied, with a print unless this is batch. Options and the dehacked finish always run. Local sounds lock only when dehacked added sounds. Symbols are removed only when the debugger is off. A missing level crashes before bots are cleared. A null bot list crashes after the clear. Bots are copied, tries are 0, and the wanted count is the list size. No player-class work remains in this tail. A failed net check returns 0 before cvars lock and before the engine event. Otherwise cvars lock. Late video runs only for a live client whose video is not up, under the late-video guard. The screen line is skipped for batch, norun, and dedicated. A live client with no shader yet compiles once. A restart, or a client whose shader already ran, updates the palette instead. Norun and dedicated do neither. A null screen crashes on that compile or palette call. A missing start screen and a wanted startup screen guard the overlay, size it from the budget times 10 over 9 when per-shader progress is positive, otherwise the budget plus 3, and render unless the overlay itself is null. Norun or batch then returns 1337. Otherwise the state is full console, the net starts, delayed commands run unless dedicated, and the state becomes startup. Custom invulnerability updates the colormap. A restart skips the session block, adjusts the mode, starts the title, and clears the mode change. A fresh game records a demo when one was requested, plays the done sound unless dedicated, and finishes the window by the texture budget. The start screen is deleted when it exists. A client compiles shaders until one succeeds, then opens the 2D drawer and updates the joystick and VR menus. A dedicated server skips that loop. A missing save is fatal with the built name and no extra newline. A null built name crashes. A found save selects the load-game action. A play demo, including an empty name, wins over a timed demo and over warping. A timed demo wins over warping. An existing load action skips warping and, when a demo is being recorded, records a null map. Autostart or a net game sets the wipe hold to 35 tics unless a warp wipe was requested, checks the map, records that map when recording, starts it, and either queues a stored warp or prints the dedicated ignore line. A null map crashes. Otherwise multiplayer, no-intros, or the no-intro switch starts the title. Otherwise the frame time is set and the intro cutscene is started in block-UI mode. A cutscene that does not start falls through to the title. A callback sets the title loop. The engine initialize event runs only on the normal return.

| Managed (`VmDMain1423Slice.cs`) | Native (`d_main.cpp`) |
|----------------------------------|----------------------|
| Launch tail | **4189–4499** |
| Band hull (exclusive slice) | **4189–4499** |

### Chunk **1423** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **311** |
| **Net headline** | **+311** vs the prior chunk |


**Post-review ratio:** 364/311 managed/native ≈ 117% (src file LOC at HEAD; see tally).

## Tests

- `VmDMain1423SliceTests.DefinitionsMenusAndEarlyExits`
- `VmDMain1423SliceTests.WarpTitleSaveAndRestart`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1423 --to 1423` (0 hard / 0 soft).

## Seams

Refresh, dehacked, menu, actor, bot, net, video, and session calls stay callees. Resource loading ends at line 4188. D_DoomMain starts at line 4507.

## Credit rationale

UNCredited d_main.cpp 4189-4499 (+311). This finishes D_InitGame after the wad-settings progress and stops at the function's closing brace. D_DoomMain stays separate because earlier credit already covers part of it. The band is not sliced further.
