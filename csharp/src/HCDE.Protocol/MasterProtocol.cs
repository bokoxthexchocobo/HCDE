namespace HCDE.Protocol;

/// <summary>
/// Protocol-only constants for HCDE master discovery.
/// Mirrors <c>protocol/hcde_master_protocol.h</c> and <c>protocol/hcde_master_protocol.json</c>.
/// </summary>
public static class MasterProtocol
{
    public const uint Version = 2;

    public const string DefaultMasterHost = "hcde.servebeer.com";
    public const ushort DefaultMasterPort = 15000;
    public const int DefaultEntryTtlSeconds = 180;
    public const int ServerHeartbeatIntervalSeconds = 25;
    public const int ServerAddressReresolveIntervalSeconds = 10800;

    public const uint ServerHeartbeatMarker = 5560020;
    public const uint LauncherListQueryMarker = 777123;
    public const uint MasterListResponseMarker = 777123;

    public const ushort ServerHeartbeatPacketSize = 6;
    public const ushort LauncherListQueryPacketSize = 4;
    public const ushort MasterListResponseHeaderSize = 6;
    public const ushort MasterListResponseEntrySize = 6;

    public const string Nms1Magic = "NMS1";
    public const byte Nms1Version = 1;
    public const ushort Nms1HeaderSize = 16;
    public const ushort Nms1ChallengeTokenSize = 32;
    public const ushort Nms1EntryTokenSize = 32;
    public const ushort Nms1MaxPacketSize = 1200;
    public const ushort Nms1MaxProtocolFamilyBytes = 32;
    public const ushort Nms1MaxBuildLabelBytes = 64;
    public const ushort Nms1MaxDisplayNameBytes = 96;
    public const ushort Nms1MaxGameNameBytes = 64;
    public const ushort Nms1MaxMapNameBytes = 64;
    public const string Nms1DefaultProtocolFamily = "raw";
}

public enum Nms1MessageType : byte
{
    ChallengeRequest = 1,
    ChallengeResponse = 2,
    Register = 3,
    RegisterAck = 4,
    Heartbeat = 5,
    HeartbeatAck = 6,
    Unregister = 7,
    UnregisterAck = 8,
    ListRequest = 9,
    ListResponse = 10,
    Error = 11,
}

public enum Nms1ChallengePurpose : byte
{
    Registration = 1,
    ListQuery = 2,
}

public enum Nms1FieldType : ushort
{
    Purpose = 1,
    ChallengeIssuedUnix = 2,
    ChallengeToken = 3,
    ProtocolFamily = 16,
    GamePort = 17,
    QueryPort = 18,
    EntryToken = 19,
    CurrentPlayers = 20,
    MaxPlayers = 21,
    ServerFlags = 22,
    PublicIp = 23,
    BuildLabel = 24,
    DisplayName = 25,
    GameName = 26,
    MapName = 27,
    Cursor = 31,
    PageSize = 32,
    EntryCount = 33,
    TotalCount = 34,
    TtlSeconds = 35,
    ErrorCode = 100,
    ErrorText = 101,
    Entries = 200,
}

public enum Nms1ErrorCode : ushort
{
    BadPacket = 1,
    UnsupportedVersion = 2,
    UnknownMessage = 3,
    MissingField = 4,
    InvalidField = 5,
    ChallengeRequired = 6,
    ChallengeInvalid = 7,
    RateLimited = 8,
    PrivateAddress = 9,
    EntryLimit = 10,
    StaleEntry = 11,
    TokenInvalid = 12,
    ServerBusy = 13,
}
