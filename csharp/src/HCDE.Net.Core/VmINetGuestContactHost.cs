namespace HCDE.Net.Core;

/// <summary>Guest HCDE service tail and per-tick send (<c>i_net.cpp</c> 3951–4179).</summary>
public static class VmINetGuestContactHost
{
    private const byte HpsGameInfo = 4;
    private const byte HpsRoster = 6;
    private const byte HpsStartGame = 7;
    private const byte HpsBootstrapBegin = 13;
    public const int GuestRosterAddrSize = 6;

    public static (int PayloadSize, int StreamSize) GameInfoPayloadSizes(int netBufferLength, int payloadOffset, int maxMsgLen)
    {
        var payloadSize = netBufferLength > payloadOffset ? netBufferLength - payloadOffset : 0;
        var streamSize = payloadSize;
        if (payloadSize > 0 && payloadOffset + payloadSize < maxMsgLen)
            streamSize = payloadSize + 1;
        return (payloadSize, streamSize);
    }

    public static bool ValidateRosterClientId(int client, int maxClients, int maxPlayers) =>
        client >= 0 && client < maxClients && client < maxPlayers;

    public static int ReadBigEndianUInt16(byte[] buffer, int offset) =>
        (buffer[offset] << 8) | buffer[offset + 1];

    public static bool RosterBufferHasBytes(int netBufferLength, int cursor, int need) =>
        netBufferLength >= cursor + need;

    public static bool ShouldRearmGuestSetupWatchdog(ulong lastProgressTime, uint lastRxSeq, uint currentRxSeq) =>
        lastProgressTime == 0 || currentRxSeq != lastRxSeq;

    public static bool GuestSetupWatchdogTimedOut(ulong now, ulong lastProgressTime, ulong timeoutMs) =>
        lastProgressTime != 0 && now - lastProgressTime >= timeoutMs;

    public static void RunGuestSetupWatchdog(GuestContactHostContext ctx)
    {
        if (ctx.IsHcdeConnect?.Invoke() != true)
            return;
        var now = ctx.MsTime?.Invoke() ?? 0;
        var rxSeq = ctx.HostHcdeServiceRxSeq;
        if (ShouldRearmGuestSetupWatchdog(ctx.GuestLastSetupProgressTime, ctx.GuestLastSetupRxSeq, rxSeq))
        {
            ctx.GuestLastSetupProgressTime = now;
            ctx.GuestLastSetupRxSeq = rxSeq;
        }
        else if (GuestSetupWatchdogTimedOut(now, ctx.GuestLastSetupProgressTime, ctx.HcdeGuestSetupProgressTimeoutMs))
        {
            ctx.DebugMark?.Invoke($"guest HCDE setup timed out waiting for progress last-rx={ctx.GuestLastSetupRxSeq}");
            ctx.FatalNetError?.Invoke("Timed out waiting for HCDE late-join setup. Try reconnecting.");
        }
    }

    public static int ClampGameInfoTicDup(byte raw, int maxTicDup) => Math.Clamp(raw, 1, maxTicDup);

    public static bool RosterPacketLooksValid(GuestContactHostContext ctx, int headerSize, int addrSize)
    {
        var cursor = headerSize;
        if (!RosterBufferHasBytes(ctx.NetBufferLength, cursor, 1))
            return false;
        var count = ctx.NetBuffer[cursor++];
        for (var entry = 0; entry < count; entry++)
        {
            if (!RosterBufferHasBytes(ctx.NetBufferLength, cursor, 1))
                return false;
            var client = ctx.NetBuffer[cursor++];
            if (!ValidateRosterClientId(client, ctx.MaxClients, ctx.MaxPlayers))
                return false;
            if (client > 0)
            {
                if (!RosterBufferHasBytes(ctx.NetBufferLength, cursor, addrSize))
                    return false;
                cursor += addrSize;
            }

            if (!RosterBufferHasBytes(ctx.NetBufferLength, cursor, 2))
                return false;
            var infoSize = ReadBigEndianUInt16(ctx.NetBuffer, cursor);
            cursor += 2;
            if (!RosterBufferHasBytes(ctx.NetBufferLength, cursor, infoSize))
                return false;
            cursor += infoSize;
        }

        return true;
    }

    public static void HandleGameInfoService(GuestContactHostContext ctx, (uint, ushort) from)
    {
        if (ctx.ConsolePlayer >= 0 && ctx.HasGameInfo?.Invoke(ctx.ConsolePlayer) != true)
        {
            var ticDup = ClampGameInfoTicDup(ctx.NetBuffer[ctx.HcdeServiceHeaderSize], ctx.MaxTicDup);
            ctx.SetTicDup?.Invoke((uint)ticDup);
            var gameId = new byte[8];
            Array.Copy(ctx.NetBuffer, ctx.HcdeServiceHeaderSize + 1, gameId, 0, 8);
            ctx.SetGameId?.Invoke(gameId);
            var payloadOffset = ctx.HcdeServiceHeaderSize + 9;
            var (_, streamSize) = GameInfoPayloadSizes(ctx.NetBufferLength, payloadOffset, ctx.NetBuffer.Length);
            ctx.ReadServerInfo?.Invoke(payloadOffset, streamSize);
            ctx.SetHasGameInfo?.Invoke(ctx.ConsolePlayer, true);
            ctx.NetLog?.Invoke("Received HCDE server info");
        }

        if (ctx.BeginReliableGameInfoAck?.Invoke() == true)
            ctx.CommitReliableGameInfoAck?.Invoke(from);
    }

