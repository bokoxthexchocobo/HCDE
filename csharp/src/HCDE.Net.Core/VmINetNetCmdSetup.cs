namespace HCDE.Net.Core;

/// <summary>`I_NetCmd` and setup connect admission (<c>i_net.cpp</c> 2337–2523).</summary>
public static class VmINetNetCmdSetup
{
    public const int CmdSend = 1;
    public const int CmdGet = 2;
    public const byte NcmdSetup = 0x80;
    public const byte PreConnect = 1;

    public static void INetCmdNative(
        int cmd,
        int remoteClient,
        Action? sendToRemote,
        Action? getPacket)
    {
        if (cmd == CmdSend)
        {
            if (remoteClient >= 0)
                sendToRemote?.Invoke();
        }
        else if (cmd == CmdGet)
            getPacket?.Invoke();
    }

    public static void SetClientAckNative(
        int client,
        int from,
        bool add,
        int maxPlayers,
        Func<int, ulong>? getInfoAck,
        Action<int, ulong>? setInfoAck,
        Action<string>? logInvalid)
    {
        if (client >= maxPlayers || from >= maxPlayers || from >= 64)
        {
            logInvalid?.Invoke($"ignored invalid client ack update client={client} from={from} add={(add ? 1 : 0)}");
            return;
        }
        var bit = (ulong)1 << from;
        var current = getInfoAck?.Invoke(client) ?? 0;
        setInfoAck?.Invoke(client, add ? current | bit : current & ~bit);
    }

    public static bool ClientGotAckNative(int client, int from, int maxPlayers, Func<int, ulong>? getInfoAck)
    {
        if (client >= maxPlayers || from >= maxPlayers || from >= 64)
            return false;
        return ((getInfoAck?.Invoke(client) ?? 0) & ((ulong)1 << from)) != 0;
    }

    public static bool GetConnectionNative(Action? getPacket, Func<int>? getNetBufferLength)
    {
        getPacket?.Invoke();
        return (getNetBufferLength?.Invoke() ?? 0) > 0;
    }

    public static bool TryParseStrictIntNative(string? text, out int value)
    {
        value = 0;
        if (string.IsNullOrEmpty(text))
            return false;
        return int.TryParse(text, out value);
    }

    public static int CountConnectedPlayersNative(int maxClients, Func<int, byte>? getStatus, byte cstatNone)
    {
        var connected = 0;
        for (var i = 0; i < maxClients; i++)
        {
            if (getStatus?.Invoke(i) != cstatNone)
                connected++;
        }
        return connected;
    }

    public static bool TryProcessSetupConnectPacketNative(
        int netBufferLength,
        ReadOnlySpan<byte> netBuffer,
        bool hasPassword,
        string netPassword,
        bool rejectForInProgress,
        bool runtimeJoin,
        int maxClients,
        Func<bool, uint, ushort, bool>? isBanned,
        Func<byte[], int, (int Error, int PasswordOffset)>? verifyEngine,
        Func<int, int, bool>? findStringEnd,
        Func<int, (bool Present, byte Version, byte Flags)>? readConnectInfo,
        byte hcdeConnectProtocolVersion,
        int currentConnected,
        Func<int>? findFreeClientSlot,
        Action<uint, ushort, int, bool>? addClientConnection,
        Action<int, int>? updatePlayers,
        Action<uint, ushort, byte>? rejectConnection,
        Action<string>? logReject)
    {
        if (netBufferLength < 2 || netBuffer[0] != NcmdSetup || netBuffer[1] != PreConnect)
            return false;
        if (netBufferLength < 9)
        {
            rejectConnection?.Invoke(0, 0, 6);
            return true;
        }
        var verify = verifyEngine?.Invoke(netBuffer.Slice(2, netBufferLength - 2).ToArray(), netBufferLength - 2) ?? (0, 0);
        if (verify.Error != 0)
            return true;
        var passwordStart = 2 + verify.PasswordOffset;
        if (passwordStart >= netBufferLength || findStringEnd?.Invoke(passwordStart, netBufferLength) != true)
        {
            rejectConnection?.Invoke(0, 0, 6);
            return true;
        }
        var connectInfo = readConnectInfo?.Invoke(passwordStart) ?? (false, 0, 0);
        if (connectInfo.Present && connectInfo.Version != hcdeConnectProtocolVersion)
        {
            rejectConnection?.Invoke(0, 0, 6);
            return true;
        }
        if (!connectInfo.Present)
        {
            logReject?.Invoke("rejecting non-HCDE PRE_CONNECT");
            rejectConnection?.Invoke(0, 0, 6);
            return true;
        }
        if (hasPassword)
        {
            var pw = System.Text.Encoding.UTF8.GetString(netBuffer.Slice(passwordStart));
            if (pw != netPassword)
            {
                rejectConnection?.Invoke(0, 0, 3);
                return true;
            }
        }
        if (currentConnected >= maxClients)
        {
            rejectConnection?.Invoke(0, 0, 2);
            return true;
        }
        if (rejectForInProgress)
        {
            rejectConnection?.Invoke(0, 0, 4);
            return true;
        }
        var free = findFreeClientSlot?.Invoke() ?? -1;
        if (free < 0 || free >= maxClients)
        {
            rejectConnection?.Invoke(0, 0, 2);
            return true;
        }
        addClientConnection?.Invoke(0, 0, free, runtimeJoin);
        updatePlayers?.Invoke(currentConnected + 1, maxClients);
        return true;
    }
}
