namespace HCDE.Net.Core;

/// <summary>
/// Net buffer reset and mirror tail from <c>d_net.cpp</c> 4300-4493.
/// The two includes splice other files. <c>Net_SetLateJoinSyncPending</c>'s body stays outside this band.
/// </summary>
public sealed class VmDNetBufferReset1386Slice
{
    public const int MaxPlayers = 64;
    public const int BackupTics = 35 * 5;
    public const int MaxSendTics = 35;
    public const int StabilityTicCount = 17;
    public const int InvasionDisabled = 0;

    public sealed class Peer
    {
        public uint TxSequence;
        public ulong Negotiated;
        public void Clear() { TxSequence = 0; Negotiated = 0; }
    }

    public sealed class Slot
    {
        public ushort AverageLatency, CurrentLatency;
        public bool NewLatency = true;
        public ulong[] Sent = new ulong[MaxSendTics];
        public ulong[] Recv = new ulong[MaxSendTics];
        public byte ResendId, StabilityBuffer;
        public int CurrentNetConsistency = -1, LastVerified = -1, ConsistencyAck = -1, ResendConsistency = -1;
        public int CurrentSequence = -1, SequenceAck = -1, ResendSequence = -1, AppliedSequence = -1;
        public int InputGapStallTic = -1;
        public long SnapshotGapStallMs = -1;
        public int Flags;
        public int[] TicLen = new int[BackupTics];
        public int[] TicCommand = new int[BackupTics];
        public int NetConsistencyMarker;
        public ulong LastPacketMs;
        public Peer Live = new();
        public int LateTargetSequence = -1, LateTargetConsistency = -1, LateStartTic = -1;
        public int BaselineRepairUntil;
        public uint AuthorityReplayNext;
        public int PassiveLastAck = -1, PassiveAckChanged, PassiveLastRequest, DeltaCompactNext;

        public void Reset()
        {
            AverageLatency = CurrentLatency = 0;
            Array.Fill(Sent, 0ul);
            Array.Fill(Recv, 0ul);
            NewLatency = true;
            ResendId = StabilityBuffer = 0;
            CurrentNetConsistency = LastVerified = ConsistencyAck = ResendConsistency = -1;
            CurrentSequence = SequenceAck = ResendSequence = -1;
            AppliedSequence = -1;
            InputGapStallTic = -1;
            SnapshotGapStallMs = -1;
            Flags = 0;
            for (var j = 0; j < BackupTics; j++)
                TicLen[j] = 0;
            LateTargetSequence = LateTargetConsistency = LateStartTic = -1;
            BaselineRepairUntil = 0;
            AuthorityReplayNext = 0;
            PassiveLastAck = -1;
            PassiveAckChanged = PassiveLastRequest = DeltaCompactNext = 0;
            Live.Clear();
        }
    }

    public readonly List<string> Logs = new();
    public readonly List<string> Calls = new();
    public readonly Slot[] Slots = new Slot[MaxPlayers];
    public readonly bool[] InGame = new bool[MaxPlayers];
    public readonly bool[] Waiting = new bool[MaxPlayers];
    public readonly bool[] Inconsistant = new bool[MaxPlayers];
    public readonly List<int> NetworkClients = new();
    public readonly int[] StabilityTics = new int[StabilityTicCount];
    public bool HudDebug = true;
    public bool LocalInvasionAuthority;
    public int InvasionState = InvasionDisabled;
    public int InvasionStateTics;
    public ulong MirrorVisualPasses, MirrorVisualMicros, MirrorVisualMaxMicros;
    public int ReplicatedActors, PendingMirrorSpawns, PendingSpawnEvents, MirrorVisualWorldSteps = 1;
    public int ClientTic, Gametic, EnterTic, LastEnterTic, LastGameUpdate;
    public int LagState;
    public int ReservedSlot = -1;
    public ulong LateJoinSyncPending, CutsceneReady, MutedClients;
    public bool HealthRepairValid;
    public int HealthRepairHealth;
    public int LevelStartStatus;
    public uint NetBufferLength, MaxClients, TicDup = 1, LocalNetBufferSize, CurStabilityTic;
    public int RemoteClient, ConsolePlayer, NetArbitrator, CurrentRoom, PendingAdoptRoom = -1;
    public bool NetGame, Multiplayer, CommandsReset;
    public int LastSentConsistency, CurrentConsistency;
    public int SkipCommandTimer, SkipCommandAmount, CommandsAhead, StabilityBuffer, PrevAvailableDiff;
    public int CutsceneCountdown, AuthorityWaitGraceUntil, LevelStartDelay, LevelStartDebug, LastTicGateStallTrace, LastLatencyUpdate;
    public uint LevelStartAck;
    public int FullLatencyCycle = MaxSendTics * 3;
    public int PredictionEndCapTic = int.MaxValue;
    public uint OutgoingGameplayFlags;
    public ulong LastLiveControlMs, LastSequenceRejectMs, LastSnapshotRejectMs, LastTicGateReportMs;
    public ulong LastLeadCapReportMs, LastProximityDumpMs, LastFaultReportMs, LastFaultBundleMs;
    public ulong LastSoftWarnMs, LastTraceDumpMs, LastMirrorVisualPassUs;