    public static bool TryHandleRosterService(GuestContactHostContext ctx, (uint, ushort) from, int rosterAddrSize)
    {
        if (ctx.ConsolePlayer < 0)
            return false;
        if (ctx.CheckHcdePregameService?.Invoke(0, ctx.HcdeServiceHeaderSize + 1, "guest service roster") != true)
            return false;
        if (!RosterPacketLooksValid(ctx, ctx.HcdeServiceHeaderSize, rosterAddrSize))
            return false;
        if (ctx.ReadRosterEntries?.Invoke(ctx.HcdeServiceHeaderSize, from) != true)
            return false;
        ctx.SetHasRosterInfo?.Invoke(ctx.ConsolePlayer, true);
        if (ctx.BeginReliableRosterAck?.Invoke() == true)
            ctx.CommitReliableRosterAck?.Invoke(from);
        return true;
    }

    public static bool TryHandleBootstrapBeginService(GuestContactHostContext ctx)
    {
        if (ctx.ConsolePlayer < 0)
            return false;
        if (ctx.CheckHcdePregameService?.Invoke(0, ctx.HcdeServiceHeaderSize + 13, "guest service bootstrap begin") != true)
            return false;
        ctx.SetHasBootstrapInfo?.Invoke(ctx.ConsolePlayer, true);
        ctx.NetMessage?.Invoke("Receiving world bootstrap");
        ctx.DebugMark?.Invoke("guest bootstrap begin");
        ctx.AdoptServerRoomId?.Invoke(ctx.NetBuffer[ctx.HcdeServiceHeaderSize]);
        ctx.AckBootstrap?.Invoke();
        return true;
    }

    public static bool TryHandleStartGameService(GuestContactHostContext ctx, (uint, ushort) from)
    {
        if (ctx.ConsolePlayer < 0)
            return false;
        if (ctx.CheckHcdePregameService?.Invoke(0, ctx.HcdeServiceHeaderSize, "guest service start") != true)
            return false;
        if (ctx.BeginReliableStartGameAck?.Invoke() == true)
        {
            ctx.CommitReliableStartGameAck?.Invoke(from);
            ctx.FlushReliableServices?.Invoke(from);
        }

        ctx.NetMessage?.Invoke("Starting game");
        ctx.NetLog?.Invoke("Received HCDE service start");
        return true;
    }

    public static void SendGuestOutboundSetup(GuestContactHostContext ctx)
    {
        ctx.NetBuffer[0] = ctx.NcmdSetup;
        if (ctx.ConsolePlayer == -1)
        {
            if (ctx.IsHcdeConnect?.Invoke() == true && ctx.HostSessionToken != 0)
            {
                if (ctx.HasPendingReliableService?.Invoke() != true)
                {
                    ctx.BeginHeartbeatPacket?.Invoke();
                    ctx.SendPacket?.Invoke(ctx.HostAddress);
                }
            }
            else
            {
                ctx.BeginConnectPacket?.Invoke();
                ctx.SendPacket?.Invoke(ctx.HostAddress);
                if (ctx.DedicatedLateJoinRetryPendingSend)
                {
                    ctx.DedicatedLateJoinRetryPendingSend = false;
                    ctx.DebugMark?.Invoke("dedicated late-join retry packet sent");
                }
            }

            return;
        }

        var status = ctx.GetClientStatus?.Invoke(ctx.ConsolePlayer) ?? ctx.CstatNone;
        if (status == ctx.CstatConnecting)
        {
            if (ctx.BeginReliableClientUserInfo?.Invoke(ctx.ConsolePlayer) == true)
            {
                ctx.AppendUserInfo?.Invoke(ctx.ConsolePlayer);
                ctx.CommitReliableClientUserInfo?.Invoke(ctx.HostAddress, ctx.ConsolePlayer);
            }
        }
        else if (status == ctx.CstatWaiting)
        {
            if (ctx.HasPendingReliableService?.Invoke() != true)
            {
                ctx.BeginHeartbeatPacket?.Invoke();
                ctx.SendPacket?.Invoke(ctx.HostAddress);
            }
        }
    }

    public static bool GuestContactHostServicesNative(GuestContactHostContext ctx, (uint, ushort) from = default)
    {
        if (ctx.NetBuffer.Length > 1 && ctx.NetBuffer[1] == ctx.PreHcdeService)
        {
            if (ctx.IsHcdeConnect?.Invoke() != true)
            {
                ctx.DebugMark?.Invoke("ignored HCDE service packet before negotiation");
                return false;
            }
            var service = ctx.NetBuffer[2];
            switch (service)
            {
            case HpsGameInfo:
                HandleGameInfoService(ctx, from);
                break;
            case HpsRoster:
                TryHandleRosterService(ctx, from, GuestRosterAddrSize);
                break;
            case HpsBootstrapBegin:
                TryHandleBootstrapBeginService(ctx);
                break;
            case HpsStartGame:
                if (TryHandleStartGameService(ctx, from))
                    return true;
                break;
            default:
                ctx.IncrementUnsupported?.Invoke();
                ctx.DebugMark?.Invoke($"ignored unsupported guest HCDE service {service}");
                break;
            }
        }

        if (!ctx.EndOfGuestContactTick)
            return false;

        RunGuestSetupWatchdog(ctx);

        if (ctx.IsHcdeConnect?.Invoke() == true)
            ctx.FlushReliableServices?.Invoke(ctx.HostAddress);

        SendGuestOutboundSetup(ctx);
        return false;
    }
}
