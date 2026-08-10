using System.Buffers.Binary;
using HCDE.Net.Transport;

namespace HCDE.Net.Pregame;

public enum PregameServiceReceiveResult
{
    Accepted,
    Duplicate,
    Quarantined,
    TooShort,
    TokenMismatch,
    InvalidSequence,
    NotServicePacket,
}

/// <summary>
/// Ports <c>CheckHCDEPregameService</c> validation from <c>i_net.cpp</c>.
/// </summary>
public sealed class PregameServiceReceiver
{
    public PregameServiceReceiveResult TryAccept(
        ReadOnlySpan<byte> netBuffer,
        PregameConnectionState connection,
        ulong nowMilliseconds,
        int minimumSize = PregameConstants.ServiceHeaderSize)
    {
        if (connection.ServiceMalformedUntil > nowMilliseconds)
            return PregameServiceReceiveResult.Quarantined;

        if (netBuffer.Length < minimumSize)
        {
            NoteMalformed(connection, nowMilliseconds);
            return PregameServiceReceiveResult.TooShort;
        }

        if (netBuffer[PregameConstants.SetupCommandOffset] != (byte)NetCommandFlags.Setup
            || netBuffer[PregameConstants.SetupTypeOffset] != (byte)PregameSetupType.HcdeService)
            return PregameServiceReceiveResult.NotServicePacket;

        var token = BinaryPrimitives.ReadUInt32BigEndian(netBuffer[PregameConstants.SessionTokenOffset..]);
        if (connection.SessionToken != token)
        {
            NoteMalformed(connection, nowMilliseconds);
            return PregameServiceReceiveResult.TokenMismatch;
        }

        connection.ServiceLastValidRxTime = nowMilliseconds;

        var sequence = BinaryPrimitives.ReadUInt32BigEndian(netBuffer[PregameConstants.ServiceSequenceOffset..]);
        var acknowledgement = BinaryPrimitives.ReadUInt32BigEndian(netBuffer[PregameConstants.ServiceAckOffset..]);

        if (sequence == 0)
        {
            NoteMalformed(connection, nowMilliseconds);
            return PregameServiceReceiveResult.InvalidSequence;
        }

        if (acknowledgement > connection.ServicePeerAck && acknowledgement <= connection.ServiceTxSeq)
            connection.ServicePeerAck = acknowledgement;

        if (sequence <= connection.ServiceRxSeq)
        {
            connection.ServiceDuplicateCount++;
            return PregameServiceReceiveResult.Duplicate;
        }

        connection.ServiceRxSeq = sequence;
        ClearQuarantine(connection);
        return PregameServiceReceiveResult.Accepted;
    }

    private static void NoteMalformed(PregameConnectionState connection, ulong nowMilliseconds)
    {
        connection.ServiceMalformedStrikes++;
        if (connection.ServiceMalformedStrikes < PregameConstants.ServiceMalformedStrikeLimit)
            return;

        connection.ServiceMalformedStrikes = 0;
        connection.ServiceMalformedUntil = nowMilliseconds + PregameConstants.ServiceMalformedQuarantineMilliseconds;
    }

    private static void ClearQuarantine(PregameConnectionState connection)
    {
        connection.ServiceMalformedStrikes = 0;
        connection.ServiceMalformedUntil = 0;
    }
}