    public VmDNetBufferReset1386Slice()
    {
        for (var i = 0; i < MaxPlayers; i++)
            Slots[i] = new Slot();
    }

    public void FinishMirrorVisualFrame(uint updated, uint skipped)
    {
        if (!HudDebug || MirrorVisualPasses % 35 != 0)
            return;
        var avg = MirrorVisualPasses > 0 ? (double)MirrorVisualMicros / MirrorVisualPasses / 1000.0 : 0.0;
        var max = (double)MirrorVisualMaxMicros / 1000.0;
        Logs.Add("HCDE mirror perf tracked=" + ReplicatedActors
            + " updated=" + updated
            + " skipped=" + skipped
            + " pending-spawns=" + PendingMirrorSpawns
            + " pending-events=" + PendingSpawnEvents
            + " world-steps=" + MirrorVisualWorldSteps
            + " avg=" + Fixed2(avg) + "ms max=" + Fixed2(max) + "ms backlog=" + (ClientTic - Gametic)
            + " lag=" + LagName() + "\n");
    }

    public void TickInvasionMirrorState()
    {
        if (LocalInvasionAuthority || InvasionState == InvasionDisabled)
            return;
        if (InvasionStateTics > 0)
            InvasionStateTics--;
    }

    public void ClearBuffers()
    {
        Calls.Add("CloseNetwork");
        for (var i = 0; i < MaxPlayers; i++)
        {
            InGame[i] = false;
            Waiting[i] = Inconsistant[i] = false;
            Slots[i].Reset();
        }
        Calls.Add("P_ClearPredictionData");
        NetBufferLength = 0;
        RemoteClient = -1;
        MaxClients = TicDup = 1;
        ConsolePlayer = 0;
        LocalNetBufferSize = 0;
        NetArbitrator = 0;
        LastLiveControlMs = LastSequenceRejectMs = LastSnapshotRejectMs = LastTicGateReportMs = 0;
        LastLeadCapReportMs = LastProximityDumpMs = LastFaultReportMs = LastFaultBundleMs = 0;
        LastSoftWarnMs = LastTraceDumpMs = LastMirrorVisualPassUs = 0;
        HealthRepairValid = false;
        Calls.Add("HCDEResetLocalPlayerBaselineSync");
        OutgoingGameplayFlags = 0;
        PredictionEndCapTic = int.MaxValue;
        Calls.Add("ViewErrorSmoother.Zero");
        LagState = 0;
        MutedClients = 0;
        LateJoinSyncPending = 0;
        CurrentRoom = 0;
        PendingAdoptRoom = -1;
        NetworkClients.Clear();
        NetGame = Multiplayer = false;
        LastSentConsistency = CurrentConsistency = 0;
        LastEnterTic = LastGameUpdate = EnterTic;
        Gametic = ClientTic = 0;
        SkipCommandTimer = SkipCommandAmount = CommandsAhead = 0;
        StabilityBuffer = PrevAvailableDiff = 0;
        CurStabilityTic = 0;
        Array.Fill(StabilityTics, 0);
        Calls.Add("NetEvents.ResetStream");
        CutsceneReady = 0;
        CutsceneCountdown = 0;
        Calls.Add("Net_ResetInvasionState:clear-buffers");
        CommandsReset = false;
        AuthorityWaitGraceUntil = 0;
        LevelStartAck = 0;
        LevelStartDelay = LevelStartDebug = 0;
        Calls.Add("Net_SetLevelStartStatus:0:net-reset");
        if (LevelStartStatus != 0)
            LevelStartStatus = 0;
        LastTicGateStallTrace = 0;
        FullLatencyCycle = MaxSendTics * 3;
        LastLatencyUpdate = 0;
        if (!IsReserved(0))
            InGame[0] = true;
        AddNetworkClient(0);
    }

    public bool IsCutsceneReadyParticipant(int player) =>
        player >= 0 && player < MaxPlayers && InGame[player] && !IsReserved(player) && !IsLateJoinSyncPending(player);

    public bool IsTicGateClient(int player)
    {
        if (player < 0 || player >= MaxPlayers || !NetworkClients.Contains(player))
            return false;
        if (IsReserved(player))
            return false;
        if ((LateJoinSyncPending & (1ul << player)) != 0)
            return false;
        return true;
    }

    public bool IsLateJoinSyncPending(int client) =>
        client >= 0 && client < MaxPlayers && (LateJoinSyncPending & (1ul << client)) != 0;

    public void AddNetworkClient(int client)
    {
        if (!NetworkClients.Contains(client))
        {
            NetworkClients.Add(client);
            NetworkClients.Sort();
        }
    }

    bool IsReserved(int client) => ReservedSlot >= 0 && client == ReservedSlot;

    string LagName() => LagState switch
    {
        0 => "none",
        1 => "predicting",
        2 => "waiting",
        3 => "skipping",
        _ => "unknown",
    };

    static string Fixed2(double value)
    {
        var scaled = (long)Math.Round(value * 100, MidpointRounding.AwayFromZero);
        var whole = scaled / 100;
        var frac = Math.Abs(scaled % 100);
        return whole + "." + (frac < 10 ? "0" : "") + frac;
    }
}
