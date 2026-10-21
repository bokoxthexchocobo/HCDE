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

public sealed class GetPacketNativeContext
{
    public byte[] TransmitBuffer { get; init; } = [];
    public byte[] NetBuffer { get; init; } = [];
    public byte[] GameId { get; init; } = [];
    public int MaxTransmitSize { get; init; }
    public int MaxMsgLen { get; init; }
    public int ConsolePlayer { get; init; }
    public bool GameStarted { get; init; }
    public bool DedicatedServerMode { get; init; }
    public bool LateJoinEnabled { get; init; }
    public byte PreInProgress { get; init; }
    public int WsaConnReset { get; init; }
    public int WsaWouldBlock { get; init; }
    public bool CaptureFromAddress { get; init; }
    public int RemoteClient { get; set; }
    public int NetBufferLength { get; set; }
    public (uint Addr, ushort Port) FromAddress { get; set; }
    public Func<byte[], int, (int Size, (uint Addr, ushort Port) From)>? RecvFrom { get; init; }
    public Func<(uint, ushort), int>? FindClient { get; init; }
    public Func<int>? GetLastSocketError { get; init; }
    public Action<string>? NetLog { get; init; }
    public Action<int>? DebugWarning { get; init; }
    public Func<int, bool>? IsResetFromReservedServerSlot { get; init; }
    public Func<int, bool>? IsHcdeServiceAuthoritySlot { get; init; }
    public Action<string>? FatalNetError { get; init; }
    public Action<string>? FatalError { get; init; }
    public Action? IncrementPacketReceived { get; init; }
    public Func<(uint, ushort), byte[], int, bool>? TryHandleServerQuery { get; init; }
    public Action? IncrementPacketTooShort { get; init; }
    public Func<byte[], int, uint>? CalcCrc32 { get; init; }
    public Func<uint, byte[], uint>? AddCrc32 { get; init; }
    public Action? IncrementPacketBadCrc { get; init; }
    public Action? IncrementCompressedMalformed { get; init; }
    public Func<byte[], int, byte[], int, int>? Uncompress { get; init; }
    public Action? IncrementDecompressFailure { get; init; }
    public Action? IncrementPacketOversized { get; init; }
    public Action<(uint, ushort), byte, int>? MarkInboundSetup { get; init; }
    public Func<bool>? IsLocalHcdeServiceAuthority { get; init; }
    public Func<(uint, ushort), bool>? TryProcessSetupConnect { get; init; }
    public Action<(uint, ushort)>? SendPacket { get; init; }
}
public sealed class HandleIncomingConnectionContext
{
    public int RemoteClient { get; init; }
    public int MaxClients { get; init; }
    public int NetBufferLength { get; init; }
    public byte[] NetBuffer { get; init; } = [];
    public byte PreHcdeService { get; init; }
    public int HcdeServiceHeaderSize { get; init; }
    public byte CstatNone { get; init; }
    public byte CstatConnecting { get; init; }
    public byte CstatWaiting { get; init; }
    public byte CstatReady { get; init; }
    public Func<bool>? IsLocalHcdeServiceAuthority { get; init; }
    public Action? HandleGuestRuntimeHcdeService { get; init; }
    public Func<int, bool>? IsHcdeConnect { get; init; }
    public Action<string>? DebugMark { get; init; }
    public Func<int, byte>? GetStatus { get; init; }
    public Func<int, int, string, bool>? CheckHcdePregameService { get; init; }
    public Action<int, int>? ReadUserInfo { get; init; }
    public Action<int, byte>? SetStatus { get; init; }
    public Action<int, uint>? NetClientConnected { get; init; }
    public Action<int, int, bool>? SetClientAck { get; init; }
    public Action<int, bool>? SetHasGameInfo { get; init; }
    public Action<int, bool>? SetHasMapLoadInfo { get; init; }
    public Action<int, bool>? SetHasRosterInfo { get; init; }
    public Func<int, bool>? HasBootstrapInfo { get; init; }
    public Action<int, string>? BeginRuntimeBootstrap { get; init; }
    public Action<int, bool>? SetHasBootstrapInfo { get; init; }
    public Action<int, string>? RequestRuntimeResync { get; init; }
    public Action<int>? QueueBootstrapResync { get; init; }
    public Action<int, bool>? SetHasStartGameAck { get; init; }
    public Action? IncrementUnsupported { get; init; }
    public Func<int>? CountConnectedPlayers { get; init; }
    public Func<int, bool>? ClientGotAck { get; init; }
    public Func<int, bool>? BeginConnectAckResend { get; init; }
    public Action<int, int, int, byte>? SendConnectAck { get; init; }
    public Func<int, bool>? BeginConsolePlayerService { get; init; }
    public Action<int, int, int, byte>? WriteConsolePlayerPayload { get; init; }
    public Func<int, bool>? CommitConsolePlayerService { get; init; }
    public Action<int>? FlushReliableServices { get; init; }
    public Func<int, bool>? BeginUserInfoAckService { get; init; }
    public Func<int, bool>? CommitUserInfoAckService { get; init; }
    public Func<int, bool>? HasMapLoadInfo { get; init; }
    public Func<int, bool>? BeginMapLoadService { get; init; }
    public Action<int>? AppendMapLoadInfo { get; init; }
    public Func<int, bool>? CommitMapLoadService { get; init; }
    public Func<int, bool>? HasGameInfo { get; init; }
    public Func<int, bool>? BeginGameInfoService { get; init; }
    public Action<int>? AppendGameInfo { get; init; }
    public Func<int, bool>? CommitGameInfoService { get; init; }
    public Func<int, bool>? HasRosterInfo { get; init; }
    public Func<int, bool>? QueueRosterService { get; init; }
    public Func<int, bool>? IsRuntimeJoin { get; init; }
    public Func<int, bool>? QueueBootstrapService { get; init; }
    public Action<int>? MarkClientReady { get; init; }
    public Action<int>? NetClientUpdated { get; init; }
    public Func<int, bool>? BeginStartGameService { get; init; }
    public Func<int, bool>? CommitStartGameService { get; init; }
}

