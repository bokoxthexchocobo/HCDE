# g_level.cpp headless level flow — chunk 8 audit

## Scope

Managed `HeadlessGameSession`, `HeadlessMapExit`, `HeadlessLevelTravel`, and `ClusterTextResolver` intermission handoff for dedicated authority: `G_InitNew` / `G_DoLoadLevel` / `ChangeLevel` / `G_DoCompleted` / intermission / `G_DoWorldDone`, plus co-op `StartTravel` / `FinishTravel` player snapshots.

**Conversion native LOC credited:** 1,588 (see `loc_tally.json` `g-level-headless-flow-chunk8`).

## Tests

- `HeadlessLevelFlowTests.InitNew_LoadsMapAndTicks`
- `HeadlessLevelFlowTests.ExitWithMapInfo_TransitionsToNextMapAfterIntermission`
- `HeadlessLevelFlowTests.CoopTravel_PreservesPlayerHealthAcrossLoad`
- `HeadlessLevelFlowTests.ShouldDoIntermission_SkipsHubHubWhenClusterDisallows`

## Verified

`dotnet build -warnaserror`; `HeadlessLevelFlowTests` (4); full `HCDE.Playsim.Tests` after chunk.
