namespace HCDE.Net.Core;

/// <summary>HCDE connect helpers and server-query snapshot build (<c>i_net.cpp</c> 711–898).</summary>
public static class VmINetConnectQuery
{
    public static ReadOnlySpan<byte> HcdeConnectMagic => "HCD3"u8;
    public const byte HcdeConnectProtocolVersion = 1;
    public const byte HcdeConnectDedicatedJoin = 1 << 0;
    public const byte HcdeConnectSuppressRoomUi = 1 << 1;
    public const byte HcdeConnectServerAuthority = 1 << 2;

    public const byte InvSpawnQfFallback = 1 << 0;
    public const byte InvSpawnQfSourceShift = 1;
    public const byte InvSpawnQfSourceMask = 0x0E;

    public const byte NetConnectFlowServerWaiting = 0;
    public const byte NetConnectFlowClientAuth = 1;
    public const byte NetConnectFlowSyncing = 2;

    public static bool ReadHcdeConnectInfoNative(
        ReadOnlySpan<byte> netBuffer,
        int netBufferLength,
        int offset,
        out HcdeConnectInfoPacket info)
    {
        info = default;
        if (offset + HcdeConnectMagic.Length + 2 > netBufferLength)
            return false;
        if (!netBuffer.Slice(offset, HcdeConnectMagic.Length).SequenceEqual(HcdeConnectMagic))
            return false;

        info.Present = true;
        info.Version = netBuffer[offset + HcdeConnectMagic.Length];
        info.Flags = netBuffer[offset + HcdeConnectMagic.Length + 1];
        return true;
    }

    public static void AppendHcdeConnectInfoNative(Span<byte> netBuffer, ref int netBufferLength, int maxMsgLen, byte flags)
    {
        if (netBufferLength + HcdeConnectMagic.Length + 2 > maxMsgLen)
            throw new InvalidOperationException("HCDE connect packet overflow");

        HcdeConnectMagic.CopyTo(netBuffer.Slice(netBufferLength));
        netBufferLength += HcdeConnectMagic.Length;
        netBuffer[netBufferLength++] = HcdeConnectProtocolVersion;
        netBuffer[netBufferLength++] = flags;
    }

    public static byte BuildLocalHcdeConnectFlagsNative(bool dedicatedJoinMode, bool silentNetStartMode)
    {
        byte flags = 0;
        if (dedicatedJoinMode)
        {
            flags |= HcdeConnectDedicatedJoin;
            flags |= HcdeConnectSuppressRoomUi;
        }
        if (silentNetStartMode)
            flags |= HcdeConnectSuppressRoomUi;
        if (dedicatedJoinMode)
            flags |= HcdeConnectServerAuthority;
        return flags;
    }

    public static string ConnectFlowNameNative(byte flow, bool dedicatedServerMode, bool dedicatedJoinMode) =>
        flow switch
        {
            NetConnectFlowServerWaiting => dedicatedServerMode ? "server-waiting" : "host-waiting",
            NetConnectFlowClientAuth => dedicatedJoinMode ? "client-auth" : "guest-contacting",
            NetConnectFlowSyncing => "syncing",
            _ => "idle",
        };

    public static string ServerGameModeNameNative(byte gameMode, bool isDeathmatch, bool isTeamplay)
    {
        return gameMode switch
        {
            1 => "Deathmatch",
            2 => "Team Deathmatch",
            3 => "CTF",
            4 => "Invasion",
            _ => isDeathmatch
                ? isTeamplay ? "Deathmatch + Teamplay" : "Deathmatch"
                : isTeamplay ? "Co-op + Teamplay" : "Co-op",
        };
    }

    public static string ServerInvasionStateNameNative(byte invasionState) =>
        invasionState switch
        {
            0 => "disabled",
            1 => "waiting",
            2 => "countdown",
            3 => "spawning",
            4 => "cleanup",
            5 => "intermission",
            6 => "victory",
            7 => "failure",
            _ => "unknown",
        };

    public static int CountConnectedClientsNative(
        int maxClients,
        int firstPlayableClientSlot,
        Func<int, byte>? getConnectedStatus,
        byte cstatNone)
    {
        var count = 0;
        for (var i = firstPlayableClientSlot; i < maxClients; i++)
        {
            if (getConnectedStatus?.Invoke(i) != cstatNone)
                count++;
        }
        return count;
    }

