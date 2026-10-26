# Chunk 509 — P_PredictClient

## Scope

P_PredictClient

## Native ranges

- `src/playsim/p_user.cpp` **P_PredictClient** 2021–2394 (374 lines)

**Credited native LOC:** 374 (tally `nativeLoc` 374).

## Managed

`VmPUserPredictClient.cs`
Managed LOC (tally): 481.
Conversion commit: `242fb2f66837ec7217d737e5921649e445eddf10`.

## Padding review (2026-10-21)

| Managed | Native (`p_user.cpp`) |
|---------|-------------------|
| `RunPredictClient` / `RunEntryGates` / `PredictClientContext` / `SinglePlayerTearsDownPrediction` / `SkipWhenNotInLevel` / `SkipReservedServerSlot` / `ConsolePlayerOutOfRange` / `ShouldEnablePrediction` / `SkipReplayWhenCaughtUp` / `SkipReplayWhenNotLive` / `SkipReplayWhenJustSpawned` / `ClampPredictionEnd` / `EmptyPredictionWindow` / `ClampPredictionStart` / `CanRubberband` / `RubberbandDistanceExceeded` / `RubberbandHitLimit` / `ReseedWeaponPspriteTic` / `AdvanceWeaponPspriteWhenPredicting` / `WeaponPspriteTicWatermark` / `ShouldClearPredictingCheat` / `ShouldSkipPredictionReplay` / `TryBuildPredictionWindow` / `EvaluateRubberband` / `LastPredictedFromBase` / `RecordReplayDiag` / `AccumulateReplayYaw` / `ApplyPortalOffsetWhenSectorPresent` / `LocalCmdIndex` / `WeaponTickForIndex` / `RubberbandLerpPath` / `PausedSetsNoInterpolate` / `FoldYawDegrees` / `ReplayInputDegreesFromBam` / `PhantomYawDegrees` / `ShouldLogYawDiag` / `ShouldLogMoveDiag` / `ShouldLogPitchSummary` / `PitchClampHit` / `NonZeroReplayPitch` / `CountZeroedPitch` / `AccumulateReplayCmd` / `FinalizePredictionTick` | `P_PredictClient` 2021–2394 (2023: `if (!netgame)`; 2026: `if (consoleplayer >= 0 && consoleplayer < MAXPLAYERS)`; 2030: `return;`) |

### Out-of-body uncredit (509)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **374** |
| **Out-of-body removed** | **0** |
| **Honest** | **374** |


**Post-review ratio:** 481/374 managed/native ≈ 129% (src file LOC at HEAD; see tally).

## Tests

- `VmPUserPredictClientTests.SinglePlayer_DisablesPrediction`
- `VmPUserPredictClientTests.Replay_SkipsWhenCaughtUp`
- `VmPUserPredictClientTests.Window_ClampReducesStart`
- `VmPUserPredictClientTests.Rubberband_TriggersOnDistance`
- `VmPUserPredictClientTests.TryBuildWindow_CapsEnd`
- `VmPUserPredictClientTests.RunEntryGates_SinglePlayerTearsDown`
- `VmPUserPredictClientTests.RunPredictClient_ReplaysWindow`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 509 --to 509` (0 hard / 0 soft).

## Seams

`P_PlayerThink`, `G_Ticker`, `NetworkEntityManager`, serializer rollback, `HCDESeatLocalPlayerToAuthoritativeBase`, and the per-tic replay loop body are not hosted.

## Credit rationale

Credits netgame/level/reserved-slot gates, prediction window clamping, rubberband eligibility, weapon psprite watermarking, and replay skip conditions from `p_user.cpp` (2021–2394).
