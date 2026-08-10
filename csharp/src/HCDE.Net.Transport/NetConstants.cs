namespace HCDE.Net.Transport;

/// <summary>
/// Core networking constants from <c>src/common/engine/i_net.h</c> and <c>i_net.cpp</c>.
/// </summary>
public static class NetConstants
{
    public const int MaxPlayers = 64;
    public const int BackupTics = 35 * 5;
    public const int MaxTicDup = 3;
    public const int MaxSendTics = 35;
    public const int StabilityTics = 17;
    public const int LocalCmdTics = BackupTics * MaxTicDup;
    public const int MaxMessageLength = 14000;
    public const int MaxTransmitSize = 8000;
    public const int MinCompressionSize = 512;
    public const int MaxPasswordSize = 256;

    public const int DefaultGamePort = 5029; // IPPORT_USERRESERVED (5000) + 29

    public const int ProtoChallenge = -5560020;
    public const int MsgChallenge = 5560020;
    public const int LauncherChallenge = 777123;
    public const ushort OdamexQueryTagId = 0x0AD0;
}

public enum NetCommand : byte
{
    None = 0,
    Send = 1,
    Get = 2,
}

[Flags]
public enum NetCommandFlags : byte
{
    None = 0,
    Latency = 0x01,
    LatencyAck = 0x02,
    Compressed = 0x04,
    Quitters = 0x08,
    LevelReady = 0x10,
    Setup = 0x20,
    Retransmit = 0x40,
    Exit = 0x80,
}
