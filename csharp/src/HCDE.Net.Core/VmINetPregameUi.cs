namespace HCDE.Net.Core;

/// <summary>Pregame UI/session hooks (<c>i_net.cpp</c> 1641–1810).</summary>
public static class VmINetPregameUi
{
    public const uint CflNone = 0;
    public const uint CflHost = 1;
    public const uint CflConsolePlayer = 2;

    public static void INetErrorNative(
        string? error,
        Action<string>? netLog,
        Action<string>? debugWarning,
        bool dedicatedServerMode,
        bool silentNetStartMode,
        Action? netClose,
        Action<string>? fatalError)
    {
        var message = string.IsNullOrEmpty(error) ? "unknown network error" : error;
        netLog?.Invoke($"Fatal network error: {message}");
        debugWarning?.Invoke(message);
        if (!dedicatedServerMode && !silentNetStartMode)
            netClose?.Invoke();
        fatalError?.Invoke(message);
    }

    public static void INetInitNative(
        string msg,
        bool host,
        bool dedicatedServerMode,
        bool silentNetStartMode,
        Action<string>? printSession,
        Action<string>? setDedicatedStatus,
        Action<string>? guardClientSubsystem,
        Action<string, bool>? netInit)
    {
        printSession?.Invoke(msg);
        if (dedicatedServerMode)
            setDedicatedStatus?.Invoke(msg);
        if (!dedicatedServerMode && !silentNetStartMode)
        {
            guardClientSubsystem?.Invoke("network session window");
            netInit?.Invoke(msg, host);
        }
    }

    public static void INetMessageNative(
        string msg,
        bool dedicatedServerMode,
        bool silentNetStartMode,
        Action<string>? printSession,
        Action<string>? setDedicatedStatus,
        Action<string>? guardClientSubsystem,
        Action<string>? netMessage)
    {
        printSession?.Invoke(msg);
        if (dedicatedServerMode)
            setDedicatedStatus?.Invoke(msg);
        if (!dedicatedServerMode && !silentNetStartMode)
        {
            guardClientSubsystem?.Invoke("network session message");
            netMessage?.Invoke(msg);
        }
    }

    public static bool INetLoopNative(
        bool dedicatedServerMode,
        bool silentNetStartMode,
        Func<bool>? loopCallback,
        Func<bool>? dedicatedServerAbortRequested,
        Action? pollRcon,
        Action? pumpDedicatedConsole,
        Func<Func<bool>?, bool>? netStartWindowLoop)
    {
        if (dedicatedServerMode || silentNetStartMode)
        {
            for (;;)
            {
                if (dedicatedServerMode)
                    pollRcon?.Invoke();
                if (loopCallback?.Invoke() == true)
                    break;
                if (dedicatedServerMode)
                {
                    pumpDedicatedConsole?.Invoke();
                    if (dedicatedServerAbortRequested?.Invoke() == true)
                        return false;
                }
                Thread.Sleep(1);
            }
            if (dedicatedServerMode)
                pumpDedicatedConsole?.Invoke();
            return dedicatedServerAbortRequested?.Invoke() != true;
        }
        return netStartWindowLoop?.Invoke(loopCallback) ?? false;
    }

    public static void INetClientConnectedNative(
        int client,
        uint charLimit,
        Func<int, bool>? isServerReservedSlot,
        Action<string>? printReservedReady,
        Func<int, uint, string>? getClientName,
        Func<int, bool>? isHcdeServiceAuthoritySlot,
        int consolePlayer,
        byte connectedStatus,
        Action<int, string, uint, byte>? netConnect)
    {
        if (isServerReservedSlot?.Invoke(client) == true)
        {
            printReservedReady?.Invoke("Dedicated server authority slot ready.");
            return;
        }
        var displayName = getClientName?.Invoke(client, 0) ?? "";
        printReservedReady?.Invoke($"Client '{displayName}' connected.");
        var name = getClientName?.Invoke(client, charLimit) ?? "";
        uint flags = CflNone;
        if (isHcdeServiceAuthoritySlot?.Invoke(client) == true && isServerReservedSlot?.Invoke(client) != true)
            flags |= CflHost;
        if (client == consolePlayer)
            flags |= CflConsolePlayer;
        netConnect?.Invoke(client, name, flags, connectedStatus);
    }

    public static void INetClientUpdatedNative(int client, byte status, Action<int, byte>? netUpdate) =>
        netUpdate?.Invoke(client, status);

    public static void INetClientDisconnectedNative(
        int client,
        string? reason,
        Func<int, string>? getClientName,
        Action<string>? printDisconnect,
        Action<int, string, string?>? netDisconnect)
    {
        var hasReason = !string.IsNullOrEmpty(reason);
        var name = getClientName?.Invoke(client) ?? "";
        printDisconnect?.Invoke(
            $"Client '{name}' disconnected{(hasReason ? $" ({reason})" : "")}.");
        netDisconnect?.Invoke(client, name, reason);
    }

    public static void INetUpdatePlayersNative(
        int current,
        int limit,
        Func<bool>? usesDedicatedServerSlot,
        Action<int, int>? netProgress)
    {
        if (usesDedicatedServerSlot?.Invoke() == true)
        {
            current = Math.Max(current - 1, 0);
            limit = Math.Max(limit - 1, 0);
        }
        netProgress?.Invoke(current, limit);
    }

    public static bool IShouldStartNetGameNative(
        bool dedicatedServerMode,
        bool dedicatedServerStartRequested,
        Func<bool>? shouldStartNet) =>
        dedicatedServerMode ? dedicatedServerStartRequested : shouldStartNet?.Invoke() ?? false;

    public static void IGetKickClientsNative(List<int> clients, Func<int>? getNetKickClient)
    {
        clients.Clear();
        int c;
        while ((c = getNetKickClient?.Invoke() ?? -1) != -1)
            clients.Add(c);
    }

    public static void IGetBanClientsNative(List<int> clients, Func<int>? getNetBanClient)
    {
        clients.Clear();
        int c;
        while ((c = getNetBanClient?.Invoke() ?? -1) != -1)
            clients.Add(c);
    }

    public static void INetDoneNative(Action? netDone) => netDone?.Invoke();
}
