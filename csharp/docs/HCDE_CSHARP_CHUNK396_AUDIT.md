# Chunk 396 — debug_restart, cleanup warn, window title, fs_dir, type, cmdline files

## Scope

debug_restart, cleanup warn, window title, fs_dir, type, cmdline files

## Native ranges

- `src/d_main.cpp` **D_Cleanup netgame warn** 4890–4897 (8 lines)
- `src/d_main.cpp` **UNSAFE_CCMD debug_restart** 4983–5019 (37 lines)
- `src/d_main.cpp` **I_UpdateWindowTitle** 5029–5081 (53 lines)
- `src/d_main.cpp` **CCMD fs_dir** 5083–5097 (15 lines)
- `src/d_main.cpp` **CCMD type** 5099–5108 (10 lines)
- `src/d_main.cpp` **D_MultiExec** 2275–2282 (8 lines)
- `src/d_main.cpp` **GetCmdLineFiles** 2284–2309 (26 lines)

**Credited native LOC:** 157 (tally `nativeLoc` 157).

## Managed

`VmDMainDebugCcmds.cs`
Managed LOC (tally): 74.
Conversion commit: `847878b658b261b2d04787e235b5203559525ec7`.

## Tests

- `VmDMainDebugCcmdsTests.ShouldIgnoreDebugRestart_DuringNetgame`
- `VmDMainDebugCcmdsTests.StripTitleColorEscapes_RemovesBracketSequence`
- `VmDMainDebugCcmdsTests.FormatBrandedWindowTitle_PrefixesHcde`
- `VmDMainDebugCcmdsTests.ShouldMarkFsDirEntryHidden_WhenShadowed`
- `VmDMainParseCVarInfoTests.IsPatchExtension_MatchesDeh`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 396 --to 396` (0 hard / 0 soft).

## Seams

`Args`, `fileSystem`, and `I_SetWindowTitle` are not hosted.

## Credit rationale

Credits `D_Cleanup` netgame warning, `debug_restart`, `I_UpdateWindowTitle`, `CCMD(fs_dir)`, `CCMD(type)`, `D_MultiExec`, and `GetCmdLineFiles` from `d_main.cpp`.
