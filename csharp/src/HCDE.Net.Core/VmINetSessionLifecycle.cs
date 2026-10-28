namespace HCDE.Net.Core;

/// <summary>Network startup/teardown and connect-flow helpers (<c>i_net.cpp</c> 1550–1638).</summary>
public static class VmINetSessionLifecycle
{
    public static void StartNetworkNative(
        bool autoPort,
        ushort gamePort,
        ref bool netgame,
        ref bool multiplayer,
        Func<int>? createUdpSocket,
        Action<int, ushort>? bindLocalPort,
        Action<int>? setNonBlocking)
    {
        netgame = true;
        multiplayer = true;
        var socket = createUdpSocket?.Invoke() ?? -1;
        bindLocalPort?.Invoke(socket, autoPort ? (ushort)0 : gamePort);
        setNonBlocking?.Invoke(socket);
    }

    public static void CloseNetworkNative(
        ref int mySocket,
        ref bool netgame,
        Action? shutdownMasters,
        Action<int>? closeSocket,
        ref bool bGameStarted,
        ref bool dedicatedJoinMode,
        ref bool dedicatedLateJoinRetryAttempted,
        ref bool dedicatedLateJoinRetryPendingSend,
        ref ulong guestHcdeLastSetupProgressTime,
        ref uint guestHcdeLastSetupRxSeq,
        ref bool dedicatedServerAbortRequested,
        Func<bool>? isDebugServerRunning,
        Action? wsaCleanup)
    {
        shutdownMasters?.Invoke();
        if (mySocket >= 0)
        {
            closeSocket?.Invoke(mySocket);
            mySocket = -1;
            netgame = false;
        }
        bGameStarted = false;
        dedicatedJoinMode = false;
        dedicatedLateJoinRetryAttempted = false;
        dedicatedLateJoinRetryPendingSend = false;
        guestHcdeLastSetupProgressTime = 0;
        guestHcdeLastSetupRxSeq = 0;
        dedicatedServerAbortRequested = false;
        if (isDebugServerRunning?.Invoke() != true)
            wsaCleanup?.Invoke();
    }

    public static void GenerateGameIdNative(Span<byte> gameId, Func<ulong>? genRand64)
    {
        var val = genRand64?.Invoke() ?? 0UL;
        BinaryPrimitivesWriteUInt64(gameId, val);
    }

    private static void BinaryPrimitivesWriteUInt64(Span<byte> gameId, ulong val)
    {
        for (var i = 0; i < 8 && i < gameId.Length; i++)
            gameId[i] = (byte)(val >> (56 - i * 8));
    }

    public static void INetLogNative(string text, Action<string>? printHigh)
    {
        printHigh?.Invoke(text);
    }

    public static void SetConnectFlowNative(
        byte newFlow,
        ref byte netConnectFlowState,
        Action<byte>? debugMarkConnectFlow)
    {
        if (netConnectFlowState == newFlow)
            return;
        netConnectFlowState = newFlow;
        debugMarkConnectFlow?.Invoke(newFlow);
    }
}
