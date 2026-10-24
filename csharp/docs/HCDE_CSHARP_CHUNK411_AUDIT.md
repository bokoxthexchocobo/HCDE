# Chunk 411 — G_FullConsole, cutscene tick/advance, CheckSpot and DM start pick

## Scope

G_FullConsole, cutscene tick/advance, CheckSpot and DM start pick

## Native ranges

- `src/g_game.cpp` **G_FullConsole** 1392–1412 (21 lines)
- `src/g_game.cpp` **D_RunCutscene** 1414–1442 (29 lines)
- `src/g_game.cpp` **D_CheckCutsceneAdvance** 1446–1450 (5 lines)
- `src/g_game.cpp` **G_TickStalledCutscene** 1456–1473 (18 lines)
- `src/g_game.cpp` **G_SpawnPlayer title tail** 2012–2031 (20 lines)
- `src/g_game.cpp` **CheckSpot** 2040–2082 (43 lines)
- `src/g_game.cpp` **PlayersRangeFromSpot** 2092–2110 (19 lines)
- `src/g_game.cpp` **SelectFarthestDeathmatchSpot** 2113–2131 (19 lines)
- `src/g_game.cpp` **SelectRandomDeathmatchSpot** 2134–2149 (16 lines)

**Credited native LOC:** 190 (tally `nativeLoc` 190).

## Managed

`VmGGameCutsceneFlow.cs`, `VmGGameCutsceneSpawnTests.cs`, `VmGGameFullConsole.cs`, `VmGGameSpawnPlayerTail.cs`, `VmGGameSpawnSpot.cs`
Managed LOC (tally): 129.
Conversion commit: `a1bd4c3f1cec477d7c000a9dcc22436aa13a9c2e`.

## Tests

- `VmGGameCutsceneSpawnTests.FullConsole_StopsMusicOutsideStartup`
- `VmGGameCutsceneSpawnTests.CutsceneFlow_EndsSinglePlayerJob`
- `VmGGameCutsceneSpawnTests.SpawnPlayerTail_SkipsInventoryOnTitleMap`
- `VmGGameCutsceneSpawnTests.SpawnSpot_RejectsZeroType`
- `VmGGameCutsceneSpawnTests.PlayersRangeFromSpot_UsesNearestLiving`
- `VmGGameCutsceneSpawnTests.SelectFarthestIndex_PrefersRemoteStart`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 411 --to 411` (0 hard / 0 soft).

## Seams

`ScreenJobRunner` VM, `P_CheckPosition`, and live player structs are not hosted.

## Credit rationale

Credits `G_FullConsole`, cutscene progression helpers, `G_SpawnPlayer` title tail, and deathmatch start selection from `g_game.cpp`.
