# g_game.cpp headless game ticker — chunk 9 audit

## Scope

`HeadlessGameTicker` and `HeadlessGameFlow` port the authority-side `G_Ticker` game-action slice, `G_BuildTiccmd` / `PlayerCommand` ingestion, single-player `G_DoReborn` reload via `ReloadRequested`, and deathmatch forced respawn.

**Conversion native LOC credited:** 1,612.

## Tests

- `HeadlessGameFlowTests.TicCommand_QueuesOnAuthoritySimulation`
- `HeadlessGameFlowTests.ReloadRequest_RestartsMapThroughSession`
- `HeadlessGameFlowTests.DeathmatchSpawn_UsesDmStartAfterRespawn`

## Verified

`dotnet build -warnaserror`; `HeadlessGameFlowTests` (3).