public sealed class HandleIncomingConnectionMaintenanceContext
{
    public int MaxClients { get; init; }
    public byte CstatNone { get; init; }
    public byte CstatReady { get; init; }
    public byte CstatConnecting { get; init; }
    public byte CstatWaiting { get; init; }
    public Func<bool>? IsLocalHcdeServiceAuthority { get; init; }
    public Func<int>? CountConnectedPlayers { get; init; }
    public Func<int, byte>? GetStatus { get; init; }
    public Func<int, bool>? IsHcdeConnect { get; init; }
    public Func<int, bool>? HasRosterInfo { get; init; }
    public Func<int, bool>? HasStartGameAck { get; init; }
    public Func<int, bool>? HasPendingReliableService { get; init; }
    public Func<int, int, string, (bool Dropped, int ConnectedPlayers)>? DropClientForHcdeTimeout { get; init; }
    public Func<int, bool>? ClientGotAck { get; init; }
    public Func<int, bool>? BeginConnectAckResend { get; init; }
    public Action<int, int, int, byte>? SendConnectAck { get; init; }
    public Func<int, bool>? BeginConsolePlayerService { get; init; }
    public Action<int, int, int, byte>? WriteConsolePlayerPayload { get; init; }
    public Func<int, bool>? CommitConsolePlayerService { get; init; }
    public Action<int>? FlushReliableServices { get; init; }
    public Func<int, bool>? BeginUserInfoAckService { get; init; }
    public Func<int, bool>? CommitUserInfoAckService { get; init; }
    public Func<int, bool>? HasMapLoadInfo { get; init; }
    public Func<int, bool>? BeginMapLoadService { get; init; }
    public Action<int>? AppendMapLoadInfo { get; init; }
    public Func<int, bool>? CommitMapLoadService { get; init; }
    public Func<int, bool>? HasGameInfo { get; init; }
    public Func<int, bool>? BeginGameInfoService { get; init; }
    public Action<int>? AppendGameInfo { get; init; }
    public Func<int, bool>? CommitGameInfoService { get; init; }
    public Func<int, bool>? QueueRosterService { get; init; }
    public Func<int, bool>? IsRuntimeJoin { get; init; }
    public Func<int, bool>? HasBootstrapInfo { get; init; }
    public Func<int, bool>? QueueBootstrapService { get; init; }
    public Action<int>? MarkClientReady { get; init; }
    public Action<int>? NetClientUpdated { get; init; }
    public Func<int, bool>? BeginStartGameService { get; init; }
    public Func<int, bool>? CommitStartGameService { get; init; }
}
public sealed class HostCheckStartGameAcksContext
{
    public int ConnectedPlayers { get; set; }
    public int MaxClients { get; init; }
    public int RemoteClient { get; init; }
    public byte[] NetBuffer { get; init; } = [];
    public byte NcmdExit { get; init; }
    public byte NcmdSetup { get; init; }
    public byte PreHcdeService { get; init; }
    public byte CstatNone { get; init; }
    public int HcdeServiceHeaderSize { get; init; }
    public Func<HostGetConnectionResult>? GetConnection { get; init; }
    public Action<int, string>? RemoveClientConnection { get; init; }
    public Action<int, int>? UpdatePlayers { get; init; }
    public Func<int, bool>? IsHcdeConnect { get; init; }
    public Action<string>? DebugMark { get; init; }
    public Func<int, int, string, bool>? CheckHcdePregameService { get; init; }
    public Func<int, bool>? HasStartGameAck { get; init; }
    public Action<int, string>? NetLog { get; init; }
    public Action<int, bool>? SetHasStartGameAck { get; init; }
    public Func<int, byte>? GetStatus { get; init; }
    public Func<int, int, string, (bool Dropped, int ConnectedPlayers)>? DropClientForHcdeTimeout { get; init; }
    public Action<int>? FlushReliableServices { get; init; }
}

