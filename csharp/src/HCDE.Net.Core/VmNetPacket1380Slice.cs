namespace HCDE.Net.Core;

/// <summary>
/// Level-start wait, net-buffer sizing, and the send/get packet path from
/// <c>d_net.cpp</c> 4992-5195. The accepts and rejects after the size printf
/// stay on the following lines. Skip, trace, and net-command calls are callees.
/// </summary>
public sealed class VmNetPacket1380Slice
{
    public const int NcmdExit = 0x80;
    public const int NcmdSetup = 0x20;
    public const int NcmdLevelReady = 0x10;
    public const int NcmdQuitters = 0x08;
    public const int NcmdLatencyAck = 0x02;
    public const int NcmdLatency = 0x01;
    public const int LstWaiting = 2;
    public const int CmdSend = 1;
    public const int CmdGet = 2;

    public bool Netgame { get; set; }
    public bool DemoPlayback { get; set; }
    public byte[] NetBuffer { get; set; } = [];
    public int NetBufferLength { get; set; }
    public int RemoteClient { get; set; }
    public int ConsolePlayer { get; set; }
    public byte[] LocalNetBuffer { get; set; } = [];
    public int LocalNetBufferSize { get; set; }
    public uint LevelStartAck { get; set; } = 9;
    public int LevelStartDelay { get; set; } = 9;
    public int LevelStartDebug { get; set; } = 9;
    public int FullLatencyCycle { get; set; } = 9;
    public uint RoomId { get; set; }
    public string? MapName { get; set; }
    public int TransportClients { get; set; }
    public Func<bool>? HasPlayable { get; set; }
    public Action<string>? ResetWatchdog { get; set; }
    public Action<int, string>? SetStatus { get; set; }
    public Func<bool>? IsLocalAuthority { get; set; }
    public Func<int>? PlayableCount { get; set; }
    public Func<int, bool>? IsAuthoritySlot { get; set; }
    public Func<bool>? LivePacket { get; set; }
    public TrySkipUserCmd? SkipUserCmd { get; set; }
    public Action<int, byte, int>? TraceOutgoing { get; set; }
    public Action<int>? NetCmd { get; set; }
    public List<string> Lines { get; } = [];
    public List<string> Watchdogs { get; } = [];
    public List<(int Status, string Reason)> Statuses { get; } = [];
    public List<int> Commands { get; } = [];

    public delegate bool TrySkipUserCmd(
        byte[] data, int offset, int size, bool lastTic, byte commandTics,
        ulong seen, byte remaining, out int payload);

    public void SetWaiting()
    {
        if (!Netgame || DemoPlayback || !CallHasPlayable())
            return;
        LevelStartAck = 0;
        LevelStartDelay = 0;
        LevelStartDebug = 0;
        FullLatencyCycle = 0;
        if (ResetWatchdog == null)
            throw new InvalidOperationException("ResetWatchdog");
        ResetWatchdog("level-wait");
        Watchdogs.Add("level-wait");
        if (SetStatus == null)
            throw new InvalidOperationException("SetStatus");
        SetStatus(LstWaiting, "map-load-wait");
        Statuses.Add((LstWaiting, "map-load-wait"));
        var map = MapName == null ? "<none>" : MapName;
        var authority = CallLocalAuthority() ? 1 : 0;
        Lines.Add(
            "net.levelstart level start waiting room=" + RoomId
            + " map=" + map
            + " authority=" + authority
            + " transport-clients=" + TransportClients
            + " playable-clients=" + CallPlayableCount());
    }

