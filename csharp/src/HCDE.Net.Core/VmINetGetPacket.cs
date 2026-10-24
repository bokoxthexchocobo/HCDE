namespace HCDE.Net.Core;

/// <summary>Whole <c>GetPacket</c> (<c>i_net.cpp</c> 2119–2335).</summary>
public static class VmINetGetPacket
{
    public const byte NcmdCompressed = 0x20;
    public const byte NcmdSetup = 0x80;
    public const byte NcmdExit = 0x04;
    public const int SocketError = -1;

    public static void GetPacketNative(GetPacketNativeContext ctx)
    {
        var recv = ctx.RecvFrom?.Invoke(ctx.TransmitBuffer, ctx.MaxTransmitSize) ?? (SocketError, default);
        var msgSize = recv.Size;
        var fromAddr = recv.From;
        var client = ctx.FindClient?.Invoke(fromAddr) ?? -1;

        if (client >= 0 && msgSize == SocketError)
        {
            var err = ctx.GetLastSocketError?.Invoke() ?? 0;
            if (err == ctx.WsaConnReset)
            {
                if (ctx.ConsolePlayer == -1)
                {
                    client = -1;
                    msgSize = 0;
                }
                else
                {
                    ctx.NetLog?.Invoke($"Connection reset by client {client}");
                    ctx.DebugWarning?.Invoke(client);
                    if (ctx.IsResetFromReservedServerSlot?.Invoke(client) == true)
                    {
                        client = -1;
                        msgSize = 0;
                    }
                    else if (ctx.IsHcdeServiceAuthoritySlot?.Invoke(client) == true)
                        ctx.FatalNetError?.Invoke("Authority unexpectedly disconnected");
                    else
                    {
                        ctx.NetBuffer[0] = NcmdExit;
                        msgSize = 1;
                    }
                }
            }
            else if (err != ctx.WsaWouldBlock)
                ctx.FatalError?.Invoke("Failed to get packet");
            else
            {
                client = -1;
                msgSize = 0;
            }
        }
        else if (msgSize > 0)
        {
            ctx.IncrementPacketReceived?.Invoke();
            if (ctx.TryHandleServerQuery?.Invoke(fromAddr, ctx.TransmitBuffer, msgSize) == true)
            {
                ctx.RemoteClient = -1;
                ctx.NetBufferLength = 0;
                if (ctx.CaptureFromAddress)
                    ctx.FromAddress = fromAddr;
                return;
            }
            if (msgSize < 5)
            {
                ctx.IncrementPacketTooShort?.Invoke();
                client = -1;
                msgSize = 0;
            }
            else
            {
                var dataStart = 4;
                var payloadSize = msgSize - 4;
                if (client == -1 && (ctx.TransmitBuffer[dataStart] & NcmdSetup) == 0)
                    msgSize = 0;
                else
                {
                    var setup = (ctx.TransmitBuffer[dataStart] & NcmdSetup) != 0;
                    var payload = ctx.TransmitBuffer.AsSpan(dataStart, payloadSize).ToArray();
                    var check = setup
                        ? ctx.CalcCrc32?.Invoke(payload, payloadSize) ?? 0
                        : ctx.AddCrc32?.Invoke(ctx.CalcCrc32?.Invoke(payload, payloadSize) ?? 0, ctx.GameId) ?? 0;
                    var crc = (uint)(ctx.TransmitBuffer[0] << 24 | ctx.TransmitBuffer[1] << 16
                        | ctx.TransmitBuffer[2] << 8 | ctx.TransmitBuffer[3]);
                    if (check != crc)
                    {
                        ctx.IncrementPacketBadCrc?.Invoke();
                        client = -1;
                        msgSize = 0;
                    }
                    else
                    {
                        ctx.NetBuffer[0] = (byte)(ctx.TransmitBuffer[dataStart] & ~NcmdCompressed);
                        if ((ctx.TransmitBuffer[dataStart] & NcmdCompressed) != 0)
                        {
                            if (payloadSize <= 1)
                            {
                                ctx.IncrementCompressedMalformed?.Invoke();
                                client = -1;
                                msgSize = 0;
                            }
                            else
                            {
                                var decompressed = ctx.Uncompress?.Invoke(
                                    ctx.TransmitBuffer.AsSpan(dataStart + 1, msgSize - 5).ToArray(),
                                    msgSize - 5,
                                    ctx.NetBuffer,
                                    ctx.NetBuffer.Length) ?? -1;
                                if (decompressed < 0)
                                {
                                    ctx.IncrementDecompressFailure?.Invoke();
                                    client = -1;
                                    msgSize = 0;
                                }
                                else
                                    msgSize = decompressed + 1;
                            }
                        }
                        else
                        {
                            var copySize = payloadSize - 1;
                            if (copySize >= ctx.MaxMsgLen || copySize >= ctx.MaxTransmitSize)
                            {
                                ctx.IncrementPacketOversized?.Invoke();
                                client = -1;
                                msgSize = 0;
                            }
                            else
                            {
                                msgSize = payloadSize;
                                if (copySize > 0)
                                    ctx.TransmitBuffer.AsSpan(dataStart + 1, copySize)
                                        .CopyTo(ctx.NetBuffer.AsSpan(1));
                            }
                        }

                        if (client == -1 && (ctx.NetBuffer[0] & NcmdSetup) != 0 && msgSize >= 2)
                            ctx.MarkInboundSetup?.Invoke(fromAddr, ctx.NetBuffer[1], msgSize);

                        if (client == -1 && ctx.GameStarted)
                        {
                            var authority = ctx.IsLocalHcdeServiceAuthority?.Invoke() == true;
                            var listenLateJoin = !ctx.DedicatedServerMode && ctx.LateJoinEnabled;
                            var admissionAllowed = authority && (ctx.DedicatedServerMode || listenLateJoin);
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
                        }
                    }
                }
            }
        }
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
}
