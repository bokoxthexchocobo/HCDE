namespace HCDE.Net.Core;

/// <summary>Master server host parsing and NMS1 helper utilities (<c>sv_master.cpp</c> 121–282).</summary>
public static class VmSvMasterResolve
{
    public const ushort DefaultMasterPort = 28910;

    public static bool IsMasterNameNative(string host)
    {
        foreach (var c in host)
        {
            if ((c < '0' || c > '9') && c != '.')
                return true;
        }
        return false;
    }

    public static bool TryParseMasterEndpointNative(
        string masterIp,
        ushort defaultPort,
        out string host,
        out ushort port,
        Action<string>? logMalformedPort)
    {
        host = masterIp;
        port = defaultPort;
        var portSep = masterIp.IndexOf(':');
        if (portSep < 0)
            return true;

        host = masterIp[..portSep];
        var portText = masterIp[(portSep + 1)..];
        if (portText.Length == 0
            || !int.TryParse(portText, out var portConversion)
            || portConversion <= 0
            || portConversion > ushort.MaxValue)
        {
            logMalformedPort?.Invoke(portText);
            return true;
        }

        port = (ushort)portConversion;
        return true;
    }

    public static ushort GetAdvertisedGamePortNative(int svNatPort, Func<ushort>? getGamePort)
    {
        if (svNatPort > 0 && svNatPort <= ushort.MaxValue)
            return (ushort)svNatPort;
        return getGamePort?.Invoke() ?? 0;
    }

    public static bool SameMasterAddressNative(
        uint leftFamily,
        ushort leftPort,
        uint leftAddr,
        uint rightFamily,
        ushort rightPort,
        uint rightAddr) =>
        leftFamily == rightFamily && leftPort == rightPort && leftAddr == rightAddr;

    public static void ResetNms1RegistrationNative(ref MasterServerNms1State state)
    {
        state.Registered = false;
        state.EntryToken = Array.Empty<byte>();
        state.GamePort = 0;
        state.TtlSeconds = 0;
        state.LastRefresh = 0;
        state.LastRegisterAttempt = 0;
    }

    public static ulong ElapsedSecondsSinceNative(ulong timestamp, ulong now)
    {
        if (timestamp == 0 || now < timestamp)
            return 0;
        return (now - timestamp) / 1000;
    }

    public static ushort RemainingNms1TtlNative(in MasterServerNms1State state, ulong now)
    {
        var elapsed = ElapsedSecondsSinceNative(state.LastRefresh, now);
        if (elapsed >= state.TtlSeconds)
            return 0;
        return (ushort)(state.TtlSeconds - elapsed);
    }

    public static uint NextNms1RequestIdNative(ref MasterServerNms1State state)
    {
        var requestId = state.NextRequestId++;
        if (state.NextRequestId == 0)
            state.NextRequestId = 1;
        if (requestId == 0)
            return NextNms1RequestIdNative(ref state);
        return requestId;
    }

    public static string MakeNms1TextNative(string? raw, int maxBytes, string? fallback)
    {
        var text = SanitizeNms1Text(raw, maxBytes);
        if (text.Length == 0 && !string.IsNullOrEmpty(fallback))
            return MakeNms1TextNative(fallback, maxBytes, null);
        return text;
    }

    private static string SanitizeNms1Text(string? raw, int maxBytes)
    {
        if (string.IsNullOrEmpty(raw))
            return "";
        var builder = new System.Text.StringBuilder();
        foreach (var c in raw)
        {
            if (builder.Length >= maxBytes)
                break;
            if (c >= 0x20 || c == '\t')
                builder.Append(c);
            else if (builder.Length > 0 && builder[^1] != ' ')
                builder.Append(' ');
        }
        return builder.ToString().Trim();
    }
}

public struct MasterServerNms1State
{
    public uint NextRequestId;
    public bool Registered;
    public byte[] EntryToken;
    public ushort GamePort;
    public ushort TtlSeconds;
    public ulong LastRefresh;
    public ulong LastRegisterAttempt;
    public string LastError;

    public MasterServerNms1State()
    {
        NextRequestId = 1;
        EntryToken = Array.Empty<byte>();
        LastError = "";
    }
}
