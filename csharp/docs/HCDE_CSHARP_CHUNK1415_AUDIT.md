# Chunk 1415 — Patch arguments, exec files, command-line wads, GAMEINFO, and the MAPxx flag in d_main.cpp (2251-2480).

## Scope

Patch arguments, exec files, command-line wads, GAMEINFO, and the MAPxx flag in d_main.cpp (2251-2480).

## Native ranges

- `src/d_main.cpp` **ConsiderPatches** 2251–2480 (230 lines)

**Credited native LOC:** 230 (tally `nativeLoc` 230).

## Managed

`VmDMain1415Slice.cs`
Managed LOC (tally): 333.
Conversion commit: `55e948505649aa5034f40613903e0ff045dff79d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| ConsiderPatches (**2251–2267**) | yes |
| D_MultiExec (**2275–2282**) | yes |
| GetCmdLineFiles (**2284–2309**) | yes |
| ParseGameInfo (**2311–2435**) | yes |
| GetReserved (**2437–2443**) | yes |
| CheckGameInfo (**2445–2465**) | yes |
| SetMapxxFlag (**2473–2480**) | yes |
| Band hull (**2251–2480**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| ParseCVarInfo | ends at line 2241 |
| D_DoomInit | starts at line 2488 |
| File search, DeHackEd, exec parse, wild add, directory check, file insert, init, and color parse | callees |

ConsiderPatches searches each argument for a .deh file and then a .bex file, loads the first hit with lump 0, and returns whether any argument was present. MultiExec parses each argument and threads the exec list through. Command-line files start at the current wad count unless the call is for optional files, which start at 0. Newly added files are forced non-optional. GAMEINFO opens as that script. An IWAD key is stored unless the launcher is showing, in which case the value is skipped as an unknown property. LOAD inserts a same-directory path when that path exists and the bare name otherwise. The insert position advances only when the add succeeds. nospriterename is true only for the text true. Startup title, colors, type, song, lights, brightmaps, widescreen, and Steam are stored. Discord is read and not stored. Unknown keys consume a comma-separated value list. CheckGameInfo builds the reserved filter, and only a successful init reads the newest global GAMEINFO. SetMapxxFlag sets the MAPxx bit when MAP01, maps/map01.wad, or maps/map01.map is in the IWAD. A negative IWAD number matches any container. Other flag bits stay set.

| Managed (`VmDMain1415Slice.cs`) | Native (`d_main.cpp`) |
|----------------------------------|----------------------|
| Patches, GAMEINFO, and MAPxx | **2251–2480** |
| Band hull (exclusive slice) | **2251–2480** |

### Chunk **1415** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **230** |
| **Net headline** | **+230** vs the prior chunk |


**Post-review ratio:** 333/230 managed/native ≈ 145% (src file LOC at HEAD; see tally).

## Tests

- `VmDMain1415SliceTests.PatchesExecFilesAndMapFlag`
- `VmDMain1415SliceTests.GameInfoParseAndCheck`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1415 --to 1415` (0 hard / 0 soft).

## Seams

File search, DeHackEd load, exec parse, wild-file add, directory existence, file insert, filesystem init, and color parse stay callees. D_DoomInit starts at line 2488.

## Credit rationale

UNCredited d_main.cpp 2251-2480 (+230). ConsiderPatches, D_MultiExec, GetCmdLineFiles, ParseGameInfo, GetReserved, CheckGameInfo, and SetMapxxFlag are whole functions. The comment banners between them sit inside the span. D_DoomInit starts at line 2488. hq4x and hq4x_asm stay later because they are generated tables. The band is not sliced further.
