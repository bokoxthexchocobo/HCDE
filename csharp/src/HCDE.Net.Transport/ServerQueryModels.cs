namespace HCDE.Net.Transport;

public sealed class ServerQueryPlayer
{
    public string Name { get; set; } = string.Empty;
    public ushort Ping { get; set; }
    public short Frags { get; set; }
    public short Kills { get; set; }
    public short Deaths { get; set; }
}

public sealed class ServerQuerySnapshot
{
    public string HostName { get; set; } = string.Empty;
    public string MapName { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public string SessionState { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string GitHash { get; set; } = string.Empty;
    public string GameModeName { get; set; } = string.Empty;
    public string InvasionStateName { get; set; } = string.Empty;
    public byte PlayerCount { get; set; }
    public byte MaxPlayers { get; set; }
    public byte GameMode { get; set; }
    public byte Skill { get; set; }
    public byte InvasionState { get; set; }
    public byte InvasionWaveFlags { get; set; }
    public byte InvasionSpawnFlags { get; set; }
    public bool Deathmatch { get; set; }
    public bool Teamplay { get; set; }
    public ushort TimeLeft { get; set; }
    public ushort FragLimit { get; set; }
    public ushort InvasionStateTics { get; set; }
    public ushort InvasionWave { get; set; }
    public ushort InvasionMaxWaves { get; set; }
    public ushort InvasionWaveBudget { get; set; }
    public ushort InvasionWaveSpawned { get; set; }
    public ushort InvasionWaveCleared { get; set; }
    public ushort InvasionSpawnSpotCount { get; set; }
    public ushort InvasionSpawnActiveSpotCount { get; set; }
    public ushort InvasionSpawnPlanBudget { get; set; }
    public ushort InvasionSpawnActiveTag { get; set; }
    public ushort InvasionActiveMonsters { get; set; }
    public List<ServerQueryPlayer> Players { get; } = [];
}
