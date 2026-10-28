# d_net.cpp cutscene advance + runtime join/disconnect — chunk 13 audit

## Scope

This chunk ports cutscene vote/countdown/advance (`Net_StartCutscene`, `Net_CutsceneReadyVoteSatisfied`, `Net_CheckCutsceneReady`, `Net_AdvanceCutscene`, participant gating), late-join sync/bootstrap/resync, and client disconnect / `Net_ResetClientState` bookkeeping.

**Tally baseline:** `runStart*` reset to cumulative totals at `ab80696b` so `convertedThisRunNative` reflects this agent run only.

**Conversion native LOC credited:** 1,562.

## Tests

- `CutsceneAdvanceSessionTests.StartCutscene_AppliesCountdownForNetGame`
- `CutsceneAdvanceSessionTests.VoteSatisfied_AllHumansReady_AdvancesBeforeCountdown`
- `CutsceneAdvanceSessionTests.VoteSatisfied_AnyoneMode_RequiresSingleReadyBit`
- `CutsceneAdvanceSessionTests.CountdownExpired_AdvancesWhenVoteFails`
- `CutsceneAdvanceSessionTests.AdvanceCutscene_ClearsReadyAndCountdown`
- `CutsceneAdvanceSessionTests.LateJoinParticipant_ExcludedFromVote`
- `LateJoinAndDisconnectSessionTests.LateJoinSync_PendingUntilCleared`
- `LateJoinAndDisconnectSessionTests.RuntimeBootstrap_SetsResendWindowAndPending`
- `LateJoinAndDisconnectSessionTests.Disconnect_IsIdempotentAndResetsRegistry`
- `LateJoinAndDisconnectSessionTests.ResetClientState_ClearsLateJoinAndCursors`

## Verified

`dotnet build -warnaserror`; full `dotnet test` (22,183 passed).
