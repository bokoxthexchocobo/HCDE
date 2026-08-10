using System.Text;
using HCDE.Net.Transport;

namespace HCDE.Net.Pregame;

public readonly struct ConnectPacket
{
    public ConnectPacket(EngineInfoSnapshot engineInfo, string password, byte connectVersion, HcdeConnectFlags connectFlags)
    {
        EngineInfo = engineInfo;
        Password = password;
        ConnectVersion = connectVersion;
        ConnectFlags = connectFlags;
    }

    public EngineInfoSnapshot EngineInfo { get; }
    public string Password { get; }
    public byte ConnectVersion { get; }
    public HcdeConnectFlags ConnectFlags { get; }
    public bool HasConnectInfo { get; init; }
}

/// <summary>
/// PRE_CONNECT build/parse matching <c>TryProcessSetupConnectPacket</c> layout.
/// </summary>
public static class ConnectPacketCodec
{
    public const int EngineInfoOffset = 2;
    public const int MinimumSize = 9;

    public static int Write(
        Span<byte> netBuffer,
        EngineInfoSnapshot engineInfo,
        string password,
        HcdeConnectFlags connectFlags)
    {
        if (netBuffer.Length < MinimumSize + HcdeConnectInfo.EncodedSize)
            return 0;

        netBuffer[PregameConstants.SetupCommandOffset] = (byte)NetCommandFlags.Setup;
        netBuffer[PregameConstants.SetupTypeOffset] = (byte)PregameSetupType.Connect;

        var engineLength = EngineInfoCodec.Write(netBuffer[EngineInfoOffset..], engineInfo);
        if (engineLength == 0)
            return 0;

        var offset = EngineInfoOffset + engineLength;
        var passwordBytes = Encoding.ASCII.GetBytes(password);
        if (offset + passwordBytes.Length + 1 + HcdeConnectInfo.EncodedSize > netBuffer.Length)
            return 0;

        passwordBytes.CopyTo(netBuffer[offset..]);
        netBuffer[offset + passwordBytes.Length] = 0;
        offset += passwordBytes.Length + 1;
        offset += HcdeConnectInfo.Write(netBuffer[offset..], PregameConstants.ConnectProtocolVersion, connectFlags);
        return offset;
    }

    public static bool TryRead(ReadOnlySpan<byte> netBuffer, out ConnectPacket packet)
    {
        packet = default;
        if (netBuffer.Length < MinimumSize)
            return false;
        if (netBuffer[PregameConstants.SetupCommandOffset] != (byte)NetCommandFlags.Setup)
            return false;
        if (netBuffer[PregameConstants.SetupTypeOffset] != (byte)PregameSetupType.Connect)
            return false;

        if (!EngineInfoCodec.TryRead(netBuffer[EngineInfoOffset..], out var engineInfo, out var engineBytesRead))
            return false;

        var offset = EngineInfoOffset + engineBytesRead;
        if (offset >= netBuffer.Length)
            return false;

        var passwordStart = offset;
        while (offset < netBuffer.Length && netBuffer[offset] != 0)
            offset++;
        if (offset >= netBuffer.Length)
            return false;

        var password = Encoding.ASCII.GetString(netBuffer[passwordStart..offset]);
        offset++;

        byte connectVersion = 0;
        HcdeConnectFlags connectFlags = HcdeConnectFlags.None;
        var hasConnectInfo = HcdeConnectInfo.TryRead(netBuffer[offset..], out connectVersion, out connectFlags);

        packet = new ConnectPacket(engineInfo, password, connectVersion, connectFlags)
        {
            HasConnectInfo = hasConnectInfo,
        };
        return true;
    }
}