public sealed class HostGameContext
{
    public int Arg { get; init; }
    public int ConsolePlayer { get; init; }
    public int MaxPlayers { get; init; }
    public int MaxClients { get; set; }
    public bool DedicatedServerMode { get; init; }
    public bool DedicatedServerAbortRequested { get; init; }
    public int SvMaxPlayers { get; init; }
    public uint GamePort { get; init; }
    public Func<int>? NumArgs { get; init; }
    public Func<int, string?>? GetArg { get; init; }
    public Func<string?, (bool Ok, int Value)>? TryParseClientCount { get; init; }
    public Action<string>? DebugMark { get; init; }
    public Action<string>? DebugWarning { get; init; }
    public Action<bool>? SetDedicatedServerStartRequested { get; init; }
    public Action<int>? ExitHost { get; init; }
    public Action<int>? SetMaxClients { get; init; }
    public Action<string>? FatalError { get; init; }
    public Action<int, string>? SetNetworkDetails { get; init; }
    public Action? GenerateGameId { get; init; }
    public Action<int>? AddNetworkClient { get; init; }
    public Action<int>? SetHostReady { get; init; }
    public Action? SetupUserInfo { get; init; }
    public Action<uint>? SetTicDup { get; init; }
    public Action<bool>? SetMultiplayer { get; init; }
    public Action<bool>? StartNetwork { get; init; }
    public Action? InitMasters { get; init; }
    public Action? SetMasterState { get; init; }
    public Action? PrintDiagnostics { get; init; }
    public Action? SetConnectFlow { get; init; }
    public Action<string>? NetInit { get; init; }
    public Action<int, int>? UpdatePlayers { get; init; }
    public Action<uint, uint>? NetClientConnected { get; init; }
    public Action<string>? NetMessage { get; init; }
}

