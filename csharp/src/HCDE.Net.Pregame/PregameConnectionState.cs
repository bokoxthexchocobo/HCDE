namespace HCDE.Net.Pregame;

public sealed class PregameConnectionState
{
    public uint SessionToken { get; set; }
    public uint ServiceTxSeq { get; set; }
    public uint ServiceRxSeq { get; set; }
    public uint ServicePeerAck { get; set; }
    public uint ServiceDuplicateCount { get; set; }
    public uint ServiceMalformedStrikes { get; set; }
    public ulong ServiceMalformedUntil { get; set; }
    public ulong ServiceLastValidRxTime { get; set; }
    public ulong RuntimeLastConnectAckTime { get; set; }

    public uint AllocateServiceSequence() => ++ServiceTxSeq;

    public void Reset()
    {
        SessionToken = 0;
        ServiceTxSeq = 0;
        ServiceRxSeq = 0;
        ServicePeerAck = 0;
        ServiceDuplicateCount = 0;
        ServiceMalformedStrikes = 0;
        ServiceMalformedUntil = 0;
        ServiceLastValidRxTime = 0;
        RuntimeLastConnectAckTime = 0;
    }
}
