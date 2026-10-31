using System.Globalization;

namespace HCDE.Net.Core;

/// <summary>
/// Dedicated-server runtime fields, argument parsing, and init through
/// <c>HCDE_ServerMode_SetAuthorityState</c> (<c>hcde_servermode.cpp</c> 1–218).
/// <c>HCDE_ServerMode_SetAuthoritySettingsController</c> at line 219 stays chunk 640.
/// </summary>
public sealed class VmServerMode1373Slice
{
    public const string ServCategory = "serv";
    public const string InitFormat =
        "init dedicated-exe=%d dedicated-srv=%d host=%d join=%d slots=%d port=%d map=%s";
    public const string NetworkFormat =
        "network visible=%d internal=%d port=%d reserved=%d flow=%s";
    public const string MasterFormat = "master advertising=%d configured=%zu";
    public const string AuthorityFormat =
        "authority slot=%d playerBacked=%d settings=%d waiting=%d name=%s";
    public const string DedicatedFatal =
        "hcdeserv was started without -server <slots>. Dedicated server builds must enter the explicit server runtime.";

    public bool Initialized;
    public bool DedicatedExecutable;
    public bool DedicatedServer;
    public bool HostMode;
    public bool JoinMode;
    public bool DedicatedJoin;
    public bool NetWaitSilent;
    public bool ReservedServerSlot;
    public bool MasterStateKnown;
    public bool MasterAdvertisingEnabled;
    public bool AuthorityStateKnown;
    public bool AuthorityPlayerBacked = true;
    public bool AuthoritySettingsController;
    public bool AuthorityWaiting;
    public ulong ConfiguredMasters;
    public int RequestedServerSlots;
    public int VisibleSlots = 1;
    public int InternalSlots = 1;
    public int GamePort;
    public int AuthoritySlot;
    public string IwadArg = "";
    public string SelectedIwad = "";
    public string MapArg = "";
    public string JoinAddress = "";
    public string FlowState = "";
    public string AuthorityName = "";
    public string?[]? Args;
    public List<ServerModeTrace> Traces { get; } = [];
    public List<string> Fatals { get; } = [];

    readonly bool _compiledDedicated;

    public VmServerMode1373Slice(bool dedicatedExecutable = false)
    {
        _compiledDedicated = dedicatedExecutable;
    }

    public void InitFromArgs()
    {
        if (Initialized)
            return;

        DedicatedExecutable = _compiledDedicated;
        if (Args == null)
            return;

        Initialized = true;
        DedicatedServer = CheckParm("-server") != 0;
        HostMode = CheckParm("-host") != 0;
        JoinMode = CheckParm("-join") != 0;
        DedicatedJoin = CheckParm("-dedicatedjoin") != 0 || CheckParm("-joindedicated") != 0;
        NetWaitSilent = CheckParm("-netwaitsilent") != 0;
        RequestedServerSlots = ReadIntValue("-server");
        GamePort = ReadIntValue("-port");
        IwadArg = AssignArgValue("-iwad");
        MapArg = AssignArgValue("+map");
        if (MapArg.Length == 0)
            MapArg = ReadWarpValue();
        JoinAddress = AssignArgValue("-join");
        if (JoinAddress.Length == 0)
            JoinAddress = AssignArgValue("-joindedicated");
        if (JoinAddress.Length == 0)
            JoinAddress = AssignArgValue("-dedicatedjoin");
        FlowState = "startup";
        Infof(InitFormat, string.Create(
            CultureInfo.InvariantCulture,
            $"init dedicated-exe={(DedicatedExecutable ? 1 : 0)} dedicated-srv={(DedicatedServer ? 1 : 0)} host={(HostMode ? 1 : 0)} join={(JoinMode ? 1 : 0)} slots={RequestedServerSlots} port={GamePort} map={TextOr(MapArg, "default")}"));
        if (DedicatedExecutable && !DedicatedServer)
        {
            Fatals.Add(DedicatedFatal);
            throw new InvalidOperationException(DedicatedFatal);
        }
    }

    public void SetSelectedIwad(string? iwadName)
    {
        InitFromArgs();
        SelectedIwad = iwadName != null ? iwadName : "";
    }

    public void SetNetworkDetails(int visibleSlots, int internalSlots, int gamePort, bool reservedServerSlot, string? flowState)
    {
        InitFromArgs();
        VisibleSlots = visibleSlots;
        InternalSlots = internalSlots;
        GamePort = gamePort;
        ReservedServerSlot = reservedServerSlot;
        FlowState = flowState != null ? flowState : "";
        Infof(NetworkFormat, string.Create(
            CultureInfo.InvariantCulture,
            $"network visible={visibleSlots} internal={internalSlots} port={gamePort} reserved={(reservedServerSlot ? 1 : 0)} flow={TextOr(FlowState, "?")}"));
    }

