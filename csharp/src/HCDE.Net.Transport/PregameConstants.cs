namespace HCDE.Net.Transport;

public enum PregameSetupType : byte
{
    Connect = 0,
    ConnectAck = 1,
    Disconnect = 2,
    Full = 3,
    InProgress = 4,
    WrongPassword = 5,
    VerificationError = 6,
    Kicked = 7,
    Banned = 8,
    ProtocolError = 9,
    HcdeService = 10,
    SetupTimeout = 11,
}

[Flags]
public enum PreConnectAckFlags : byte
{
    None = 0,
    Dedicated = 1 << 0,
    HcdeService = 1 << 1,
    ServerAuthority = 1 << 2,
}

[Flags]
public enum HcdeConnectFlags : byte
{
    None = 0,
    DedicatedJoin = 1 << 0,
    SuppressRoomUi = 1 << 1,
    ServerAuthority = 1 << 2,
}

public enum PregameServiceType : byte
{
    Heartbeat = 0,
    ClientUserInfo = 1,
    UserInfoAck = 2,
    GameInfo = 3,
    GameInfoAck = 4,
    MapLoad = 5,
    MapLoadAck = 6,
    StartGameAck = 7,
    RosterAck = 8,
    BootstrapBegin = 9,
    BootstrapAck = 10,
    ResyncRequest = 11,
    ResyncBegin = 12,
    ResyncAck = 13,
}

public enum ConnectionStatus : byte
{
    None = 0,
    Connecting = 1,
    Waiting = 2,
    Ready = 3,
}

public static class PregameConstants
{
    public const int ServiceSequenceOffset = 7;
    public const int ServiceAckOffset = 11;
    public const int ServiceHeaderSize = 15;
    public const int MaxReliableServices = 16;

    public const uint ServiceResendMilliseconds = 250;
    public const uint RuntimeConnectAckResendMilliseconds = 250;
    public const uint ServiceTimeoutMilliseconds = 15000;
    public const uint GuestSetupProgressTimeoutMilliseconds = 30000;
    public const uint ServiceHardTimeoutMilliseconds = 300000;
    public const uint ServiceMalformedStrikeLimit = 4;
    public const uint ServiceMalformedQuarantineMilliseconds = 3000;

    public const byte ConnectProtocolVersion = 1;
    public static ReadOnlySpan<byte> ConnectMagic => "HCD3"u8;
}
