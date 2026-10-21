namespace HCDE.Net.Core;

/// <summary><c>Net_TickInvasionMirrorState</c>, <c>Net_ClearBuffers</c>, and gate helpers from <c>d_net.cpp</c> (4328–4490).</summary>
public static class VmDNetClearBuffers
{
    public readonly record struct ClientSlotClearPlan(
        bool PlayingInGame,
        bool Waiting,
        bool Inconsistent,
        uint AverageLatency,
        uint CurrentLatency,
        bool NewLatency,
        uint ResendId,
        uint StabilityBuffer,
        int CurrentNetConsistency,
        int LastVerifiedConsistency,
        int ConsistencyAck,
        int ResendConsistencyFrom,
        int CurrentSequence,
        int SequenceAck,
        int ResendSequenceFrom,
        int AppliedSequence,
        int InputGapStallTic,
        int SnapshotGapStallMs,
        uint Flags,
        int LateJoinSyncTargetSequence,
        int LateJoinSyncTargetConsistency,
        int LateJoinSyncStartTic,
        uint ActorBaselineRepairUntilTic,
        uint AuthorityEventReplayNextId,
        int PassiveResendLastSequenceAck,
        int PassiveResendAckChangedTic,
        int PassiveResendLastRequestTic,
        int ActorDeltaV2ReceiveCompactNextTic);

    public readonly record struct GlobalNetClearPlan(
        uint NetBufferLength,
        int RemoteClient,
        uint MaxClients,
        uint TicDup,
        int ConsolePlayer,
        uint LocalNetBufferSize,
        int NetArbitrator,
        ulong LastLiveControlMs,
        bool PendingLocalHealthRepairValid,
        uint OutgoingGameplayFlags,
        int PredictionEndCapTic,
        int LagStateNone,
        uint MutedClients,
        ulong LateJoinSyncPending,
        uint CurrentRoomId,
        int PendingAdoptRoomId,
        bool NetGame,
        bool Multiplayer,
        int LastSentConsistency,
        int CurrentConsistency,
        int Gametic,
        int ClientTic,
        int SkipCommandTimer,
        int SkipCommandAmount,
        int CommandsAhead,
        int StabilityBuffer,
        int PrevAvailableDiff,
        uint CurStabilityTic,
        uint CutsceneReady,
        int CutsceneCountdown,
        bool CommandsReset,
        int AuthorityWaitGraceUntil,
        uint LevelStartAck,
        int LevelStartDelay,
        int LevelStartDebug,
        int LastTicGateStallTrace,
        int FullLatencyCycle,
        int LastLatencyUpdate);

    public static bool ShouldDecrementInvasionStateTics(bool localInvasionAuthority, bool invasionDisabled) =>
        !localInvasionAuthority && !invasionDisabled;

    public static int NextInvasionStateTics(int invasionStateTics) =>
        invasionStateTics > 0 ? invasionStateTics - 1 : invasionStateTics;

    public static ClientSlotClearPlan DefaultClientSlotClearPlan() =>
        new(
            PlayingInGame: false,
            Waiting: false,
            Inconsistent: false,
            AverageLatency: 0,
            CurrentLatency: 0,
            NewLatency: true,
            ResendId: 0,
            StabilityBuffer: 0,
            CurrentNetConsistency: -1,
            LastVerifiedConsistency: -1,
            ConsistencyAck: -1,
            ResendConsistencyFrom: -1,
            CurrentSequence: -1,
            SequenceAck: -1,
            ResendSequenceFrom: -1,
            AppliedSequence: -1,
            InputGapStallTic: -1,
            SnapshotGapStallMs: -1,
            Flags: 0,
            LateJoinSyncTargetSequence: -1,
            LateJoinSyncTargetConsistency: -1,
            LateJoinSyncStartTic: -1,
            ActorBaselineRepairUntilTic: 0,
            AuthorityEventReplayNextId: 0,
            PassiveResendLastSequenceAck: -1,
            PassiveResendAckChangedTic: 0,
            PassiveResendLastRequestTic: 0,
            ActorDeltaV2ReceiveCompactNextTic: 0);

    public static GlobalNetClearPlan DefaultGlobalNetClearPlan(int maxSendTics) =>
        new(
            NetBufferLength: 0,
            RemoteClient: -1,
            MaxClients: 1,
            TicDup: 1,
            ConsolePlayer: 0,
            LocalNetBufferSize: 0,
            NetArbitrator: 0,
            LastLiveControlMs: 0,
            PendingLocalHealthRepairValid: false,
            OutgoingGameplayFlags: 0,
            PredictionEndCapTic: int.MaxValue,
            LagStateNone: 0,
            MutedClients: 0,
            LateJoinSyncPending: 0,
            CurrentRoomId: 0,
            PendingAdoptRoomId: -1,
            NetGame: false,
            Multiplayer: false,
            LastSentConsistency: 0,
            CurrentConsistency: 0,
            Gametic: 0,
            ClientTic: 0,
            SkipCommandTimer: 0,
            SkipCommandAmount: 0,
            CommandsAhead: 0,
            StabilityBuffer: 0,
            PrevAvailableDiff: 0,
            CurStabilityTic: 0,
            CutsceneReady: 0,
            CutsceneCountdown: 0,
            CommandsReset: false,
            AuthorityWaitGraceUntil: 0,
            LevelStartAck: 0,
            LevelStartDelay: 0,
            LevelStartDebug: 0,
            LastTicGateStallTrace: 0,
            FullLatencyCycle: maxSendTics * 3,
            LastLatencyUpdate: 0);

    public static bool ShouldSetConsolePlayerInGameOnClear(bool serverReservedSlot0) =>
        !serverReservedSlot0;

    public static bool IsCutsceneReadyParticipant(
        int player,
        int maxPlayers,
        bool playingInGame,
        bool serverReservedSlot,
        bool lateJoinPending) =>
        player >= 0
        && player < maxPlayers
        && playingInGame
        && !serverReservedSlot
        && !lateJoinPending;

    public static bool IsTicGateClient(
        int player,
        int maxPlayers,
        bool networkClientInGame,
        bool serverReservedSlot,
        bool lateJoinPending) =>
        player >= 0
        && player < maxPlayers
        && networkClientInGame
        && !serverReservedSlot
        && !lateJoinPending;

    public static bool IsLateJoinSyncPending(int client, int maxPlayers, ulong lateJoinSyncPending) =>
        client >= 0
        && client < maxPlayers
        && (lateJoinSyncPending & (1ul << client)) != 0ul;
}
