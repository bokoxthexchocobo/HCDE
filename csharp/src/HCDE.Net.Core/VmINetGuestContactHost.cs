namespace HCDE.Net.Core;

/// <summary>Guest HCDE service tail and per-tick send (<c>i_net.cpp</c> 3951–4179).</summary>
public static class VmINetGuestContactHost
{
    private const byte HpsGameInfo = 4;
    private const byte HpsRoster = 6;
    private const byte HpsStartGame = 7;
    private const byte HpsBootstrapBegin = 13;

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
                if (ctx.ConsolePlayer >= 0 && ctx.HasGameInfo?.Invoke(ctx.ConsolePlayer) != true)
                {
                    var ticDup = Math.Clamp(ctx.NetBuffer[ctx.HcdeServiceHeaderSize], 1, ctx.MaxTicDup);
                    ctx.SetTicDup?.Invoke((uint)ticDup);
                    var gameId = new byte[8];
                    Array.Copy(ctx.NetBuffer, ctx.HcdeServiceHeaderSize + 1, gameId, 0, 8);
                    ctx.SetGameId?.Invoke(gameId);
                    var payloadOffset = ctx.HcdeServiceHeaderSize + 9;
                    var payloadSize = ctx.NetBufferLength > payloadOffset ? ctx.NetBufferLength - payloadOffset : 0;
                    ctx.ReadServerInfo?.Invoke(payloadOffset, payloadSize);
                    ctx.SetHasGameInfo?.Invoke(ctx.ConsolePlayer, true);
                    ctx.NetLog?.Invoke("Received HCDE server info");
                }
                if (ctx.BeginReliableGameInfoAck?.Invoke() == true)
                    ctx.CommitReliableGameInfoAck?.Invoke(from);
                break;
            case HpsRoster:
                if (ctx.ConsolePlayer < 0)
                    break;
                if (ctx.CheckHcdePregameService?.Invoke(0, ctx.HcdeServiceHeaderSize + 1, "guest service roster") != true)
                    break;
                if (ctx.ReadRosterEntries?.Invoke(ctx.HcdeServiceHeaderSize, from) != true)
                    break;
                ctx.SetHasRosterInfo?.Invoke(ctx.ConsolePlayer, true);
                if (ctx.BeginReliableRosterAck?.Invoke() == true)
                    ctx.CommitReliableRosterAck?.Invoke(from);
                break;
            case HpsBootstrapBegin:
                if (ctx.ConsolePlayer < 0)
                    break;
                if (ctx.CheckHcdePregameService?.Invoke(0, ctx.HcdeServiceHeaderSize + 13, "guest service bootstrap begin") != true)
                    break;
                ctx.SetHasBootstrapInfo?.Invoke(ctx.ConsolePlayer, true);
                ctx.NetMessage?.Invoke("Receiving world bootstrap");
                ctx.DebugMark?.Invoke("guest bootstrap begin");
                ctx.AdoptServerRoomId?.Invoke(ctx.NetBuffer[ctx.HcdeServiceHeaderSize]);
                ctx.AckBootstrap?.Invoke();
                break;
            case HpsStartGame:
                if (ctx.ConsolePlayer < 0)
                    break;
                if (ctx.CheckHcdePregameService?.Invoke(0, ctx.HcdeServiceHeaderSize, "guest service start") != true)
                    break;
                if (ctx.BeginReliableStartGameAck?.Invoke() == true)
                {
                    ctx.CommitReliableStartGameAck?.Invoke(from);
                    ctx.FlushReliableServices?.Invoke(from);
                }
                ctx.NetMessage?.Invoke("Starting game");
                ctx.NetLog?.Invoke("Received HCDE service start");
                return true;
            default:
                ctx.IncrementUnsupported?.Invoke();
                ctx.DebugMark?.Invoke($"ignored unsupported guest HCDE service {service}");
                break;
            }
        }

        if (!ctx.EndOfGuestContactTick)
            return false;

        if (ctx.IsHcdeConnect?.Invoke() == true)
        {
            var now = ctx.MsTime?.Invoke() ?? 0;
            var rxSeq = ctx.HostHcdeServiceRxSeq;
            if (ctx.GuestLastSetupProgressTime == 0 || rxSeq != ctx.GuestLastSetupRxSeq)
            {
                ctx.GuestLastSetupProgressTime = now;
                ctx.GuestLastSetupRxSeq = rxSeq;
            }
            else if (now - ctx.GuestLastSetupProgressTime >= ctx.HcdeGuestSetupProgressTimeoutMs)
            {
                ctx.DebugMark?.Invoke($"guest HCDE setup timed out waiting for progress last-rx={ctx.GuestLastSetupRxSeq}");
                ctx.FatalNetError?.Invoke("Timed out waiting for HCDE late-join setup. Try reconnecting.");
            }
        }

        if (ctx.IsHcdeConnect?.Invoke() == true)
            ctx.FlushReliableServices?.Invoke(ctx.HostAddress);

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
        }
        else
        {
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
        return false;
    }
}
