# Chunk 396 — debug_restart, cleanup warn, window title, fs_dir, type, cmdline files Retired (2026-10-24): `--band` hull overlaps existing credit (d-main-gameinfo-scan-chunk399 src/d_main.cpp 2311-2435).

## Scope

debug_restart, cleanup warn, window title, fs_dir, type, cmdline files Retired (2026-10-24): `--band` hull overlaps existing credit (d-main-gameinfo-scan-chunk399 src/d_main.cpp 2311-2435).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `8a0d35e1054d17265e670e5c80e2edf5ddfae7e7`.

## Padding review (2026-10-21)



### Out-of-body uncredit (396)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **157** |
| **Out-of-body removed** | **0** |
| **Honest** | **157** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 396 --to 396` (0 hard / 0 soft).

## Seams

`Args`, `fileSystem`, and `I_SetWindowTitle` are not hosted.

## Credit rationale

Credits `D_Cleanup` netgame warning, `debug_restart`, `I_UpdateWindowTitle`, `CCMD(fs_dir)`, `CCMD(type)`, `D_MultiExec`, and `GetCmdLineFiles` from `d_main.cpp`.
