# Chunk 1416 — Startup init, autoload files, and command-line checks in d_main.cpp (2488-2800).

## Scope

Startup init, autoload files, and command-line checks in d_main.cpp (2488-2800).

## Native ranges

- `src/d_main.cpp` **D_DoomInit** 2488–2800 (313 lines)

**Credited native LOC:** 313 (tally `nativeLoc` 313).

## Managed

`VmDMain1416Slice.cs`
Managed LOC (tally): 418.
Conversion commit: `e7aa7515f57ccbb84cd23f4dd5f09e0b078568fb`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| D_DoomInit (**2488–2505**) | yes |
| AddAutoloadFiles (**2513–2586**) | yes |
| CheckCmdLine (**2594–2800**) | yes |
| Band hull (**2488–2800**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| SetMapxxFlag | ends at line 2480 |
| StartupStatusLine | starts at line 2802 |
| Response files, file collection, defaults, file search, add file, directories, config files, app-data path, nice path, mode traces, map name, map override, map data, and the dev string | callees |

D_DoomInit searches response files, then collects -deh/.deh, -bex/.bex, -exec/.cfg, -playdemo/.lmp, -file, and -optfile. It sets the startup state, prints the defaults line unless the run is a batch, and loads defaults. AddAutoloadFiles appends a dot to the IWAD filter. Shareware or -noextras skips lights.pk3, brightmaps.pk3, and game_widescreen_gfx.pk3. Each of those loads when its startup value is 1, or when the value is any other non-zero number and its autoload cvar is on, and the matching block switch is absent. A hit is added as an optional checked file at position -1. Shareware, -noautoload, the disable flag, -host, a join, or a dedicated server skips the rest. Otherwise zvox.wad is searched without the look-first flag. Skins are the share directory on Unix and the program directory otherwise, for both wad and pk3 patterns. Unix also adds the app-data skins path after NicePath. Global.Autoload is loaded, then every dot whose index is greater than 0 adds that prefix plus Autoload. A dot at index 0 does not start the loop. CheckCmdLine keeps the current dmflags. The first +sv_gametype argument, or +set followed by sv_gametype, supplies the explicit type and stops the scan. Value 4 is invasion. The checking line is skipped for a batch run. -nomonsters, -respawn, and -fast set their bits. -devparm overwrites the dev flag. -altdeath sets deathmatch and the game type to 1 and sets item respawn. Otherwise -deathmatch also sets weapons stay. Otherwise -coop, except during invasion, clears deathmatch, teamplay, and the game type, sets the coop weapon and player flags, and adds local items unless the game is Hexen. Otherwise a launch that is not hosting, joining, or dedicated and has no explicit type resets those three values to 0. A trace is skipped when the value is already set. The default map is MAP01, E1M1, or the Hexen hub. A stored warp sets autostart without changing the map. -skill uses the first character. -warp reads one map number in MAPxx mode and an episode plus a map otherwise; a missing or out-of-range second number moves the episode into the map slot. +map is removed. A compatibility override replaces the name only when that map exists, and a name that fails the check still blocks the launcher-default flag. A missing map is reported and leaves an earlier warp in place. -turbo prints a rounded percent, -timer prints a singular or plural minute line, and -avg forces 20 minutes whenever its value is present. Without a start screen the function returns before the status lines. Those lines are deathmatch, no monsters, respawn, and the warp line, whose skill is one more than the stored skill.

| Managed (`VmDMain1416Slice.cs`) | Native (`d_main.cpp`) |
|----------------------------------|----------------------|
| Startup init, autoload, and command line | **2488–2800** |
| Band hull (exclusive slice) | **2488–2800** |

### Chunk **1416** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **313** |
| **Net headline** | **+313** vs the prior chunk |


**Post-review ratio:** 418/313 managed/native ≈ 134% (src file LOC at HEAD; see tally).

## Tests

- `VmDMain1416SliceTests.InitCollectsAndAutoloadPaths`
- `VmDMain1416SliceTests.CommandLineModesWarpAndStatus`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1416 --to 1416` (0 hard / 0 soft).

## Seams

Response files, file collection, defaults, file search, add file, directories, config files, app-data path, nice path, mode traces, map name, map override, map data, and the dev string stay callees. StartupStatusLine starts at line 2802.

## Credit rationale

UNCredited d_main.cpp 2488-2800 (+313). D_DoomInit, AddAutoloadFiles, and CheckCmdLine are whole functions. The comment banners between them sit inside the span. StartupStatusLine starts at line 2802. hq4x and hq4x_asm stay later because they are generated tables. The band is not sliced further.