    public void SetMasterState(bool advertisingEnabled, ulong configuredMasters)
    {
        InitFromArgs();
        MasterStateKnown = true;
        MasterAdvertisingEnabled = advertisingEnabled;
        ConfiguredMasters = configuredMasters;
        Infof(MasterFormat, string.Create(
            CultureInfo.InvariantCulture,
            $"master advertising={(advertisingEnabled ? 1 : 0)} configured={configuredMasters}"));
    }

    public void SetAuthorityState(int internalSlot, bool playerBacked, bool settingsController, bool waiting, string? displayName)
    {
        InitFromArgs();
        AuthorityStateKnown = true;
        AuthoritySlot = internalSlot;
        AuthorityPlayerBacked = playerBacked;
        AuthoritySettingsController = settingsController;
        AuthorityWaiting = waiting;
        AuthorityName = displayName != null ? displayName : "";
        Markf(AuthorityFormat, string.Create(
            CultureInfo.InvariantCulture,
            $"authority slot={AuthoritySlot} playerBacked={(AuthorityPlayerBacked ? 1 : 0)} settings={(AuthoritySettingsController ? 1 : 0)} waiting={(AuthorityWaiting ? 1 : 0)} name={TextOr(AuthorityName, "unknown")}"));
    }

    public bool IsValueArg(int index)
    {
        var argv = Args;
        if (argv == null || index <= 0 || index >= argv.Length)
            return false;
        var text = argv[index];
        if (text == null)
            return false;
        if (text.Length == 0)
            return true;
        return text[0] != '-' && text[0] != '+';
    }

    public int CheckParm(string flag)
    {
        var argv = Args;
        if (argv == null)
            return 0;
        for (var i = 1; i < argv.Length; i++)
        {
            var text = argv[i];
            if (text != null && string.Equals(text, flag, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return 0;
    }

    public string? CheckValue(string flag)
    {
        var argv = Args;
        if (argv == null)
            return null;
        var index = CheckParm(flag);
        if (index > 0 && index < argv.Length - 1)
        {
            var text = argv[index + 1];
            if (text != null && text.Length != 0 && text[0] != '+' && text[0] != '-')
                return text;
        }
        return null;
    }

    public int ReadIntValue(string flag)
    {
        var index = CheckParm(flag);
        if (!IsValueArg(index + 1))
            return 0;
        var text = Args![index + 1];
        if (text == null)
            return 0;
        return StrictBase10(text);
    }

    public string AssignArgValue(string flag)
    {
        var value = CheckValue(flag);
        if (value == null)
            return "";
        return value;
    }

    public string ReadWarpValue()
    {
        var index = CheckParm("-warp");
        if (!IsValueArg(index + 1))
            return "";
        var text = Args![index + 1] ?? "";
        if (IsValueArg(index + 2))
            text = string.Create(CultureInfo.InvariantCulture, $"{text} {Args![index + 2]}");
        return text;
    }

    public static string TextOr(string text, string fallback)
    {
        if (text.Length != 0)
            return text;
        return fallback;
    }

    public static string YesNo(bool value)
    {
        if (value)
            return "yes";
        return "no";
    }

    public static int StrictBase10(string text)
    {
        var i = 0;
        while (i < text.Length && IsCSpace(text[i]))
            i++;
        if (i >= text.Length)
            return 0;
        var sign = 1;
        if (text[i] == '+' || text[i] == '-')
        {
            if (text[i] == '-')
                sign = -1;
            i++;
        }
        if (i >= text.Length || text[i] < '0' || text[i] > '9')
            return 0;
        ulong magnitude = 0;
        var overflow = false;
        while (i < text.Length && text[i] >= '0' && text[i] <= '9')
        {
            var digit = (ulong)(text[i] - '0');
            if (magnitude > (ulong.MaxValue - digit) / 10)
                overflow = true;
            else
                magnitude = magnitude * 10 + digit;
            i++;
        }
        if (i != text.Length || overflow)
            return 0;
        if (sign < 0)
        {
            if (magnitude > (ulong)int.MaxValue + 1)
                return 0;
            if (magnitude == (ulong)int.MaxValue + 1)
                return int.MinValue;
            return -(int)magnitude;
        }
        if (magnitude > (ulong)int.MaxValue)
            return 0;
        return (int)magnitude;
    }

    static bool IsCSpace(char c) => c is ' ' or '\t' or '\n' or '\v' or '\f' or '\r';

    void Infof(string format, string text) =>
        Traces.Add(new ServerModeTrace("Infof", ServCategory, format, text));

    void Markf(string format, string text) =>
        Traces.Add(new ServerModeTrace("Markf", ServCategory, format, text));
}

public readonly record struct ServerModeTrace(string Level, string Category, string Format, string Text);
