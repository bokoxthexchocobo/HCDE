namespace HCDE.Net.Core;

/// <summary><c>GetNetBufferSize</c>, <c>HSendPacket</c>, and <c>HGetPacket</c> policy from <c>d_net.cpp</c> (5010–5195).</summary>
public static class VmDNetGetNetBufferSize
{
    public const byte NcmdExit = 0x80;
    public const byte NcmdSetup = 0x01;
    public const byte NcmdLatency = 0x02;
    public const byte NcmdLatencyAck = 0x04;
    public const byte NcmdLevelReady = 0x08;
    public const byte NcmdQuitters = 0x10;

    public static nuint GetNetBufferSizeNative(
        ReadOnlySpan<byte> netBuffer,
        uint netBufferLength,
        bool remoteIsAuthoritySlot,
        Func<bool> liveLooksLikePacket)
    {
        if (netBufferLength == 0)
            return 0;

        var cmd = netBuffer[0];
        if ((cmd & NcmdExit) != 0)
            return (nuint)(1 + (remoteIsAuthoritySlot ? 1 : 0));

        if ((cmd & NcmdSetup) != 0)
            return netBufferLength < 2 ? 2 : netBufferLength;

        if ((cmd & (NcmdLatency | NcmdLatencyAck)) != 0)
            return 2;

        if (liveLooksLikePacket())
            return netBufferLength;

        if ((cmd & NcmdLevelReady) != 0)
        {
            var bytes = 2u;
            if (remoteIsAuthoritySlot)
                bytes += 6;
            return bytes;
        }

        const nuint headerBytes = 10;
        if (netBufferLength < headerBytes)
            return headerBytes;

        var totalBytes = headerBytes;
        if ((cmd & NcmdQuitters) != 0)
        {
            if (netBufferLength < totalBytes + 1)
                return totalBytes;
            var quitterCountBytes = (nuint)netBuffer[totalBytes] + 1;
            totalBytes += quitterCountBytes;
            if (netBufferLength < totalBytes)
                return totalBytes;
        }

        if (netBufferLength < totalBytes + 1)
            return totalBytes + 1;
        var playerCount = netBuffer[totalBytes++];
        if (netBufferLength < totalBytes + 1)
            return totalBytes + 1;
        var numTics = netBuffer[totalBytes++];
        if (numTics > 0)
        {
            totalBytes += 4;
            if (netBufferLength < totalBytes)
                return totalBytes;
        }

        if (netBufferLength < totalBytes + 1)
            return totalBytes + 1;
        var ranTics = netBuffer[totalBytes++];
        if (ranTics > 0)
        {
            totalBytes += 4;
            if (netBufferLength < totalBytes)
                return totalBytes;
        }

        totalBytes += 1;
        if (netBufferLength < totalBytes)
            return totalBytes;

        var padding = 1u;
        if (remoteIsAuthoritySlot)
            padding += 2u;
        if (netBufferLength < totalBytes + playerCount * padding)
            return totalBytes + playerCount * padding;

        return netBufferLength;
    }

    public static bool HSendPacketShouldSkip(bool demoPlayback) => demoPlayback;

    public static bool HGetPacketFromLocalBuffer(ref uint localSize, out int remoteClient)
    {
        remoteClient = 0;
        if (localSize == 0)
            return false;
        localSize = 0;
        return true;
    }
}
