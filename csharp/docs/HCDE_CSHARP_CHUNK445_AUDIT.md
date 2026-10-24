# Chunk 445 — D_DoomMain_Internal restart loop: IWAD pass, debug listen, D_DoomLoop, SignalHandler, and GameMain (consolidated chunk445–448).

## Scope

D_DoomMain_Internal restart loop: IWAD pass, debug listen, D_DoomLoop, SignalHandler, and GameMain (consolidated chunk445–448).

## Native ranges

- `src/d_main.cpp` **do loop PClass/PType init** 4658–4664 (7 lines)
- `src/d_main.cpp` **IWAD/mod iteration body** 4666–4761 (96 lines)
- `src/d_main.cpp` **debug listen and D_DoomLoop cycle** 4763–4792 (30 lines)
- `src/d_main.cpp` **SignalHandler** 4795–4807 (13 lines)
- `src/d_main.cpp` **GameMain** 4809–4880 (72 lines)

**Credited native LOC:** 218 (tally `nativeLoc` 218).

## Managed

`VmDMainGameMain.cs`, `VmDMainGameMainTests.cs`
Managed LOC (tally): 161.
Conversion commit: `9165fd6e7dc93c71131e7d958b66558e8b3b6210`.

## Tests

- `VmDDoomMainLoopIterTests.AfterPClassStaticInit`
- `VmDDoomMainIwadIterationTests.SharewareBlocksPwads_WhenModsPresent`
- `VmDDoomMainIwadIterationTests.AbortLoopWhenInitGameFailed`
- `VmDDoomMainRunLoopTests.DebugPortInRange_Valid`
- `VmDDoomMainRunLoopTests.DebugPortInRange_RejectsHigh`
- `VmDMainSignalHandlerTests.ExitImmediatelyWhenAlreadyAborting`
- `VmDMainSignalHandlerTests.RequestShutdownWhenRunning`
- `VmDMainGameMainTests.CreateDisplayWhenInteractive`
- `VmDMainGameMainTests.SkipCloseWidgetWhenNorun`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 445 --to 445` (0 hard / 0 soft).

## Seams

`FIWadManager`, `D_InitGame`, `DebugServer`, `D_DoomLoop`, `DisplayBackend`, and shutdown I/O are not hosted.

## Credit rationale

Credits contiguous `d_main.cpp` 4658–4880: class/type static init, IWAD/mod resolution, shareware and SteamAppId gates, debug-port policy, doom-loop restart, signal handler, and `GameMain` entry.
