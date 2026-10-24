# Chunk 405 — G_BuildTiccmd tail, LookAdjust, view pitch/yaw, ChangeSpy

## Scope

G_BuildTiccmd tail, LookAdjust, view pitch/yaw, ChangeSpy

## Native ranges

- `src/g_game.cpp` **G_BuildTiccmd tail** 1101–1128 (28 lines)
- `src/g_game.cpp` **LookAdjust** 1130–1148 (19 lines)
- `src/g_game.cpp` **G_AddViewPitch** 1150–1189 (40 lines)
- `src/g_game.cpp` **G_AddViewAngle** 1191–1204 (14 lines)
- `src/g_game.cpp` **ChangeSpy** 1208–1263 (56 lines)
- `src/g_game.cpp` **spynext/spyprev/spycancel** 1265–1281 (17 lines)

**Credited native LOC:** 174 (tally `nativeLoc` 174).

## Managed

`VmGGameBuildTiccmdTail.cs`, `VmGGameChangeSpy.cs`, `VmGGameInputTrace.cs`, `VmGGameLookAdjust.cs`, `VmGGameLookAdjustTests.cs`
Managed LOC (tally): 150.
Conversion commit: `2346f2ea666f291beab2e288ae3f85909df42c2b`.

## Tests

- `VmGGameLookAdjustTests.ScaleMovementAxis_ShiftsEightBits`
- `VmGGameLookAdjustTests.FormatInputTrace_IncludesButtonsHex`
- `VmGGameLookAdjustTests.ScaleLook_AppliesFovThenLookScale`
- `VmGGameLookAdjustTests.ClampPitchUp_StopsAtClamp`
- `VmGGameLookAdjustTests.IsSpyingDisallowed_WhenDf2Set`
- `VmGGameLookAdjustTests.StepSpyIndex_WrapsWithinMaxPlayers`
- `VmGGameLookAdjustTests.IsEligibleSpyTarget_AllowsTeammate`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 405 --to 405` (0 hard / 0 soft).

## Seams

`players`, `StatusBar`, and demo net messages are not hosted.

## Credit rationale

Credits the `G_BuildTiccmd` net tail, `LookAdjust`, `G_AddViewPitch`, `G_AddViewAngle`, and `ChangeSpy` from `g_game.cpp`.