public sealed class SendAbortContext
{
    public byte[] NetBuffer { get; init; } = [];
    public int NetBufferLength { get; set; }
    public int ConsolePlayer { get; init; }
    public int MaxClients { get; init; }
    public byte NcmdExit { get; init; }
    public byte CstatNone { get; init; }
    public Func<int, byte>? GetStatus { get; init; }
    public Action<int>? SendPacket { get; init; }
}
public sealed class HostCheckForConnectionsContext
{
    public int ConnectedPlayers { get; set; }
    public int MaxClients { get; init; }
    public int MaxPlayers { get; init; }
    public int RemoteClient { get; init; }
    public int NetBufferLength { get; init; }
    public byte[] NetBuffer { get; init; } = [];
    public bool DedicatedServerMode { get; init; }
    public bool SvDedicatedAutostart { get; init; }
    public byte NcmdSetup { get; init; }
    public byte NcmdExit { get; init; }
    public byte PreConnect { get; init; }
    public byte PreHcdeService { get; init; }
    public byte PreKicked { get; init; }
    public byte PreBanned { get; init; }
    public byte CstatNone { get; init; }
    public byte CstatConnecting { get; init; }
    public byte CstatWaiting { get; init; }
    public byte CstatReady { get; init; }
    public int HcdeServiceHeaderSize { get; init; }
    public Func<bool>? HasNetPassword { get; init; }
    public Func<bool>? ShouldStartNetGame { get; init; }
    public Action<bool>? SetDedicatedServerStartRequested { get; init; }
    public Action<string>? Printf { get; init; }
    public Func<IReadOnlyList<int>>? GetKickClients { get; init; }
    public Func<IReadOnlyList<int>>? GetBanClients { get; init; }
    public Func<int, byte>? GetStatus { get; init; }
    public Func<int, (uint, ushort)>? GetClientAddress { get; init; }
    public Action<int, string>? RemoveClientConnection { get; init; }
    public Action<int, int>? UpdatePlayers { get; init; }
    public Action<(uint, ushort), byte>? RejectConnection { get; init; }
    public Action<(uint, ushort)>? PushBannedConnection { get; init; }
    public Func<HostGetConnectionResult>? GetConnection { get; init; }
    public Func<(uint, ushort), bool, bool, bool, int, int?>? TryProcessSetupConnect { get; init; }
    public Action? UpdateMaster { get; init; }
    public Func<int, int, string, (bool Dropped, int ConnectedPlayers)>? DropClientForHcdeTimeout { get; init; }
    public Action<int>? FlushReliableServices { get; init; }
    public Func<int, bool>? IsHcdeConnect { get; init; }
    public Action<string>? DebugMark { get; init; }
    public Func<int, int, string, bool>? CheckHcdePregameService { get; init; }
    public Action<int, int>? ReadUserInfo { get; init; }
    public Action<int, byte>? SetStatus { get; init; }
    public Action<int, uint>? NetClientConnected { get; init; }
    public Action<int, int, bool>? SetClientAck { get; init; }
    public Action<int, bool>? SetHasGameInfo { get; init; }
    public Action<int, bool>? SetHasMapLoadInfo { get; init; }
    public Action<int, bool>? SetHasRosterInfo { get; init; }
    public Func<int, bool>? HasBootstrapInfo { get; init; }
    public Action<int, string>? BeginRuntimeBootstrap { get; init; }
    public Action<int, bool>? SetHasBootstrapInfo { get; init; }
    public Action<int, string>? RequestRuntimeResync { get; init; }
    public Action<int>? QueueBootstrapResync { get; init; }
    public Func<int, bool>? HasStartGameAck { get; init; }
    public Action<int, string>? NetLog { get; init; }
    public Action<int, bool>? SetHasStartGameAck { get; init; }
    public Action? IncrementUnsupported { get; init; }
    public Func<int, bool>? ShouldResendConnectAck { get; init; }
    public Action<int, int>? SendConnectAckPacket { get; init; }
    public Func<int, bool>? BeginConsolePlayerService { get; init; }
    public Action<int, int>? WriteConsolePlayerPayload { get; init; }
    public Func<int, bool>? CommitConsolePlayerService { get; init; }
    public Func<int, int, bool>? ClientGotAck { get; init; }
    public Func<int, bool>? BeginUserInfoAckService { get; init; }
    public Func<int, bool>? CommitUserInfoAckService { get; init; }
    public Func<int, bool>? HasMapLoadInfo { get; init; }
    public Func<int, bool>? BeginMapLoadService { get; init; }
    public Action<int>? AppendMapLoadInfo { get; init; }
    public Func<int, bool>? CommitMapLoadService { get; init; }
    public Func<int, bool>? HasGameInfo { get; init; }
    public Func<int, bool>? BeginGameInfoService { get; init; }
    public Action<int>? AppendGameInfo { get; init; }
    public Func<int, bool>? CommitGameInfoService { get; init; }
    public Func<int, bool>? HasRosterInfo { get; init; }
    public Func<int, bool>? QueueRosterService { get; init; }
    public Func<int, bool>? IsRuntimeJoin { get; init; }
    public Func<int, bool>? QueueBootstrapService { get; init; }
    public Action<int>? NetClientUpdated { get; init; }
    public Func<int, bool>? HasPendingReliableService { get; init; }
    public Action<int, int>? SendHeartbeat { get; init; }
}