    public static ServerQuerySnapshotBuildResult BuildServerQuerySnapshotNative(ServerQuerySnapshotBuildContext ctx)
    {
        var snapshot = new ServerQuerySnapshotBuildResult();
        var hostname = ctx.SvHostname;
        snapshot.HostName = !string.IsNullOrEmpty(hostname) ? hostname : $"{ctx.GameNameDefault} server";

        if (!string.IsNullOrEmpty(ctx.PrimaryMapName))
            snapshot.MapName = ctx.PrimaryMapName;
        else if (!string.IsNullOrEmpty(ctx.LevelMapName))
            snapshot.MapName = ctx.LevelMapName;
        else if (!string.IsNullOrEmpty(ctx.PrimaryLevelName))
            snapshot.MapName = ctx.PrimaryLevelName;
        else if (!string.IsNullOrEmpty(ctx.LevelLevelName))
            snapshot.MapName = ctx.LevelLevelName;
        else
            snapshot.MapName = "unknown";

        snapshot.GameName = !string.IsNullOrEmpty(ctx.GameStartupName) ? ctx.GameStartupName : ctx.GameNameDefault;
        snapshot.SessionState = ConnectFlowNameNative(ctx.NetConnectFlow, ctx.DedicatedServerMode, ctx.DedicatedJoinMode);
        snapshot.Version = ctx.VersionString;
        snapshot.GitHash = ctx.GitHash;

        var connectedClients = CountConnectedClientsNative(
            ctx.MaxClients, ctx.FirstPlayableClientSlot, ctx.GetConnectedStatus, ctx.CstatNone);
        var visibleMaxClients = ctx.GetVisibleMaxClients?.Invoke() ?? ctx.MaxClients;
        var advertisedMaxClients = ctx.SvMaxPlayers > 0
            ? Math.Clamp(ctx.SvMaxPlayers, connectedClients, visibleMaxClients)
            : visibleMaxClients;
        snapshot.MaxPlayers = (byte)Math.Clamp(advertisedMaxClients, 0, byte.MaxValue);
        snapshot.PlayerCount = 0;
        snapshot.Skill = (byte)Math.Clamp(ctx.GameSkill, 0, byte.MaxValue);
        snapshot.Deathmatch = ctx.Deathmatch;
        snapshot.Teamplay = ctx.Teamplay;
        snapshot.GameMode = (byte)Math.Clamp(ctx.SvGametype, 0, byte.MaxValue);
        snapshot.GameModeName = ServerGameModeNameNative(snapshot.GameMode, snapshot.Deathmatch, snapshot.Teamplay);
        snapshot.InvasionState = (byte)Math.Clamp(ctx.GetInvasionState(), 0, byte.MaxValue);
        snapshot.InvasionStateTics = (ushort)Math.Clamp(ctx.GetInvasionStateTics(), 0, ushort.MaxValue);
        var invasionStateName = ctx.GetInvasionStateName();
        if (!string.IsNullOrEmpty(invasionStateName))
            snapshot.InvasionStateName = invasionStateName;
        snapshot.InvasionWave = (ushort)Math.Clamp(ctx.GetInvasionWave(), 0, ushort.MaxValue);
        snapshot.InvasionMaxWaves = (ushort)Math.Clamp(ctx.GetInvasionMaxWaves(), 0, ushort.MaxValue);
        snapshot.InvasionWaveBudget = (ushort)Math.Clamp(ctx.GetInvasionWaveBudget(), 0, ushort.MaxValue);
        snapshot.InvasionWaveSpawned = (ushort)Math.Clamp(ctx.GetInvasionWaveSpawned(), 0, ushort.MaxValue);
        snapshot.InvasionWaveCleared = (ushort)Math.Clamp(ctx.GetInvasionWaveCleared(), 0, ushort.MaxValue);
        snapshot.InvasionWaveFlags = ctx.IsInvasionBossWave() ? (byte)1 : (byte)0;
        snapshot.InvasionSpawnSpotCount = (ushort)Math.Clamp(ctx.GetInvasionSpawnSpotCount(), 0, ushort.MaxValue);
        snapshot.InvasionSpawnActiveSpotCount = (ushort)Math.Clamp(ctx.GetInvasionActiveSpawnSpotCount(), 0, ushort.MaxValue);
        snapshot.InvasionSpawnPlanBudget = (ushort)Math.Clamp(ctx.GetInvasionSpawnPlanBudget(), 0, ushort.MaxValue);
        snapshot.InvasionSpawnActiveTag = (ushort)Math.Clamp(ctx.GetInvasionSpawnActiveTag(), 0, ushort.MaxValue);
        snapshot.InvasionActiveMonsters = (ushort)Math.Clamp(ctx.GetInvasionActiveMonsterCount(), 0, ushort.MaxValue);

        var fallbackSource = (byte)Math.Clamp(ctx.GetInvasionSpawnFallbackSource(), 0, 7);
        byte spawnFlags = 0;
        if (ctx.IsInvasionSpawnUsingFallback())
            spawnFlags |= InvSpawnQfFallback;
        spawnFlags |= (byte)((fallbackSource << InvSpawnQfSourceShift) & InvSpawnQfSourceMask);
        snapshot.InvasionSpawnFlags = spawnFlags;

        if (snapshot.GameMode == 4 && !string.IsNullOrEmpty(snapshot.InvasionStateName))
            snapshot.SessionState = $"{snapshot.SessionState} | invasion-{snapshot.InvasionStateName}";

        snapshot.FragLimit = ctx.FragLimit > 0 ? (ushort)Math.Clamp(ctx.FragLimit, 0, ushort.MaxValue) : (ushort)0;
        if (ctx.Timelimit > 0f)
        {
            var timeleft = (int)(ctx.Timelimit - ctx.LevelTime / (ctx.TicRate * 60));
            snapshot.TimeLeft = (ushort)Math.Max(timeleft, 0);
        }

        snapshot.Players.Capacity = snapshot.PlayerCount;
        for (var i = ctx.FirstPlayableClientSlot; i < ctx.MaxClients; i++)
        {
            var connectedSetupSlot = ctx.GetConnectedStatus?.Invoke(i) != ctx.CstatNone;
            if (!ctx.IsPlayerInGame(i) && !connectedSetupSlot)
                continue;

            var player = new ServerQueryPlayerRow
            {
                Name = ctx.GetPlayerName(i),
                Ping = (ushort)Math.Clamp(ctx.GetAverageLatency(i), 0u, ushort.MaxValue),
                Frags = (short)Math.Clamp(ctx.GetFragCount(i), short.MinValue, short.MaxValue),
                Kills = (short)Math.Clamp(ctx.GetKillCount(i), short.MinValue, short.MaxValue),
                Deaths = 0,
            };
            snapshot.Players.Add(player);
        }
        snapshot.PlayerCount = (byte)Math.Clamp(snapshot.Players.Count, 0, byte.MaxValue);
        return snapshot;
    }
}
