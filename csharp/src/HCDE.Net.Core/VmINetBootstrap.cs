namespace HCDE.Net.Core;

/// <summary>Early <c>i_net.cpp</c> CVars, protocol constants, and connection state (<c>82–447</c>).</summary>
public static class VmINetBootstrap
{
    public const int MaxPlayersLimit = 255;
    public const int MaxTransmitSize = 8000;
    public const int MinCompressionSize = 512;
    public const int MaxPasswordSize = 256;
    public const int HcdeServiceSequenceOffset = 7;
    public const int HcdeServiceAckOffset = 11;
    public const int HcdeServiceHeaderSize = 15;
    public const int MaxHcdeReliableServices = 16;
    public const ulong HcdeServiceResendMs = 250;
    public const ulong HcdeRuntimeConnectAckResendMs = 250;
    public const ulong HcdeServiceTimeoutMs = 15000;
    public const ulong HcdeGuestSetupProgressTimeoutMs = 30000;
    public const ulong HcdeServiceHardTimeoutMs = 300000;
    public const uint HcdeServiceMalformedStrikeLimit = 4;
    public const ulong HcdeServiceMalformedQuarantineMs = 3000;
    public const byte HcdeConnectProtocolVersion = 1;
    public static ReadOnlySpan<byte> HcdeConnectMagic => "HCD3"u8;

    public const byte PreConnect = 0;
    public const byte PreConnectAck = 1;
    public const byte PreDisconnect = 2;
    public const byte PreFull = 3;
    public const byte PreInProgress = 4;
    public const byte PreWrongPassword = 5;
    public const byte PreVerificationError = 6;
    public const byte PreKicked = 7;
    public const byte PreBanned = 8;
    public const byte PreProtocolError = 9;
    public const byte PreHcdeService = 10;
    public const byte PreSetupTimeout = 11;

    public const byte PreConnectAckDedicated = 1 << 0;
    public const byte PreConnectAckHcdeService = 1 << 1;
    public const byte PreConnectAckServerAuthority = 1 << 2;

    public const byte HcdeConnectDedicatedJoin = 1 << 0;
    public const byte HcdeConnectSuppressRoomUi = 1 << 1;
    public const byte HcdeConnectServerAuthority = 1 << 2;

    public const byte CstatNone = 0;
    public const byte CstatConnecting = 1;
    public const byte CstatWaiting = 2;
    public const byte CstatReady = 3;

    public const byte NcfIdle = 0;
    public const byte NcfServerWaiting = 1;
    public const byte NcfClientAuth = 2;
    public const byte NcfSyncing = 3;

    public const byte HpsHeartbeat = 1;
    public const byte HpsClientUserInfo = 2;
    public const byte HpsUserInfoAck = 3;
    public const byte HpsGameInfo = 4;
    public const byte HpsGameInfoAck = 5;
    public const byte HpsRoster = 6;
    public const byte HpsStartGame = 7;
    public const byte HpsConsolePlayer = 8;
    public const byte HpsMapLoad = 9;
    public const byte HpsMapLoadAck = 10;
    public const byte HpsStartGameAck = 11;
    public const byte HpsRosterAck = 12;
    public const byte HpsBootstrapBegin = 13;
    public const byte HpsBootstrapAck = 14;
    public const byte HpsResyncRequest = 15;
    public const byte HpsResyncBegin = 16;
    public const byte HpsResyncAck = 17;

    public const uint CflNone = 0;
    public const uint CflConsolePlayer = 1;
    public const uint CflHost = 1u << 1;

    public const ushort DefaultGamePort = 5000 + 29;

    public static int ClampSvMaxPlayersNative(int value, int maxPlayersMinusOne)
    {
        if (value < 0)
            return 0;
        if (value > maxPlayersMinusOne)
            return maxPlayersMinusOne;
        return value;
    }

    public static bool MatchesHcdeConnectMagicNative(ReadOnlySpan<byte> data) =>
        data.Length >= 4
        && data[0] == (byte)'H'
        && data[1] == (byte)'C'
        && data[2] == (byte)'D'
        && data[3] == (byte)'3';

    public static bool IsPreHcdeServicePacketNative(byte packetType) => packetType == PreHcdeService;

    public static bool HasPreConnectAckFlagNative(byte flags, byte mask) => (flags & mask) != 0;

    public static byte MergeHcdeConnectFlagsNative(byte existing, byte added) => (byte)(existing | added);

    public static bool ShouldUseCompressionNative(int payloadLength) => payloadLength >= MinCompressionSize;

    public static bool PasswordFitsBufferNative(string password) => password.Length + 1 <= MaxPasswordSize;

