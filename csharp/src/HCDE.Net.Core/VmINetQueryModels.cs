namespace HCDE.Net.Core;

public struct HcdeConnectInfoPacket
{
    public bool Present;
    public byte Version;
    public byte Flags;
}

public sealed class ServerQuerySnapshotBuildContext
{
    public string? SvHostname { get; init; }
    public string GameNameDefault { get; init; } = "HCDE";
    public string? PrimaryMapName { get; init; }
    public string? LevelMapName { get; init; }
    public string? PrimaryLevelName { get; init; }
    public string? LevelLevelName { get; init; }
    public string? GameStartupName { get; init; }
    public byte NetConnectFlow { get; init; }
    public bool DedicatedServerMode { get; init; }
    public bool DedicatedJoinMode { get; init; }
    public string VersionString { get; init; } = "";
    public string GitHash { get; init; } = "";
    public int MaxClients { get; init; }
    public int FirstPlayableClientSlot { get; init; }
    public Func<int, byte>? GetConnectedStatus { get; init; }
    public byte CstatNone { get; init; }
    public Func<int>? GetVisibleMaxClients { get; init; } = () => 8;
    public int SvMaxPlayers { get; init; }
    public int GameSkill { get; init; }
    public bool Deathmatch { get; init; }
    public bool Teamplay { get; init; }
    public int SvGametype { get; init; }
    public Func<int> GetInvasionState { get; init; } = () => 0;
    public Func<int> GetInvasionStateTics { get; init; } = () => 0;
    public Func<string?> GetInvasionStateName { get; init; } = () => null;
    public Func<int> GetInvasionWave { get; init; } = () => 0;
    public Func<int> GetInvasionMaxWaves { get; init; } = () => 0;
    public Func<int> GetInvasionWaveBudget { get; init; } = () => 0;
    public Func<int> GetInvasionWaveSpawned { get; init; } = () => 0;
    public Func<int> GetInvasionWaveCleared { get; init; } = () => 0;
    public Func<bool> IsInvasionBossWave { get; init; } = () => false;
    public Func<int> GetInvasionSpawnSpotCount { get; init; } = () => 0;
    public Func<int> GetInvasionActiveSpawnSpotCount { get; init; } = () => 0;
    public Func<int> GetInvasionSpawnPlanBudget { get; init; } = () => 0;
    public Func<int> GetInvasionSpawnActiveTag { get; init; } = () => 0;
    public Func<int> GetInvasionActiveMonsterCount { get; init; } = () => 0;
    public Func<int> GetInvasionSpawnFallbackSource { get; init; } = () => 0;
    public Func<bool> IsInvasionSpawnUsingFallback { get; init; } = () => false;
    public int FragLimit { get; init; }
    public float Timelimit { get; init; }
    public double LevelTime { get; init; }
    public int TicRate { get; init; } = 35;
    public Func<int, bool> IsPlayerInGame { get; init; } = _ => false;
    public Func<int, string> GetPlayerName { get; init; } = _ => "";
    public Func<int, uint> GetAverageLatency { get; init; } = _ => 0;
    public Func<int, int> GetFragCount { get; init; } = _ => 0;
    public Func<int, int> GetKillCount { get; init; } = _ => 0;
}

public sealed class ServerQuerySnapshotBuildResult
{
    public string HostName { get; set; } = "";
    public string MapName { get; set; } = "";
    public string GameName { get; set; } = "";
    public string SessionState { get; set; } = "";
    public string Version { get; set; } = "";
    public string GitHash { get; set; } = "";
    public byte MaxPlayers { get; set; }
    public byte PlayerCount { get; set; }
    public byte Skill { get; set; }
    public bool Deathmatch { get; set; }
    public bool Teamplay { get; set; }
    public byte GameMode { get; set; }
    public string GameModeName { get; set; } = "";
    public byte InvasionState { get; set; }
    public ushort InvasionStateTics { get; set; }
    public string InvasionStateName { get; set; } = "";
    public ushort InvasionWave { get; set; }
    public ushort InvasionMaxWaves { get; set; }
    public ushort InvasionWaveBudget { get; set; }
    public ushort InvasionWaveSpawned { get; set; }
    public ushort InvasionWaveCleared { get; set; }
    public byte InvasionWaveFlags { get; set; }
    public ushort InvasionSpawnSpotCount { get; set; }
    public ushort InvasionSpawnActiveSpotCount { get; set; }
    public ushort InvasionSpawnPlanBudget { get; set; }
    public ushort InvasionSpawnActiveTag { get; set; }
    public byte InvasionSpawnFlags { get; set; }
    public ushort InvasionActiveMonsters { get; set; }
    public ushort FragLimit { get; set; }
    public ushort TimeLeft { get; set; }
    public List<ServerQueryPlayerRow> Players { get; } = [];
}

public sealed class ServerQueryPlayerRow
{
    public string Name { get; set; } = "";
    public ushort Ping { get; set; }
    public short Frags { get; set; }
    public short Kills { get; set; }
    public short Deaths { get; set; }
}

public sealed class ServerQuerySnapshotDecodeTarget
{
    public string HostName { get; set; } = "";
    public byte PlayerCount { get; set; }
    public byte MaxPlayers { get; set; }
    public string MapName { get; set; } = "";
    public string SessionState { get; set; } = "";
    public bool Deathmatch { get; set; }
    public byte Skill { get; set; }
    public bool Teamplay { get; set; }
    public ushort TimeLeft { get; set; }
    public ushort FragLimit { get; set; }
    public string Version { get; set; } = "";
    public string GitHash { get; set; } = "";
    public byte GameMode { get; set; }
    public string GameModeName { get; set; } = "";
    public string GameName { get; set; } = "";
    public byte InvasionState { get; set; }
    public ushort InvasionStateTics { get; set; }
    public string InvasionStateName { get; set; } = "";
    public ushort InvasionWave { get; set; }
    public ushort InvasionMaxWaves { get; set; }
    public ushort InvasionWaveBudget { get; set; }
    public ushort InvasionWaveSpawned { get; set; }
    public ushort InvasionWaveCleared { get; set; }
    public byte InvasionWaveFlags { get; set; }
    public ushort InvasionSpawnSpotCount { get; set; }
    public ushort InvasionSpawnActiveSpotCount { get; set; }
    public ushort InvasionSpawnPlanBudget { get; set; }
    public ushort InvasionSpawnActiveTag { get; set; }
    public byte InvasionSpawnFlags { get; set; }
    public ushort InvasionActiveMonsters { get; set; }
    public List<ServerQueryPlayerRow> Players { get; } = [];
}

public sealed class ServerQuerySnapshotCacheState
{
    public ServerQuerySnapshotBuildResult CachedSnapshot { get; set; } = new();
    public ulong LastSnapshotTime { get; set; }
}
