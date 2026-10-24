namespace HCDE.Net.Core;

/// <summary>Guest setup rejection through early HCDE services (<c>i_net.cpp</c> 3718–3950).</summary>
public static class VmINetGuestContactReject
{
    private const byte HpsHeartbeat = 1;
    private const byte HpsUserInfoAck = 3;
    private const byte HpsGameInfo = 4;
    private const byte HpsConsolePlayer = 8;
    private const byte HpsMapLoad = 9;

    public static bool GuestContactHostRejectionsNative(GuestContactHostContext ctx)
    {
        while (ctx.GetConnection?.Invoke() is { GotPacket: true } conn)
        {
            var msgSize = ctx.NetBufferLength;
            if (ctx.RemoteClient != 0)
                continue;
            if (ctx.NetBuffer[0] != ctx.NcmdSetup)
                continue;
            var pre = ctx.NetBuffer[1];
            if (pre == ctx.PreFull)
                ctx.FatalNetError?.Invoke("The game is full");
            else if (pre == ctx.PreInProgress)
            {
                if (ctx.DedicatedJoinMode && ctx.DedicatedLateJoinRetryPendingSend)
                {
                    ctx.DebugMark?.Invoke("ignoring PRE_IN_PROGRESS while dedicated late-join retry is pending send");
                    continue;
                }
                if (!ctx.DedicatedJoinMode && !ctx.DedicatedLateJoinRetryAttempted)
                {
                    ctx.DedicatedLateJoinRetryAttempted = true;
                    ctx.DedicatedLateJoinRetryPendingSend = true;
                    ctx.DedicatedJoinMode = true;
                    ctx.ClearHostSessionToken?.Invoke();
                    ctx.ClearHostHcdeConnect?.Invoke();
                    ctx.NetMessage?.Invoke("Server is mid-match. Retrying dedicated late join...");
                    ctx.NetLog?.Invoke("Retrying connect with dedicated late-join flags after PRE_IN_PROGRESS");
                    ctx.DebugMark?.Invoke("guest connect fallback: retrying as dedicated late-join");
                    continue;
                }
                if (ctx.DedicatedJoinMode)
                    ctx.FatalNetError?.Invoke("The dedicated server rejected late join. The server may be running an older build.");
                else
                    ctx.FatalNetError?.Invoke("The game has already started");
            }
            else if (pre == ctx.PreWrongPassword)
                ctx.FatalNetError?.Invoke("Invalid password");
            else if (pre == ctx.PreVerificationError)
            {
                if (msgSize < 3)
                {
                    ctx.FatalNetError?.Invoke("Malformed verification error response");
                    continue;
                }
                var payload = new byte[msgSize - 2];
                Array.Copy(ctx.NetBuffer, 2, payload, 0, payload.Length);
                ctx.FatalNetError?.Invoke(ctx.ReadVerificationError?.Invoke(payload) ?? "Unknown error");
            }
            else if (pre == ctx.PreKicked)
                ctx.FatalNetError?.Invoke("You have been kicked from the game");
            else if (pre == ctx.PreBanned)
                ctx.FatalNetError?.Invoke("You have been banned from the game");
            else if (pre == ctx.PreProtocolError)
                ctx.FatalNetError?.Invoke("The server rejected HCDE service protocol negotiation");
            else if (pre == ctx.PreSetupTimeout)
                ctx.FatalNetError?.Invoke("The server timed out during HCDE setup");
            else if (pre == ctx.PreConnectAck)
            {
                if (ctx.ConsolePlayer != -1)
                    continue;
                if (msgSize < 9)
                    continue;
                ctx.SetHostSessionToken?.Invoke(ctx.ReadSessionToken?.Invoke(5) ?? 0);
                ctx.DedicatedLateJoinRetryPendingSend = false;
                var ackFlags = msgSize >= 10 ? ctx.NetBuffer[9] : (byte)0;
                if ((ackFlags & ctx.PreConnectAckDedicated) != 0)
                    ctx.DedicatedJoinMode = true;
                var serviceConnect = (ackFlags & ctx.PreConnectAckHcdeService) != 0;
                if (serviceConnect)
                {
                    if (msgSize < 12)
                        ctx.FatalNetError?.Invoke("Malformed HCDE service connect acknowledgement");
                    if (ctx.NetBuffer[10] != ctx.HcdeConnectProtocolVersion)
                        ctx.FatalNetError?.Invoke("Unsupported HCDE service connect version from server");
                    ctx.SetHostHcdeConnect?.Invoke(ctx.NetBuffer[10], ctx.NetBuffer[11]);
                    ctx.Printf?.Invoke($"NetSession:: HCDE service connect negotiated v{ctx.NetBuffer[10]} flags=0x{ctx.NetBuffer[11]:x2}");
                }
                var announcedMaxClients = ctx.NetBuffer[4];
                if (announcedMaxClients < 1 || announcedMaxClients > ctx.MaxPlayers)
                {
                    ctx.DebugMark?.Invoke($"ignored connect ack: invalid max-clients={announcedMaxClients}");
                    continue;
                }
                ctx.MaxClients = announcedMaxClients;
                if (ctx.GetHostStatus?.Invoke() != ctx.CstatWaiting)
                {
                    ctx.SetHostStatus?.Invoke(ctx.CstatWaiting);
                    ctx.NetClientUpdated?.Invoke(0);
                }
                ctx.UpdatePlayers?.Invoke(ctx.NetBuffer[3], ctx.MaxClients);
                if (!serviceConnect)
                    ctx.FatalNetError?.Invoke("Server did not negotiate HCDE service. The host is not running HCDE or is an incompatible build.");
                ctx.NetMessage?.Invoke("Waiting for server assignment");
            }
            else if (pre == ctx.PreHcdeService)
            {
                if (ctx.IsHcdeConnect?.Invoke() != true)
                {
                    ctx.DebugMark?.Invoke("ignored HCDE service packet before negotiation");
                    continue;
                }
                if (msgSize < ctx.HcdeServiceHeaderSize)
                {
                    ctx.DebugMark?.Invoke($"guest HCDE service packet too short len={msgSize}");
                    continue;
                }
                var service = ctx.NetBuffer[2];
                switch (service)
                {
                case HpsConsolePlayer:
                    if (ctx.CheckHcdePregameService?.Invoke(0, ctx.HcdeServiceHeaderSize + 4, "guest service console player") != true)
                        break;
                    var assigned = ctx.NetBuffer[ctx.HcdeServiceHeaderSize];
                    var connectedPlayers = ctx.NetBuffer[ctx.HcdeServiceHeaderSize + 1];
                    var rosterMax = ctx.NetBuffer[ctx.HcdeServiceHeaderSize + 2];
                    var firstPlayable = ctx.GetFirstPlayableClientSlot?.Invoke() ?? 0;
                    if (assigned < firstPlayable || assigned >= rosterMax || rosterMax > ctx.MaxPlayers)
                    {
                        ctx.DebugMark?.Invoke($"ignored invalid HCDE console player assignment slot={assigned} max={rosterMax}");
                        break;
                    }
                    ctx.MaxClients = rosterMax;
                    if (ctx.ConsolePlayer == -1)
                    {
                        ctx.SetConsolePlayer?.Invoke(assigned);
                        ctx.AddNetworkClient?.Invoke(assigned);
                        ctx.SetClientStatus?.Invoke(assigned, ctx.CstatConnecting);
                        ctx.CopyHostSessionToClient?.Invoke(assigned);
                        ctx.SetClientHcdeConnect?.Invoke(assigned, ctx.NetBuffer[ctx.HcdeServiceHeaderSize + 3]);
                        ctx.SetupUserInfo?.Invoke();
                        ctx.NetMessage?.Invoke("Sending player information");
                        ctx.NetLog?.Invoke($"Received HCDE console player {assigned}");
                        ctx.NetClientConnected?.Invoke(assigned, 16);
                    }
                    ctx.UpdatePlayers?.Invoke(connectedPlayers, ctx.MaxClients);
                    break;
                case HpsHeartbeat:
                    if (ctx.ConsolePlayer < 0)
                        break;
                    if (ctx.CheckHcdePregameService?.Invoke(0, ctx.HcdeServiceHeaderSize + 2, "guest service heartbeat") != true)
                        break;
                    var hbMax = ctx.NetBuffer[ctx.HcdeServiceHeaderSize + 1];
                    if (hbMax < 1 || hbMax > ctx.MaxPlayers)
                    {
                        ctx.DebugMark?.Invoke($"ignored HCDE service heartbeat: invalid max-clients={hbMax}");
                        break;
                    }
                    ctx.MaxClients = hbMax;
                    ctx.UpdatePlayers?.Invoke(ctx.NetBuffer[ctx.HcdeServiceHeaderSize], ctx.MaxClients);
                    break;
                case HpsUserInfoAck:
                    if (ctx.ConsolePlayer < 0)
                        break;
                    if (ctx.CheckHcdePregameService?.Invoke(0, ctx.HcdeServiceHeaderSize + 1, "guest service userinfo ack") != true)
                        break;
                    if (ctx.NetBuffer[ctx.HcdeServiceHeaderSize] == ctx.ConsolePlayer)
                        ctx.SetClientAck?.Invoke(ctx.ConsolePlayer, ctx.ConsolePlayer, true);
                    if (ctx.GetClientStatus?.Invoke(ctx.ConsolePlayer) == ctx.CstatConnecting)
                    {
                        ctx.SetClientStatus?.Invoke(ctx.ConsolePlayer, ctx.CstatWaiting);
                        ctx.NetClientUpdated?.Invoke((uint)ctx.ConsolePlayer);
                        ctx.NetMessage?.Invoke("Waiting for server start");
                    }
                    if (ctx.BeginReliableUserInfoAck?.Invoke(ctx.ConsolePlayer) == true)
                    {
                        ctx.NetBuffer[ctx.HcdeServiceHeaderSize] = (byte)ctx.ConsolePlayer;
                        ctx.NetBufferLength = ctx.HcdeServiceHeaderSize + 1;
                        ctx.CommitReliableUserInfoAck?.Invoke(conn.From);
                    }
                    break;
                case HpsMapLoad:
                    if (ctx.ConsolePlayer < 0)
                        break;
                    if (ctx.CheckHcdePregameService?.Invoke(0, ctx.HcdeServiceHeaderSize, "guest service mapload") != true)
                        break;
                    if (ctx.HasMapLoadInfo?.Invoke(ctx.ConsolePlayer) != true)
                    {
                        ctx.ReadMapLoadInfo?.Invoke(ctx.HcdeServiceHeaderSize);
                        ctx.SetHasMapLoadInfo?.Invoke(ctx.ConsolePlayer, true);
                        ctx.NetLog?.Invoke("Received HCDE map load");
                    }
                    if (ctx.BeginReliableMapLoadAck?.Invoke() == true)
                        ctx.CommitReliableMapLoadAck?.Invoke(conn.From);
                    break;
                case HpsGameInfo:
                    if (ctx.ConsolePlayer < 0)
                        break;
                    if (ctx.CheckHcdePregameService?.Invoke(0, ctx.HcdeServiceHeaderSize + 9, "guest service gameinfo") != true)
                        break;
                    if (ctx.HasGameInfo?.Invoke(ctx.ConsolePlayer) != true)
                    {
                    }
                    break;
                default:
                    break;
                }
            }
        }
        return false;
    }
}
