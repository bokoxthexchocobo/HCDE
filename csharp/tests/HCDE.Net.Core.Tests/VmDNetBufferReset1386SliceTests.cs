namespace HCDE.Net.Core.Tests;

public class VmDNetBufferReset1386SliceTests
{
    [Fact]
    public void MirrorPerfPrintsOnPassMultiplesAndMirrorTicDecrements()
    {
        var slice = new VmDNetBufferReset1386Slice
        {
            HudDebug = false,
            MirrorVisualPasses = 0,
            MirrorVisualMicros = 87500,
            MirrorVisualMaxMicros = 1500,
            ReplicatedActors = 6,
            PendingMirrorSpawns = 7,
            PendingSpawnEvents = 8,
            MirrorVisualWorldSteps = 2,
            ClientTic = 3,
            Gametic = 10,
            LagState = 2,
        };
        slice.FinishMirrorVisualFrame(1, 2);
        slice.HudDebug = true;
        slice.MirrorVisualPasses = 34;
        slice.FinishMirrorVisualFrame(1, 2);
        slice.MirrorVisualPasses = 0;
        slice.FinishMirrorVisualFrame(1, 2);
        slice.MirrorVisualPasses = 35;
        slice.FinishMirrorVisualFrame(4, 5);
        slice.MirrorVisualPasses = 70;
        slice.LagState = 9;
        slice.FinishMirrorVisualFrame(4, 5);
        Assert.Equal(
            [
                "HCDE mirror perf tracked=6 updated=1 skipped=2 pending-spawns=7 pending-events=8 world-steps=2 avg=0.00ms max=1.50ms backlog=-7 lag=waiting\n",
                "HCDE mirror perf tracked=6 updated=4 skipped=5 pending-spawns=7 pending-events=8 world-steps=2 avg=2.50ms max=1.50ms backlog=-7 lag=waiting\n",
                "HCDE mirror perf tracked=6 updated=4 skipped=5 pending-spawns=7 pending-events=8 world-steps=2 avg=1.25ms max=1.50ms backlog=-7 lag=unknown\n",
            ],
            slice.Logs);

        slice.LocalInvasionAuthority = true;
        slice.InvasionState = 1;
        slice.InvasionStateTics = 4;
        slice.TickInvasionMirrorState();
        Assert.Equal(4, slice.InvasionStateTics);
        slice.LocalInvasionAuthority = false;
        slice.InvasionState = VmDNetBufferReset1386Slice.InvasionDisabled;
        slice.TickInvasionMirrorState();
        Assert.Equal(4, slice.InvasionStateTics);
        slice.InvasionState = 2;
        slice.TickInvasionMirrorState();
        Assert.Equal(3, slice.InvasionStateTics);
        slice.InvasionStateTics = 1;
        slice.TickInvasionMirrorState();
        Assert.Equal(0, slice.InvasionStateTics);
        slice.TickInvasionMirrorState();
        Assert.Equal(0, slice.InvasionStateTics);
    }

