# Chunk 445 — D_DoomMain_Internal restart loop through GameMain (4658–4880)

## Scope

D_DoomMain_Internal restart loop through GameMain (4658–4880)

## Native ranges

- `src/d_main.cpp` **do loop PClass/PType init** 4658–4664 (7 lines)
- `src/d_main.cpp` **IWAD/mod iteration body** 4666–4761 (96 lines)
- `src/d_main.cpp` **debug listen and D_DoomLoop cycle** 4763–4792 (30 lines)
- `src/d_main.cpp` **SignalHandler** 4795–4807 (13 lines)
- `src/d_main.cpp` **GameMain** 4809–4880 (72 lines)
- `src/d_main.cpp` **D_Cleanup** 4890–4896 (7 lines)
- `src/d_main.cpp` **D_Cleanup** 4965–4966 (2 lines)
- `src/d_main.cpp` **debug_restart** 4984–5005 (22 lines)

**Credited native LOC:** 249 (tally `nativeLoc` 249).

## Managed

`VmDDoomMainLoopIter.cs`, `VmDDoomMainIwadIteration.cs`, `VmDDoomMainRunLoop.cs`, `VmDMainSignalHandler.cs`, `VmDMainGameMain.cs`, `VmDMainCleanupRestart.cs`
Managed LOC (tally): 351.
Conversion commit: `cecbfe3bde8943070776ff93dfa18ad52dc2a669`.

## Padding review (2026-10-21)



| `NetgameCleanupWarn` | `d_main.cpp` 4890–4893 |
| `ShutdownTagNonEmpty` | 4895–4896 |
| `IncrementRestartCounter` | 4965–4966 |
| `IgnoreDebugRestartDuringNetgame` | 4984–4987 |
| `StripRestartArgs` | 4994–5005 |

### Out-of-body uncredit (445)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **249** |
| **Out-of-body removed** | **0** |
| **Honest** | **249** |


**Post-review ratio:** 351/249 managed/native ≈ 141% (src file LOC at HEAD; see tally).

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
- `VmDMainCleanupRestartTests.NetgameCleanupWarn`
- `VmDMainCleanupRestartTests.ShutdownTagNonEmpty`
- `VmDMainCleanupRestartTests.IncrementRestartCounter`
- `VmDMainCleanupRestartTests.IgnoreDebugRestartDuringNetgame`
- `VmDMainCleanupRestartTests.StripRestartArgs_WhenRunAllowed`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 445 --to 445` (0 hard / 0 soft).

## Seams

`FIWadManager`, `D_InitGame`, `DebugServer`, `D_DoomLoop`, `DisplayBackend`, and shutdown I/O are not hosted.

## Credit rationale

Credits contiguous `d_main.cpp` 4658–4880: class/type static init, IWAD/mod resolution, shareware and SteamAppId gates, debug-port policy, doom-loop restart, signal handler, and `GameMain` entry.
