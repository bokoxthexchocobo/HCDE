namespace HCDE.Net.Core;

/// <summary>Whole <c>GetPacket</c> (<c>i_net.cpp</c> 2119–2335).</summary>
public static class VmINetGetPacket
{
    public const byte NcmdCompressed = 0x20;
    public const byte NcmdSetup = 0x80;
    public const byte NcmdExit = 0x04;
    public const int SocketError = -1;
    public const int MinDatagramBytes = 5;
    public const int TransmitCrcBytes = 4;

    public static bool DatagramTooShort(int msgSize) => msgSize < MinDatagramBytes;

    public static bool UnknownClientRequiresSetup(byte payloadCmd) => (payloadCmd & NcmdSetup) == 0;

    public static bool PayloadIsCompressed(byte cmdByte) => (cmdByte & NcmdCompressed) != 0;

    public static byte StripCompressionFlag(byte cmdByte) => (byte)(cmdByte & ~NcmdCompressed);

    public static bool LateJoinAdmissionAllowed(bool authority, bool dedicatedServer, bool lateJoinEnabled) =>
        authority && (dedicatedServer || lateJoinEnabled);

    public static void GetPacketNative(GetPacketNativeContext ctx)
    {
        var recv = ctx.RecvFrom?.Invoke(ctx.TransmitBuffer, ctx.MaxTransmitSize) ?? (SocketError, default);
        var msgSize = recv.Size;
        var fromAddr = recv.From;
        var client = ctx.FindClient?.Invoke(fromAddr) ?? -1;

        if (client >= 0 && msgSize == SocketError)
            (client, msgSize) = HandleKnownClientSocketError(ctx, client);
        else if (msgSize > 0)
            (client, msgSize) = ProcessInboundDatagram(ctx, fromAddr, client, msgSize);
        else
            client = -1;

        ctx.RemoteClient = client;
        ctx.NetBufferLength = Math.Max(msgSize, 0);
        if (ctx.NetBufferLength > ctx.MaxMsgLen)
        {
            ctx.IncrementPacketOversized?.Invoke();
            ctx.NetBufferLength = ctx.MaxMsgLen;
        }

        if (ctx.CaptureFromAddress)
            ctx.FromAddress = fromAddr;
    }

    static (int client, int msgSize) HandleKnownClientSocketError(GetPacketNativeContext ctx, int client)
    {
        var err = ctx.GetLastSocketError?.Invoke() ?? 0;
        if (err == ctx.WsaConnReset)
        {
            if (ctx.ConsolePlayer == -1)
                return (-1, 0);

            ctx.NetLog?.Invoke($"Connection reset by client {client}");
            ctx.DebugWarning?.Invoke(client);
            if (ctx.IsResetFromReservedServerSlot?.Invoke(client) == true)
                return (-1, 0);
            if (ctx.IsHcdeServiceAuthoritySlot?.Invoke(client) == true)
            {
                ctx.FatalNetError?.Invoke("Authority unexpectedly disconnected");
                return (client, SocketError);
            }

            ctx.NetBuffer[0] = NcmdExit;
            return (client, 1);
        }

        if (err != ctx.WsaWouldBlock)
        {
            ctx.FatalError?.Invoke("Failed to get packet");
            return (client, SocketError);
        }

        return (-1, 0);
    }

    static (int client, int msgSize) ProcessInboundDatagram(
        GetPacketNativeContext ctx,
        (uint Address, ushort Port) fromAddr,
        int client,
        int msgSize)
    {
        ctx.IncrementPacketReceived?.Invoke();
        if (ctx.TryHandleServerQuery?.Invoke(fromAddr, ctx.TransmitBuffer, msgSize) == true)
        {
            ctx.RemoteClient = -1;
            ctx.NetBufferLength = 0;
            if (ctx.CaptureFromAddress)
                ctx.FromAddress = fromAddr;
            return (-1, 0);
        }

        if (DatagramTooShort(msgSize))
        {
            ctx.IncrementPacketTooShort?.Invoke();
            return (-1, 0);
        }

        var dataStart = TransmitCrcBytes;
        var payloadSize = msgSize - TransmitCrcBytes;
        if (client == -1 && UnknownClientRequiresSetup(ctx.TransmitBuffer[dataStart]))
            return (-1, 0);

        if (!VerifyPacketCrc(ctx, dataStart, payloadSize, out var setup))
            return (-1, 0);

        ctx.NetBuffer[0] = StripCompressionFlag(ctx.TransmitBuffer[dataStart]);
        if (PayloadIsCompressed(ctx.TransmitBuffer[dataStart]))
        {
            if (!TryDecodeCompressed(ctx, dataStart, msgSize, payloadSize, out msgSize))
                return (-1, 0);
        }
        else if (!TryCopyUncompressed(ctx, dataStart, payloadSize, out msgSize))
            return (-1, 0);

        if (client == -1 && (ctx.NetBuffer[0] & NcmdSetup) != 0 && msgSize >= 2)
            ctx.MarkInboundSetup?.Invoke(fromAddr, ctx.NetBuffer[1], msgSize);

        if (client == -1 && ctx.GameStarted)
            return TryLateJoinAdmission(ctx, fromAddr, client, msgSize);

        return (client, msgSize);
    }

