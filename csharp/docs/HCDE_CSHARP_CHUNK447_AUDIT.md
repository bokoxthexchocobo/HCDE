# Chunk 447 — SignalHandler gameloop_abort policy

## Scope

SignalHandler gameloop_abort policy

## Native ranges

- `src/d_main.cpp` **SignalHandler** 4795–4807 (13 lines)

**Credited native LOC:** 13 (tally `nativeLoc` 13).

## Managed

`VmDMainSignalHandler.cs`, `VmDMainSignalHandlerTests.cs`
Managed LOC (tally): 13.
Conversion commit: `86b2f3cf27c87f1713a36409934cdbe953fb16c0`.

## Tests

- `VmDMainSignalHandlerTests.ExitImmediatelyWhenAlreadyAborting`
- `VmDMainSignalHandlerTests.RequestShutdownWhenRunning`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 447 --to 447` (0 hard / 0 soft).

## Seams

POSIX signal delivery is not hosted.

## Credit rationale

Credits immediate exit when already aborting versus setting gameloop_abort from `d_main.cpp`.
