# Chunk 393 — dmflags3, compatflags refresh, HCDE rcon poll hook

## Scope

dmflags3, compatflags refresh, HCDE rcon poll hook

## Native ranges

- `src/d_main.cpp` **CUSTOM_CVAR dmflags3** 803–805 (3 lines)
- `src/d_main.cpp` **CVAR dmflags3 flags** 807–814 (8 lines)
- `src/d_main.cpp` **CUSTOM_CVAR compatflags** 824–830 (7 lines)
- `src/d_main.cpp` **CUSTOM_CVAR compatflags2** 832–839 (8 lines)
- `src/d_net_rcon.cpp` **HCDERconPollListener** 423–436 (14 lines)
- `src/d_net_rcon.cpp` **HCDERconDrainIngress** 438–497 (60 lines)
- `src/d_main.cpp` **CUSTOM_CVAR wipetype** 485–488 (4 lines)
- `src/d_main.cpp` **CUSTOM_CVAR mouse_capturemode** 3321–3331 (11 lines)
- `src/d_main.cpp` **CUSTOM_CVAR vm_debug** 3333–3340 (8 lines)
- `src/am_map.cpp` **CCMD am_restorecolors** 578–593 (16 lines)
- `src/maploader/maploader.cpp` **MapLoader::GroupLines** 2659–2705 (47 lines)

**Credited native LOC:** 186 (tally `nativeLoc` 186).

## Managed

`VmAutomapRestoreColors.cs`, `VmAutomapRestoreColorsTests.cs`, `VmDMainCompatCallbacks.cs`, `VmDMainRconPoll.cs`
Managed LOC (tally): 59.
Conversion commit: `6799ce33a56e6475f5314b97c72e018e1b7d0bef`.

## Tests

- `VmDMainCompatCallbacksTests.IsDmflags3FlagSet_DetectsClip`
- `VmDMainRconPollTests.ShouldRestartListener_WhenPortChanges`
- `VmDMainRconPollTests.ShouldRejectClient_WhenNoSlots`
- `VmAutomapRestoreColorsTests.ShouldResetStandardSlot_InRange`
- `VmMapLoaderGroupLinesTests.ShouldFlagMissingFrontSector`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 393 --to 393` (0 hard / 0 soft).

## Seams

`Level->ApplyCompatibility` and socket IO are not hosted.

## Credit rationale

Credits `CUSTOM_CVAR(dmflags3)`, `compatflags`/`compatflags2`, dmflags3 flag CVARs, and `HCDERconPollListener` from `d_main.cpp` / `d_net_rcon.cpp`.