    static bool VerifyPacketCrc(GetPacketNativeContext ctx, int dataStart, int payloadSize, out bool setup)
    {
        setup = (ctx.TransmitBuffer[dataStart] & NcmdSetup) != 0;
        var payload = ctx.TransmitBuffer.AsSpan(dataStart, payloadSize).ToArray();
        var check = setup
            ? ctx.CalcCrc32?.Invoke(payload, payloadSize) ?? 0
            : ctx.AddCrc32?.Invoke(ctx.CalcCrc32?.Invoke(payload, payloadSize) ?? 0, ctx.GameId) ?? 0;
        var crc = ReadTransmitCrc(ctx.TransmitBuffer);
        if (check == crc)
            return true;

        ctx.IncrementPacketBadCrc?.Invoke();
        return false;
    }

    static uint ReadTransmitCrc(byte[] buffer) =>
        (uint)(buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3]);

    static bool TryDecodeCompressed(
        GetPacketNativeContext ctx,
        int dataStart,
        int msgSize,
        int payloadSize,
        out int decodedSize)
    {
        decodedSize = 0;
        if (payloadSize <= 1)
        {
            ctx.IncrementCompressedMalformed?.Invoke();
            return false;
        }

        var decompressed = ctx.Uncompress?.Invoke(
            ctx.TransmitBuffer.AsSpan(dataStart + 1, msgSize - 5).ToArray(),
            msgSize - 5,
            ctx.NetBuffer,
            ctx.NetBuffer.Length) ?? -1;
        if (decompressed < 0)
        {
            ctx.IncrementDecompressFailure?.Invoke();
            return false;
        }

        decodedSize = decompressed + 1;
        return true;
    }

    static bool TryCopyUncompressed(
        GetPacketNativeContext ctx,
        int dataStart,
        int payloadSize,
        out int outSize)
    {
        outSize = 0;
        var copySize = payloadSize - 1;
        if (copySize >= ctx.MaxMsgLen || copySize >= ctx.MaxTransmitSize)
        {
            ctx.IncrementPacketOversized?.Invoke();
            return false;
        }

        outSize = payloadSize;
        if (copySize > 0)
            ctx.TransmitBuffer.AsSpan(dataStart + 1, copySize).CopyTo(ctx.NetBuffer.AsSpan(1));
        return true;
    }

    static (int client, int msgSize) TryLateJoinAdmission(
        GetPacketNativeContext ctx,
        (uint Address, ushort Port) fromAddr,
        int client,
        int msgSize)
    {
        var authority = ctx.IsLocalHcdeServiceAuthority?.Invoke() == true;
        var admissionAllowed = LateJoinAdmissionAllowed(authority, ctx.DedicatedServerMode, ctx.LateJoinEnabled);
        ctx.NetBufferLength = Math.Max(msgSize, 0);
        var processed = admissionAllowed && ctx.TryProcessSetupConnect?.Invoke(fromAddr) == true;
        if (processed)
            client = ctx.FindClient?.Invoke(fromAddr) ?? -1;
        if (client == -1 && !processed)
        {
            ctx.NetBuffer[0] = NcmdSetup;
            ctx.NetBuffer[1] = ctx.PreInProgress;
            ctx.NetBufferLength = 2;
            ctx.SendPacket?.Invoke(fromAddr);
            msgSize = 0;
        }

        return (client, msgSize);
    }
}