    public int GetNetBufferSize()
    {
        if (NetBufferLength == 0)
            return 0;
        var header = NetBuffer[0];
        if ((header & NcmdExit) != 0)
            return 1 + (CallAuthority(RemoteClient) ? 1 : 0);
        if ((header & NcmdSetup) != 0)
            return NetBufferLength < 2 ? 2 : NetBufferLength;
        if ((header & (NcmdLatency | NcmdLatencyAck)) != 0)
            return 2;
        if (CallLive())
            return NetBufferLength;
        if ((header & NcmdLevelReady) != 0)
        {
            var bytes = 2;
            if (CallAuthority(RemoteClient))
                bytes += 6;
            return bytes;
        }

        var total = 10;
        if (NetBufferLength < total)
            return total;
        if ((header & NcmdQuitters) != 0)
        {
            total += NetBuffer[total] + 1;
            if (NetBufferLength < total)
                return total;
        }

        if (NetBufferLength < total + 1)
            return total + 1;
        var playerCount = NetBuffer[total];
        total++;
        if (NetBufferLength < total + 1)
            return total + 1;
        var numTics = NetBuffer[total];
        total++;
        if (numTics > 0)
        {
            total += 4;
            if (NetBufferLength < total)
                return total;
        }

        if (NetBufferLength < total + 1)
            return total + 1;
        var ranTics = NetBuffer[total];
        total++;
        if (ranTics > 0)
        {
            total += 4;
            if (NetBufferLength < total)
                return total;
        }

        total++;
        if (NetBufferLength < total)
            return total;
        var padding = 1;
        if (CallAuthority(RemoteClient))
            padding += 2;
        if (NetBufferLength < total + playerCount * padding)
            return total + playerCount * padding;

        var cursor = total;
        var remain = NetBufferLength - total;
        for (var player = 0; player < playerCount; player++)
        {
            if (remain < 1)
                return NetBufferLength + 1;
            cursor++;
            remain--;
            if (CallAuthority(RemoteClient))
            {
                if (remain < 2)
                    return NetBufferLength + 1;
                cursor += 2;
                remain -= 2;
            }

            for (var i = 0; i < ranTics; i++)
            {
                if (remain < 3)
                    return NetBufferLength + 1;
                cursor += 3;
                remain -= 3;
            }

            ulong seen = 0;
            for (var i = 0; i < numTics; i++)
            {
                if (remain < 1)
                    return NetBufferLength + 1;
                var commandOffset = NetBuffer[cursor];
                if (commandOffset >= numTics || commandOffset >= 64)
                    return NetBufferLength + 1;
                var mask = 1ul << commandOffset;
                if ((seen & mask) != 0)
                    return NetBufferLength + 1;
                seen |= mask;
                cursor++;
                remain--;
                if (SkipUserCmd == null)
                    throw new InvalidOperationException("SkipUserCmd");
                var ok = SkipUserCmd(
                    NetBuffer, cursor, remain, i + 1 == numTics, numTics, seen,
                    (byte)(numTics - i - 1), out var payload);
                if (!ok || payload > remain)
                    return NetBufferLength + 1;
                cursor += payload;
                remain -= payload;
            }
        }

        return cursor;
    }

    public void SendPacket(int client, int size)
    {
        if (DemoPlayback)
            return;
        if (Netgame && size > 0)
        {
            if (TraceOutgoing == null)
                throw new InvalidOperationException("TraceOutgoing");
            TraceOutgoing(client, NetBuffer[0], size);
        }

        RemoteClient = client;
        NetBufferLength = size;
        if (client == ConsolePlayer)
        {
            if (size > 0)
                Array.Copy(NetBuffer, LocalNetBuffer, size);
            LocalNetBufferSize = size;
            return;
        }

        if (!Netgame)
            throw new InvalidOperationException("Tried to send a packet to a client while offline");
        CallNet(CmdSend);
    }

    public bool GetPacket()
    {
        if (DemoPlayback)
            return false;
        if (LocalNetBufferSize != 0)
        {
            Array.Copy(LocalNetBuffer, NetBuffer, LocalNetBufferSize);
            NetBufferLength = LocalNetBufferSize;
            RemoteClient = ConsolePlayer;
            LocalNetBufferSize = 0;
            return true;
        }

        if (!Netgame)
            return false;
        CallNet(CmdGet);
        if (RemoteClient == -1)
            return false;
        var sizeCheck = GetNetBufferSize();
        if (NetBufferLength != sizeCheck)
        {
            Lines.Add("Incorrect packet size " + NetBufferLength + " (expected " + sizeCheck + ")\n");
            return false;
        }

        return true;
    }

    bool CallHasPlayable()
    {
        if (HasPlayable == null)
            throw new InvalidOperationException("HasPlayable");
        return HasPlayable();
    }

    bool CallLocalAuthority()
    {
        if (IsLocalAuthority == null)
            throw new InvalidOperationException("IsLocalAuthority");
        return IsLocalAuthority();
    }

    int CallPlayableCount()
    {
        if (PlayableCount == null)
            throw new InvalidOperationException("PlayableCount");
        return PlayableCount();
    }

    bool CallAuthority(int client)
    {
        if (IsAuthoritySlot == null)
            throw new InvalidOperationException("IsAuthoritySlot");
        return IsAuthoritySlot(client);
    }

    bool CallLive()
    {
        if (LivePacket == null)
            throw new InvalidOperationException("LivePacket");
        return LivePacket();
    }

    void CallNet(int command)
    {
        if (NetCmd == null)
            throw new InvalidOperationException("NetCmd");
        Commands.Add(command);
        NetCmd(command);
    }
}
