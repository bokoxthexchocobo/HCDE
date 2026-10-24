# Chunk 509 — P_PredictClient

## Scope

P_PredictClient

## Native ranges

- `src/playsim/p_user.cpp` **P_PredictClient** 2021–2394 (374 lines)

**Credited native LOC:** 374 (tally `nativeLoc` 374).

## Managed

`VmPUserPredictClient.cs`, `VmPUserPredictClientTests.cs`
Managed LOC (tally): 111.
Conversion commit: `a2db518bc082ac776a84b7932ea9ddd406712801`.

## Tests

- `VmPUserPredictClientTests.SinglePlayer_DisablesPrediction`
- `VmPUserPredictClientTests.Replay_SkipsWhenCaughtUp`
- `VmPUserPredictClientTests.Window_ClampReducesStart`
- `VmPUserPredictClientTests.Rubberband_TriggersOnDistance`
- `VmPUserPredictClientTests.TryBuildWindow_CapsEnd`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 509 --to 509` (0 hard / 0 soft).

## Seams

`P_PlayerThink`, `G_Ticker`, `NetworkEntityManager`, serializer rollback, `HCDESeatLocalPlayerToAuthoritativeBase`, and the per-tic replay loop body are not hosted.

## Credit rationale

Credits netgame/level/reserved-slot gates, prediction window clamping, rubberband eligibility, weapon psprite watermarking, and replay skip conditions from `p_user.cpp` (2021–2394).
