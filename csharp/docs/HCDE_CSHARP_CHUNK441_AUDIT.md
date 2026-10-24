# Chunk 441 — D_InitGame shader compile, net init, demo/title startup

## Scope

D_InitGame shader compile, net init, demo/title startup

## Native ranges

- `src/d_main.cpp` **shader compile and start screen** 4346–4354 (9 lines)
- `src/d_main.cpp` **norun exit and net init** 4356–4372 (17 lines)
- `src/d_main.cpp` **record demo and progress finish** 4374–4412 (39 lines)
- `src/d_main.cpp` **loadgame demo timedemo warp** 4414–4487 (74 lines)
- `src/d_main.cpp` **restart title and engine init event** 4489–4499 (11 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmDMainInitGameFinish.cs`, `VmDMainInitGameFinishTests.cs`
Managed LOC (tally): 82.
Conversion commit: `b226ed590dcfa3da2e19d37f4f026426219b28b4`.

## Tests

- `VmDMainInitGameFinishTests.CreateStartScreenWhenMissing`
- `VmDMainInitGameFinishTests.NorunBatchEarlyExit`
- `VmDMainInitGameFinishTests.NorunExitCode`
- `VmDMainInitGameFinishTests.RecordDemoFromArg`
- `VmDMainInitGameFinishTests.LoadgameFatalWhenMissing`
- `VmDMainInitGameFinishTests.AutostartWarp`
- `VmDMainInitGameFinishTests.DedicatedIgnoresStoredWarp`
- `VmDMainInitGameFinishTests.RestartUsesStartTitle`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 441 --to 441` (0 hard / 0 soft).

## Seams

`G_InitNew`, `G_LoadGame`, and `StartCutscene` are not hosted.

## Credit rationale

Credits norun/batch early return, delayed commands, invul map toggle, demo autostart, warp wipe policy, and dedicated stored-warp ignore from `d_main.cpp`.
