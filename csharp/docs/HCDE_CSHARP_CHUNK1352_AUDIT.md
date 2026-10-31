# Chunk 1352 — Unix config, cache, documents, and save paths from i_specialpaths.cpp (1-262), through end of file.

## Scope

Unix config, cache, documents, and save paths from i_specialpaths.cpp (1-262), through end of file.

## Native ranges

- `src/common/platform/posix/unix/i_specialpaths.cpp` **M_GetAppDataPath** 1–262 (262 lines)

**Credited native LOC:** 262 (tally `nativeLoc` 262).

## Managed

`VmUnixPaths1352Slice.cs`
Managed LOC (tally): 187.
Conversion commit: `4f8971b50ebd1962f78f33bc0ccf41dce2e79b8d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| XDG path macros (**36–67**) | yes |
| User file and the config directory (**69–128**) | yes |
| Cache, autoexec, config, and documents (**130–194**) | yes |
| Screenshots, savegames, demos, and realpath (**196–262**) | yes |
| Band hull (**1–262**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| NicePath | the string rewrite stays outside this file |
| CreatePath | the recursive directory create stays outside this file |
| realpath | the absolute path stays outside this file |

Apple returns the fallback and does not read the environment. Other platforms cache the first getenv result, and an empty variable is kept. A missing game directory creates the config directory at mode 0700 and then the game directory. A failed create reports the raw config path or the niced game path plus the error text. An existing non-directory is rejected before any later create. The config-file reader flag is ignored. App data and the cache path call CreatePath only when asked. Haiku documents are the config folder plus the game name, and screenshots hang off that. Other platforms hang documents off app data and screenshots off the pictures folder plus HCDE. The screenshots string is niced again on later calls. Savegames append netgame only while that flag is set. realpath failure returns null.

| Managed (VmUnixPaths1352Slice.cs) | Native (i_specialpaths.cpp) |
|-----------------------------------|-----------------------------|
| XDG folders, user file, and saves | **1–262** |
| Band hull (exclusive slice) | **1–262** |

### Chunk **1352** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **262** |
| **Net headline** | **+262** vs the prior chunk |


**Post-review ratio:** 187/262 managed/native ≈ 71% (src file LOC at HEAD; see tally).

## Tests

- `VmUnixPaths1352SliceTests.EnvCacheAndDocuments`
- `VmUnixPaths1352SliceTests.UserFileMkdirAndConfig`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1352 --to 1352` (0 hard / 0 soft).

## Seams

NicePath, stat, mkdir, CreatePath, getenv, and realpath stay injected. The game names are hcde and HCDE.

## Credit rationale

UNCredited i_specialpaths.cpp 1-262 (+262). A full tally line scan shows no overlap. This is the largest remaining hole of at most 400 lines, ahead of p_lnspec.cpp 2869-3129.