public sealed class JoinGameContext
{
    public int Arg { get; init; }
    public int MaxClients { get; init; }
    public uint GamePort { get; init; }
    public string ResolvedHostAddress { get; init; } = "";
    public bool DedicatedJoinMode { get; init; }
    public byte CstatNone { get; init; }
    public Func<int>? NumArgs { get; init; }
    public Func<int, string?>? GetArg { get; init; }
    public Action<string>? DebugMark { get; init; }
    public Action<string>? FatalError { get; init; }
    public Action<int>? SetConsolePlayer { get; init; }
    public Action? ResetLateJoinState { get; init; }
    public Action? ClearHostSlot { get; init; }
    public Action<bool>? StartNetwork { get; init; }
    public Action<string>? BuildHostAddress { get; init; }
    public Action? SetHostConnecting { get; init; }
    public Action? SetConnectFlowAuth { get; init; }
    public Action<string, bool>? NetInit { get; init; }
    public Action<uint, int>? UpdatePlayers { get; init; }
    public Action<uint>? NetClientUpdated { get; init; }
    public Func<bool>? NetLoopGuestContact { get; init; }
    public Action? LogJoinAbort { get; init; }
    public Action? SendAbort { get; init; }
    public Action<int>? ExitHost { get; init; }
    public Action? LogJoinComplete { get; init; }
    public Func<int, byte>? GetStatus { get; init; }
    public Action<int>? SetStatusReady { get; init; }
    public Action? SetConnectFlowSyncing { get; init; }
    public Action<int, string>? SetNetworkDetails { get; init; }
    public Action? PrintDiagnostics { get; init; }
    public Action? NetLogPlayers { get; init; }
    public Action? NetDone { get; init; }
}

