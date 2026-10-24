# Chunk 412 — PickPlayerStart, QueueBody, DoPlayerPop, ScreenShot

## Scope

PickPlayerStart, QueueBody, DoPlayerPop, ScreenShot

## Native ranges

- `src/g_game.cpp` **QueueBody** 2307–2357 (51 lines)
- `src/g_game.cpp` **G_DoPlayerPop** 2453–2499 (47 lines)
- `src/g_game.cpp` **G_ScreenShot** 2501–2509 (9 lines)
- `src/g_game.cpp` **G_LoadGame** 2515–2522 (8 lines)
- `src/g_game.cpp` **CheckSingleWad** 2524–2570 (47 lines)

**Credited native LOC:** 162 (tally `nativeLoc` 158).

## Managed

`VmGGameDeathmatchSpawnFlow.cs`, `VmGGameDmSpawnFlowTests.cs`, `VmGGamePickPlayerStart.cs`, `VmGGamePlayerPop.cs`, `VmGGameQueueBody.cs`, `VmGGameScreenShot.cs`
Managed LOC (tally): 133.
Conversion commit: `2f3f64e76048a7b687a8fb2b67e19aac2d033ced`.

## Tests

- `VmGGameDmSpawnFlowTests.QueueBody_LimitsPlayerCorpsesWhenFilterSet`
- `VmGGameDmSpawnFlowTests.QueueBody_DestroysWhenLimitZero`
- `VmGGameDmSpawnFlowTests.PlayerPop_RevertsSpyCamera`
- `VmGGameDmSpawnFlowTests.ScreenShot_QueuesWhenIdle`
- `VmGGameDmSpawnFlowTests.LoadGame_SetsActionWhenNamed`
- `VmGGameDmSpawnFlowTests.CheckSaveWads_FailsWhenMissing`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 412 --to 412` (0 hard / 0 soft).

## Seams

Filesystem, serializer, and screenshot/menu IO are not hosted.

## Credit rationale

Credits corpse queue, disconnect pop, screenshot gate, and save-game WAD validation from `g_game.cpp` (bands avoid chunk 9 deathmatch spawn).