    public static void ClearHcdeConnectInfoNative(ref HcdeConnectInfoState info)
    {
        info.Present = false;
        info.Version = 0;
        info.Flags = 0;
    }

    public static bool IsConnectionReadyNative(byte status) => status == CstatReady;

    public static bool IsConnectionWaitingNative(byte status) => status == CstatWaiting;

    public static void ClearPendingServiceSlotNative(ref HcdePendingServiceSlot slot) => slot.Clear();

    public static void ClearConnectionFieldsNative(ref HcdeBootstrapConnectionState state)
    {
        state.Status = CstatNone;
        state.InfoAck = 0;
        state.HasGameInfo = false;
        state.HasMapLoadInfo = false;
        state.HasRosterInfo = false;
        state.HasBootstrapInfo = false;
        state.HasStartGameAck = false;
        state.SessionToken = 0;
        state.HcdeConnect = false;
        state.HcdeConnectVersion = 0;
        state.HcdeConnectFlags = 0;
        state.ServiceTxSeq = 0;
        state.ServiceRxSeq = 0;
        state.ServicePeerAck = 0;
        state.ServiceDuplicateCount = 0;
        state.ServiceMalformedStrikes = 0;
        state.ServiceMalformedUntil = 0;
        state.ServiceLastValidRxTime = 0;
        state.RuntimeLastConnectAckTime = 0;
        state.ServiceLastWaitLogTime = 0;
        state.ServiceLastReuseLogTime = 0;
        state.RuntimeJoin = false;
        if (state.PendingServices is not null)
        {
            for (var i = 0; i < state.PendingServices.Length; i++)
                state.PendingServices[i].Clear();
        }
    }

    public static void ResetGlobalNetStateNative(ref HcdeGlobalNetState state)
    {
        state.NetConnectFlow = NcfIdle;
        state.DedicatedServerMode = false;
        state.SilentNetStartMode = false;
        state.DedicatedJoinMode = false;
        state.DedicatedServerStartRequested = false;
        state.DedicatedServerAbortRequested = false;
        state.DedicatedLateJoinRetryAttempted = false;
        state.DedicatedLateJoinRetryPendingSend = false;
        state.GuestHcdeLastSetupProgressTime = 0;
        state.GuestHcdeLastSetupRxSeq = 0;
        state.Netgame = false;
        state.Multiplayer = false;
        state.ConsolePlayer = 0;
        state.NetArbitrator = 0;
        state.TicDup = 1;
        state.MaxClients = 1;
        state.RemoteClient = -1;
        state.NetBufferLength = 0;
        state.MySocket = -1;
        state.GameStarted = false;
        state.GamePort = DefaultGamePort;
    }
}

public struct HcdeBootstrapConnectionState
{
    public byte Status;
    public ulong InfoAck;
    public bool HasGameInfo;
    public bool HasMapLoadInfo;
    public bool HasRosterInfo;
    public bool HasBootstrapInfo;
    public bool HasStartGameAck;
    public uint SessionToken;
    public bool HcdeConnect;
    public byte HcdeConnectVersion;
    public byte HcdeConnectFlags;
    public uint ServiceTxSeq;
    public uint ServiceRxSeq;
    public uint ServicePeerAck;
    public uint ServiceDuplicateCount;
    public uint ServiceMalformedStrikes;
    public ulong ServiceMalformedUntil;
    public ulong ServiceLastValidRxTime;
    public ulong RuntimeLastConnectAckTime;
    public ulong ServiceLastWaitLogTime;
    public ulong ServiceLastReuseLogTime;
    public bool RuntimeJoin;
    public HcdePendingServiceSlot[]? PendingServices;
}

public struct HcdeConnectInfoState
{
    public bool Present;
    public byte Version;
    public byte Flags;
}

public struct HcdeGlobalNetState
{
    public byte NetConnectFlow;
    public bool DedicatedServerMode;
    public bool SilentNetStartMode;
    public bool DedicatedJoinMode;
    public bool DedicatedServerStartRequested;
    public bool DedicatedServerAbortRequested;
    public bool DedicatedLateJoinRetryAttempted;
    public bool DedicatedLateJoinRetryPendingSend;
    public ulong GuestHcdeLastSetupProgressTime;
    public uint GuestHcdeLastSetupRxSeq;
    public bool Netgame;
    public bool Multiplayer;
    public int ConsolePlayer;
    public int NetArbitrator;
    public byte TicDup;
    public int MaxClients;
    public int RemoteClient;
    public uint NetBufferLength;
    public int MySocket;
    public bool GameStarted;
    public ushort GamePort;
}