public sealed class GuestContactHostContext
{
    public int RemoteClient { get; init; }
    public int NetBufferLength { get; set; }
    public byte[] NetBuffer { get; init; } = [];
    public byte NcmdSetup { get; init; }
    public byte PreFull { get; init; }
    public byte PreInProgress { get; init; }
    public byte PreWrongPassword { get; init; }
    public byte PreVerificationError { get; init; }
    public byte PreKicked { get; init; }
    public byte PreBanned { get; init; }
    public byte PreProtocolError { get; init; }
    public byte PreSetupTimeout { get; init; }
    public byte PreConnectAck { get; init; }
    public byte PreHcdeService { get; init; }
    public byte PreConnectAckDedicated { get; init; }
    public byte PreConnectAckHcdeService { get; init; }
    public byte HcdeConnectProtocolVersion { get; init; }
    public int HcdeServiceHeaderSize { get; init; }
    public int MaxPlayers { get; init; }
    public int MaxTicDup { get; init; }
    public int MaxClients { get; set; }
    public int ConsolePlayer { get; set; } = -1;
    public bool DedicatedJoinMode { get; set; }
    public bool DedicatedLateJoinRetryAttempted { get; set; }
    public bool DedicatedLateJoinRetryPendingSend { get; set; }
    public uint HostSessionToken { get; set; }
    public (uint, ushort) HostAddress { get; set; }
    public uint HostHcdeServiceRxSeq { get; set; }
    public ulong GuestLastSetupProgressTime { get; set; }
    public uint GuestLastSetupRxSeq { get; set; }
    public ulong HcdeGuestSetupProgressTimeoutMs { get; init; }
    public bool EndOfGuestContactTick { get; set; }
    public byte CstatNone { get; init; }
    public byte CstatConnecting { get; init; }
    public byte CstatWaiting { get; init; }
    public Func<HostGetConnectionResult>? GetConnection { get; init; }
    public Action<string>? FatalNetError { get; init; }
    public Action<string>? DebugMark { get; init; }
    public Action<string>? NetMessage { get; init; }
    public Action<string>? NetLog { get; init; }
    public Action<string>? Printf { get; init; }
    public Func<byte[], string>? ReadVerificationError { get; init; }
    public Func<int, uint>? ReadSessionToken { get; init; }
    public Action<uint>? SetHostSessionToken { get; init; }
    public Action? ClearHostSessionToken { get; init; }
    public Action? ClearHostHcdeConnect { get; init; }
    public Action<byte, byte>? SetHostHcdeConnect { get; init; }
    public Func<byte>? GetHostStatus { get; init; }
    public Action<byte>? SetHostStatus { get; init; }
    public Action<uint>? NetClientUpdated { get; init; }
    public Action<byte, int>? UpdatePlayers { get; init; }
    public Func<bool>? IsHcdeConnect { get; init; }
    public Func<int, int, string, bool>? CheckHcdePregameService { get; init; }
    public Func<int>? GetFirstPlayableClientSlot { get; init; }
    public Action<int>? SetConsolePlayer { get; init; }
    public Action<int>? AddNetworkClient { get; init; }
    public Action<int, byte>? SetClientStatus { get; init; }
    public Action<int>? CopyHostSessionToClient { get; init; }
    public Action<int, byte>? SetClientHcdeConnect { get; init; }
    public Action? SetupUserInfo { get; init; }
    public Action<int, uint>? NetClientConnected { get; init; }
    public Action<int, int, bool>? SetClientAck { get; init; }
    public Func<int, byte>? GetClientStatus { get; init; }
    public Func<int, bool>? BeginReliableUserInfoAck { get; init; }
    public Action<(uint, ushort)>? CommitReliableUserInfoAck { get; init; }
    public Func<int, bool>? HasMapLoadInfo { get; init; }
    public Action<int>? ReadMapLoadInfo { get; init; }
    public Action<int, bool>? SetHasMapLoadInfo { get; init; }
    public Func<bool>? BeginReliableMapLoadAck { get; init; }
    public Action<(uint, ushort)>? CommitReliableMapLoadAck { get; init; }
    public Func<int, bool>? HasGameInfo { get; init; }
    public Action<uint>? SetTicDup { get; init; }
    public Action<byte[]>? SetGameId { get; init; }
    public Action<int, int>? ReadServerInfo { get; init; }
    public Action<int, bool>? SetHasGameInfo { get; init; }
    public Func<bool>? BeginReliableGameInfoAck { get; init; }
    public Action<(uint, ushort)>? CommitReliableGameInfoAck { get; init; }
    public Func<int, (uint, ushort), bool>? ReadRosterEntries { get; init; }
    public Action<int, bool>? SetHasRosterInfo { get; init; }
    public Func<bool>? BeginReliableRosterAck { get; init; }
    public Action<(uint, ushort)>? CommitReliableRosterAck { get; init; }
    public Action<int, bool>? SetHasBootstrapInfo { get; init; }
    public Action<byte>? AdoptServerRoomId { get; init; }
    public Action? AckBootstrap { get; init; }
    public Func<bool>? BeginReliableStartGameAck { get; init; }
    public Action<(uint, ushort)>? CommitReliableStartGameAck { get; init; }
    public Action<(uint, ushort)>? FlushReliableServices { get; init; }
    public Action? IncrementUnsupported { get; init; }
    public Func<ulong>? MsTime { get; init; }
    public Func<bool>? HasPendingReliableService { get; init; }
    public Action? BeginHeartbeatPacket { get; init; }
    public Action? BeginConnectPacket { get; init; }
    public Action<(uint, ushort)>? SendPacket { get; init; }
    public Func<int, bool>? BeginReliableClientUserInfo { get; init; }
    public Action<int>? AppendUserInfo { get; init; }
    public Action<(uint, ushort), int>? CommitReliableClientUserInfo { get; init; }
}

public readonly struct HostGetConnectionResult(bool gotPacket, (uint, ushort) from)
{
    public bool GotPacket { get; } = gotPacket;
    public (uint, ushort) From { get; } = from;
}