    [Fact]
    public void ClearBuffersResetsPlayersAndGatesSkipReservedOrPending()
    {
        var slice = new VmDNetBufferReset1386Slice();
        Assert.False(slice.IsCutsceneReadyParticipant(-1));
        Assert.False(slice.IsTicGateClient(-1));
        Assert.False(slice.IsLateJoinSyncPending(-1));
        Assert.False(slice.IsCutsceneReadyParticipant(64));
        Assert.False(slice.IsTicGateClient(64));
        Assert.False(slice.IsLateJoinSyncPending(64));

        slice.InGame[1] = true;
        Assert.True(slice.IsCutsceneReadyParticipant(1));
        Assert.False(slice.IsTicGateClient(1));
        slice.AddNetworkClient(1);
        Assert.True(slice.IsTicGateClient(1));
        slice.LateJoinSyncPending = 1ul << 1;
        Assert.False(slice.IsCutsceneReadyParticipant(1));
        Assert.False(slice.IsTicGateClient(1));
        Assert.True(slice.IsLateJoinSyncPending(1));
        Assert.False(slice.IsLateJoinSyncPending(2));
        slice.LateJoinSyncPending = 0;
        slice.ReservedSlot = 1;
        Assert.False(slice.IsCutsceneReadyParticipant(1));
        Assert.False(slice.IsTicGateClient(1));
        slice.InGame[2] = true;
        slice.AddNetworkClient(2);
        Assert.True(slice.IsCutsceneReadyParticipant(2));
        Assert.True(slice.IsTicGateClient(2));
        slice.LateJoinSyncPending = 1ul << 63;
        slice.InGame[63] = true;
        slice.AddNetworkClient(63);
        Assert.True(slice.IsLateJoinSyncPending(63));
        Assert.False(slice.IsTicGateClient(63));

        slice.ReservedSlot = -1;
        slice.LateJoinSyncPending = 0;
        var slot = slice.Slots[3];
        slice.InGame[3] = true;
        slice.Waiting[3] = true;
        slice.Inconsistant[3] = true;
        slot.AverageLatency = 9;
        slot.CurrentLatency = 8;
        slot.NewLatency = false;
        slot.Sent[34] = 99;
        slot.Recv[0] = 98;
        slot.ResendId = 4;
        slot.StabilityBuffer = 5;
        slot.CurrentNetConsistency = slot.LastVerified = slot.ConsistencyAck = slot.ResendConsistency = 12;
        slot.CurrentSequence = slot.SequenceAck = slot.ResendSequence = slot.AppliedSequence = 12;
        slot.InputGapStallTic = 3;
        slot.SnapshotGapStallMs = -5;
        slot.Flags = 9;
        slot.TicLen[174] = 4;
        slot.TicCommand[174] = 6;
        slot.NetConsistencyMarker = 11;
        slot.LastPacketMs = 12;
        slot.Live.TxSequence = 8;
        slot.Live.Negotiated = 3;
        slot.LateTargetSequence = 2;
        slot.LateTargetConsistency = 2;
        slot.LateStartTic = 2;
        slot.BaselineRepairUntil = 5;
        slot.AuthorityReplayNext = 7;
        slot.PassiveLastAck = 4;
        slot.PassiveAckChanged = 6;
        slot.PassiveLastRequest = 8;
        slot.DeltaCompactNext = 9;
        slice.EnterTic = 40;
        slice.Gametic = 9;
        slice.ClientTic = 4;
        slice.HealthRepairValid = true;
        slice.HealthRepairHealth = 11;
        slice.LevelStartStatus = 2;
        slice.LagState = 2;
        slice.NetGame = true;
        slice.Multiplayer = true;
        slice.CommandsReset = true;
        slice.RemoteClient = 4;
        slice.MaxClients = 8;
        slice.TicDup = 3;
        slice.ConsolePlayer = 5;
        slice.NetArbitrator = 2;
        slice.NetBufferLength = 9;
        slice.LocalNetBufferSize = 7;
        slice.CurrentRoom = 9;
        slice.PendingAdoptRoom = 4;
        slice.MutedClients = 3;
        slice.CutsceneReady = 9;
        slice.CutsceneCountdown = 2;
        slice.AuthorityWaitGraceUntil = 6;
        slice.LevelStartAck = 1;
        slice.LevelStartDelay = 2;
        slice.LevelStartDebug = 3;
        slice.LastTicGateStallTrace = 4;
        slice.LastLatencyUpdate = 5;
        slice.FullLatencyCycle = 1;
        slice.PredictionEndCapTic = 3;
        slice.OutgoingGameplayFlags = 4;
        slice.LastSentConsistency = 6;
        slice.CurrentConsistency = 7;
        slice.SkipCommandTimer = 1;
        slice.SkipCommandAmount = 2;
        slice.CommandsAhead = 3;
        slice.StabilityBuffer = 4;
        slice.PrevAvailableDiff = 5;
        slice.CurStabilityTic = 4;
        slice.StabilityTics[16] = 5;
        slice.LastLiveControlMs = slice.LastSequenceRejectMs = slice.LastSnapshotRejectMs = 1;
        slice.LastTicGateReportMs = slice.LastLeadCapReportMs = slice.LastProximityDumpMs = 1;
        slice.LastFaultReportMs = slice.LastFaultBundleMs = slice.LastSoftWarnMs = 1;
        slice.LastTraceDumpMs = slice.LastMirrorVisualPassUs = 1;
        slice.AddNetworkClient(5);

        slice.ClearBuffers();

        Assert.Equal(
            [
                "CloseNetwork",
                "P_ClearPredictionData",
                "HCDEResetLocalPlayerBaselineSync",
                "ViewErrorSmoother.Zero",
                "NetEvents.ResetStream",
                "Net_ResetInvasionState:clear-buffers",
                "Net_SetLevelStartStatus:0:net-reset",
            ],
            slice.Calls);
        Assert.True(slice.InGame[0]);
        Assert.False(slice.InGame[3]);
        Assert.False(slice.Waiting[3]);
        Assert.False(slice.Inconsistant[3]);
        Assert.Equal(0, slot.AverageLatency);
        Assert.Equal(0, slot.CurrentLatency);
        Assert.True(slot.NewLatency);
        Assert.Equal(0ul, slot.Sent[34]);
        Assert.Equal(0ul, slot.Recv[0]);
        Assert.Equal(0, slot.ResendId);
        Assert.Equal(0, slot.StabilityBuffer);
        Assert.Equal(-1, slot.CurrentNetConsistency);
        Assert.Equal(-1, slot.LastVerified);
        Assert.Equal(-1, slot.ConsistencyAck);
        Assert.Equal(-1, slot.ResendConsistency);
        Assert.Equal(-1, slot.CurrentSequence);
        Assert.Equal(-1, slot.SequenceAck);
        Assert.Equal(-1, slot.ResendSequence);
        Assert.Equal(-1, slot.AppliedSequence);
        Assert.Equal(-1, slot.InputGapStallTic);
        Assert.Equal(-1, slot.SnapshotGapStallMs);
        Assert.Equal(0, slot.Flags);
        Assert.Equal(0, slot.TicLen[174]);
        Assert.Equal(6, slot.TicCommand[174]);
        Assert.Equal(11, slot.NetConsistencyMarker);
        Assert.Equal(12ul, slot.LastPacketMs);
        Assert.Equal(0u, slot.Live.TxSequence);
        Assert.Equal(0ul, slot.Live.Negotiated);
        Assert.Equal(-1, slot.LateTargetSequence);
        Assert.Equal(-1, slot.LateTargetConsistency);
        Assert.Equal(-1, slot.LateStartTic);
        Assert.Equal(0, slot.BaselineRepairUntil);
        Assert.Equal(0u, slot.AuthorityReplayNext);
        Assert.Equal(-1, slot.PassiveLastAck);
        Assert.Equal(0, slot.PassiveAckChanged);
        Assert.Equal(0, slot.PassiveLastRequest);
        Assert.Equal(0, slot.DeltaCompactNext);
        Assert.Equal(40, slice.EnterTic);
        Assert.Equal(40, slice.LastEnterTic);
        Assert.Equal(40, slice.LastGameUpdate);
        Assert.Equal(0, slice.Gametic);
        Assert.Equal(0, slice.ClientTic);
        Assert.False(slice.HealthRepairValid);
        Assert.Equal(11, slice.HealthRepairHealth);
        Assert.Equal(0, slice.LevelStartStatus);
        Assert.Equal(0, slice.LagState);
        Assert.False(slice.NetGame);
        Assert.False(slice.Multiplayer);
        Assert.False(slice.CommandsReset);
        Assert.Equal(-1, slice.RemoteClient);
        Assert.Equal(1u, slice.MaxClients);
        Assert.Equal(1u, slice.TicDup);
        Assert.Equal(0, slice.ConsolePlayer);
        Assert.Equal(0, slice.NetArbitrator);
        Assert.Equal(0u, slice.NetBufferLength);
        Assert.Equal(0u, slice.LocalNetBufferSize);
        Assert.Equal(0, slice.CurrentRoom);
        Assert.Equal(-1, slice.PendingAdoptRoom);
        Assert.Equal(0ul, slice.MutedClients);
        Assert.Equal(0ul, slice.CutsceneReady);
        Assert.Equal(0, slice.CutsceneCountdown);
        Assert.Equal(0, slice.AuthorityWaitGraceUntil);
        Assert.Equal(0u, slice.LevelStartAck);
        Assert.Equal(0, slice.LevelStartDelay);
        Assert.Equal(0, slice.LevelStartDebug);
        Assert.Equal(0, slice.LastTicGateStallTrace);
        Assert.Equal(0, slice.LastLatencyUpdate);
        Assert.Equal(105, slice.FullLatencyCycle);
        Assert.Equal(int.MaxValue, slice.PredictionEndCapTic);
        Assert.Equal(0u, slice.OutgoingGameplayFlags);
        Assert.Equal(0, slice.LastSentConsistency);
        Assert.Equal(0, slice.CurrentConsistency);
        Assert.Equal(0, slice.SkipCommandTimer);
        Assert.Equal(0, slice.SkipCommandAmount);
        Assert.Equal(0, slice.CommandsAhead);
        Assert.Equal(0, slice.StabilityBuffer);
        Assert.Equal(0, slice.PrevAvailableDiff);
        Assert.Equal(0u, slice.CurStabilityTic);
        Assert.Equal(0, slice.StabilityTics[16]);
        Assert.Equal(0ul, slice.LastLiveControlMs);
        Assert.Equal(0ul, slice.LastMirrorVisualPassUs);
        Assert.Equal(0ul, slice.LateJoinSyncPending);
        Assert.Equal([0], slice.NetworkClients);
        Assert.True(slice.IsCutsceneReadyParticipant(0));
        Assert.True(slice.IsTicGateClient(0));
        Assert.False(slice.IsCutsceneReadyParticipant(1));
        Assert.False(slice.IsTicGateClient(63));

        slice.ReservedSlot = 0;
        slice.InGame[0] = true;
        slice.LevelStartStatus = 0;
        slice.ClearBuffers();
        Assert.False(slice.InGame[0]);
        Assert.Equal([0], slice.NetworkClients);
        Assert.False(slice.IsCutsceneReadyParticipant(0));
        Assert.False(slice.IsTicGateClient(0));
        Assert.Equal(14, slice.Calls.Count);
    }
}
