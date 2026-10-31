# Chunk 1365 — Crash info, OS detection, and SDL main from i_main.cpp (1-238), through end of file.

## Scope

Crash info, OS detection, and SDL main from i_main.cpp (1-238), through end of file.

## Native ranges

- `src/common/platform/posix/sdl/i_main.cpp` **SdlMain** 1–238 (238 lines)

**Credited native LOC:** 238 (tally `nativeLoc` 238).

## Managed

`VmSdlMain1365Slice.cs`
Managed LOC (tally): 308.
Conversion commit: `5bf29f08e18cc2050ff011666fdd1953cc4d054e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Banner, includes, and prototypes (**1–112**) | yes |
| `GetCrashInfo` (**115–119**) | yes |
| `I_DetectOS` (**121–170**) | yes |
| `main` (**174–238**) | yes |
| Band hull (**1–238**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| `Mac_I_FatalError` and `Linux_I_FatalError` | declarations; the bodies are not in this file |
| `SignalHandler`, `GameMain`, and `FArgs` | the bodies are not in this file |
| `cc_install_handlers`, SDL, `realpath`, `popen`, and `uname` | injected results |
| Commented `SIGHUP` and `SIGQUIT` | not installed |
| `PROGDIR` | not defined in this translation unit |

`GetCrashInfo` calls `CrashInfo` only when the pointer is set. The separator is `\n` and the size is the end-pointer difference. The return is `strlen` after that call, so the callback can shorten the buffer. A null buffer throws `buffer`. A buffer with no NUL throws `buffer`.

`I_DetectOS` tries `/etc/os-release` and then `/usr/lib/os-release`. A nonzero `stat` tries the next path. A null `popen` tries the next path and does not `pclose`. A successful `popen` always `pclose`s and then breaks, including when the text is shorter than 2 bytes. The command is `. <path> && echo ${PRETTY_NAME}`, cut to 255 bytes. The read keeps at most 255 bytes. `strlen` decides the length, and a length above 1 drops the last byte whether or not it is a newline. An embedded NUL ends the string. `uname` failure leaves `sys_ostype` unchanged. An empty result becomes `Unknown` only after that call. Success appends `, ` only when a pretty name was stored. `sys_ostype` is `sysname release on machine` and does not include the pretty name. Empty uname fields still produce `  on `.

On a non-Apple build, `main` installs handlers with `SIGSEGV`, `SIGILL`, `SIGFPE`, and `SIGBUS` (11, 4, 8, 7), count 4, and `hcde-crash.log`. Apple skips that call. `SIGINT` (2) and `SIGTERM` (15) are installed next. The banner is `HCDE <version> - <git> - SDL version` and `Compiled on <date>`, before `seteuid(getuid())`. `setenv` writes `LC_NUMERIC` `C` with overwrite 1. `setlocale` uses `LC_ALL` (6) and `C`. `SDL_Init(0)` below 0 prints `Could not initialize SDL:` plus the error and returns -1. That path has already printed, dropped privileges, and set the locale, and it does not build `Args`, set `progdir`, start joysticks, call `GameMain`, release the mouse, or quit. A zero or positive init continues. `FArgs` copies `argc` strings and turns a null slot into an empty string. An `argc` past the array throws `argv` before `Args` is replaced. `argc` 0 still leaves an empty `Args` when `argv[0]` is read afterward.

Without `PROGDIR`, a null `realpath` copies `argv[0]`. The directory keeps the last slash. No slash yields `./`. A backslash is not a separator. A length of at least 4096 throws `program` before joysticks. A null `argv[0]` throws `argv` before `realpath`, after `Args` is stored. A set `PROGDIR`, including an empty one, skips `realpath`. Joysticks run, then `GameMain`. The restart check is next. A true check runs `I_TryRestart` and still sets relative mouse mode to 0 and quits. The process result is `GameMain`'s return, including a negative value. `SDL_Init` failure returns -1 rather than the SDL code. Linux false returns before `getenv`. A set `APPIMAGE`, including an empty string, opens that path read-only, calls `fexecve` with that fd and `environ`, and does not open `/proc/self/exe`. An unset variable opens `/proc/self/exe`. The fd is passed through even when it is -1.

| Managed (`VmSdlMain1365Slice.cs`) | Native (`i_main.cpp`) |
|-----------------------------------|----------------------|
| Crash info, OS detection, and SDL main | **1–238** |
| Band hull (exclusive slice) | **1–238** |

### Chunk **1365** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **238** |
| **Net headline** | **+238** vs the prior chunk |


**Post-review ratio:** 308/238 managed/native ≈ 129% (src file LOC at HEAD; see tally).

## Tests

- `VmSdlMain1365SliceTests.DetectOsAndCrashInfo`
- `VmSdlMain1365SliceTests.StartupMain`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1365 --to 1365` (0 hard / 0 soft).

## Seams

cc_install_handlers, signal, SDL, realpath, popen, uname, and fexecve stay injected. GameMain, I_StartupJoysticks, and FArgs stay outside this file. PROGDIR is off unless the caller sets it. The Linux restart body is the compiled path.

## Credit rationale

UNCredited i_main.cpp 1-238 (+238). A full tally line scan, including recognition ranges, shows no overlap. This is the largest remaining hole of at most 400 lines. It is not a tie. file_7z.cpp 1-235 is 235. Gaps longer than 400 lines stay whole and are not sliced. The file ends at line 238.
