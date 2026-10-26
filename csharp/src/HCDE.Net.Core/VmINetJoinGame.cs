namespace HCDE.Net.Core;

/// <summary>Client join setup (<c>i_net.cpp</c> 4182–4260).</summary>
public static class VmINetJoinGame
{
    public static bool JoinArgMissing(int arg, int numArgs, string joinArg) =>
        arg >= numArgs || (joinArg.Length > 0 && (joinArg[0] == '-' || joinArg[0] == '+'));

    public static void PromoteConnectedClientsToReady(
        int maxClients,
        byte cstatNone,
        Func<int, byte>? getStatus,
        Action<int>? setReady)
    {
        if (getStatus == null || setReady == null)
            return;
        for (var i = 1; i < maxClients; i++)
        {
            if (getStatus(i) != cstatNone)
                setReady(i);
        }
    }

    public static bool JoinGameNative(JoinGameContext ctx)
    {
        ctx.DebugMark?.Invoke($"join request arg={ctx.Arg}");
        var numArgs = ctx.NumArgs?.Invoke() ?? 0;
        var joinArg = ctx.GetArg?.Invoke(ctx.Arg) ?? "";
        if (JoinArgMissing(ctx.Arg, numArgs, joinArg))
            ctx.FatalError?.Invoke("You need to specify the host machine's address");
        ctx.SetConsolePlayer?.Invoke(-1);
        ctx.ResetLateJoinState?.Invoke();
        ctx.ClearHostSlot?.Invoke();
        ctx.StartNetwork?.Invoke(true);
        ctx.DebugMark?.Invoke($"join network ready port={ctx.GamePort}");
        ctx.BuildHostAddress?.Invoke(ctx.GetArg?.Invoke(ctx.Arg) ?? "");
        ctx.DebugMark?.Invoke($"join target arg='{ctx.GetArg?.Invoke(ctx.Arg)}' resolved={ctx.ResolvedHostAddress}");
        ctx.SetHostConnecting?.Invoke();
        ctx.SetConnectFlowAuth?.Invoke();
        ctx.NetInit?.Invoke("Joining server...", false);
        ctx.UpdatePlayers?.Invoke(0, ctx.MaxClients);
        ctx.NetClientUpdated?.Invoke(0);
        if (ctx.NetLoopGuestContact?.Invoke() != true)
        {
            ctx.LogJoinAbort?.Invoke();
            ctx.SendAbort?.Invoke();
            ctx.ExitHost?.Invoke(0);
            return false;
        }
        ctx.LogJoinComplete?.Invoke();
        PromoteConnectedClientsToReady(ctx.MaxClients, ctx.CstatNone, ctx.GetStatus, ctx.SetStatusReady);
        ctx.SetConnectFlowSyncing?.Invoke();
        ctx.SetNetworkDetails?.Invoke(ctx.MaxClients, ctx.DedicatedJoinMode ? "client-syncing" : "syncing");
        ctx.PrintDiagnostics?.Invoke();
        ctx.NetLogPlayers?.Invoke();
        ctx.NetDone?.Invoke();
        return true;
    }
}
