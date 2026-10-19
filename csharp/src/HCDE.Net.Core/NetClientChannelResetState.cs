namespace HCDE.Net.Core;

/// <summary>Native <c>client_t</c> fields cleared by <c>Net_ResetClientState</c> (latency, sequences, consistency).</summary>
public sealed class NetClientChannelResetState
{
    public const int ConsistencyHistoryLength = 64;
    public const int LatencyRingLength = 4;

    public uint CurrentLatency { get; set; }
    public bool NewLatency { get; set; } = true;
    public uint AverageLatency { get; set; }
    public uint[] SentTimeMs { get; } = new uint[LatencyRingLength];
    public uint[] RecvTimeMs { get; } = new uint[LatencyRingLength];
    public uint LastPacketTimeMs { get; set; }
    public uint Flags { get; set; }
    public byte MalformedPacketStrikes { get; set; }
    public uint MalformedWindowStartMs { get; set; }
    public int LastVerifiedConsistency { get; set; } = -1;
    public int[] NetConsistency { get; } = new int[ConsistencyHistoryLength];
    public int[] LocalConsistency { get; } = new int[ConsistencyHistoryLength];
    public int ConsistencyGraceUntilTic { get; set; }
    public int PassiveResendLastSequenceAck { get; set; } = -1;
    public int PassiveResendAckChangedTic { get; set; }
    public int PassiveResendLastRequestTic { get; set; }
    public int ActorDeltaReceiveCompactNextTic { get; set; }

    public void Reset()
    {
        CurrentLatency = 0;
        NewLatency = true;
        AverageLatency = 0;
        Array.Clear(SentTimeMs);
        Array.Clear(RecvTimeMs);
        LastPacketTimeMs = 0;
        Flags = 0;
        MalformedPacketStrikes = 0;
        MalformedWindowStartMs = 0;
        LastVerifiedConsistency = -1;
        Array.Clear(NetConsistency);
        Array.Clear(LocalConsistency);
        ConsistencyGraceUntilTic = 0;
        PassiveResendLastSequenceAck = -1;
        PassiveResendAckChangedTic = 0;
        PassiveResendLastRequestTic = 0;
        ActorDeltaReceiveCompactNextTic = 0;
    }
}
