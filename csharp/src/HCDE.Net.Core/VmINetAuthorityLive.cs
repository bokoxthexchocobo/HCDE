namespace HCDE.Net.Core;

/// <summary>Client slot mapping, HCDE authority, and live routing (<c>i_net.cpp</c> 4417–4623).</summary>
public static class VmINetAuthorityLive
{
    public static bool ClientSlotInRange(int client, int maxClients) =>
        client >= 0 && client < maxClients;

    public static bool ClientSlotWithinPlayers(int client, int maxPlayers) =>
        client >= 0 && client < maxPlayers;

    public static bool IIsPlayableClientSlotNative(NetClientSlotContext ctx, int client) =>
        ClientSlotInRange(client, ctx.MaxClients) && !IIsServerReservedSlotNative(ctx, client);

    public static bool IVisibleClientSlotValidNative(NetClientSlotContext ctx, int visibleClient)
    {
        var maxVisible = IGetVisibleMaxClientsNative(ctx);
        return visibleClient >= 0 && visibleClient < maxVisible;
    }

    static string ResyncDebugReason(string? reason) =>
        string.IsNullOrEmpty(reason) ? "manual" : reason;

    public static bool IIsServerReservedSlotNative(NetClientSlotContext ctx, int client)
    {
        var reserved = IGetReservedServerSlotNative(ctx);
        return reserved >= 0 && client == reserved;
    }

    public static int IGetFirstPlayableClientSlotNative(NetClientSlotContext ctx)
    {
        var reserved = IGetReservedServerSlotNative(ctx);
        return reserved >= 0 ? reserved + 1 : 0;
    }

    public static int IGetVisibleMaxClientsNative(NetClientSlotContext ctx)
    {
        return IGetReservedServerSlotNative(ctx) >= 0 ? Math.Max(ctx.MaxClients - 1, 0) : ctx.MaxClients;
    }

    public static int IToVisibleClientSlotNative(NetClientSlotContext ctx, int client)
    {
        var reserved = IGetReservedServerSlotNative(ctx);
        if (reserved >= 0 && client > reserved)
            return client - 1;
        return client;
    }

    public static int IToInternalClientSlotNative(NetClientSlotContext ctx, int visibleClient)
    {
        var reserved = IGetReservedServerSlotNative(ctx);
        if (reserved >= 0 && visibleClient >= reserved)
            return visibleClient + 1;
        return visibleClient;
    }

    public static bool IClientUsesHcdeServiceNative(NetClientSlotContext ctx, int client) =>
        ClientSlotInRange(client, ctx.MaxClients) && ctx.IsHcdeConnect?.Invoke(client) == true;

    public static int IGetHcdeServiceAuthoritySlotNative(NetAuthorityContext ctx) =>
        ctx.UsesDedicatedServerSlot?.Invoke() == true && ctx.HasAuthorityState?.Invoke() == true
            ? ctx.GetAuthoritySlot?.Invoke() ?? -1
            : ctx.NetArbitrator;

    public static bool IIsLocalHcdeServiceAuthorityNative(NetAuthorityContext ctx)
    {
        if (ctx.DedicatedServerMode && ctx.HasAuthorityState?.Invoke() == true && ctx.IsAuthorityPlayerBacked?.Invoke() != true)
            return true;
        return ctx.ConsolePlayer == IGetHcdeServiceAuthoritySlotNative(ctx);
    }

    public static bool IIsHcdeServiceAuthoritySlotNative(NetAuthorityContext ctx, int client) =>
        ClientSlotInRange(client, ctx.MaxClients) && client == IGetHcdeServiceAuthoritySlotNative(ctx);

    public static bool IIsRemoteHcdeServiceAuthorityNative(NetAuthorityContext ctx, int client) =>
        !IIsLocalHcdeServiceAuthorityNative(ctx) && IIsHcdeServiceAuthoritySlotNative(ctx, client);

    public static int IGetHcdeLiveAuthoritySlotNative(NetAuthorityContext ctx) =>
        IGetHcdeServiceAuthoritySlotNative(ctx);

    public static void IResetHcdePregameServiceProfileNative(Action? clearProfile) => clearProfile?.Invoke();

    public static int ICountHcdePregameServiceQuarantinesNative(NetQuarantineContext ctx)
    {
        var now = ctx.MsTime?.Invoke() ?? 0;
        var quarantined = 0;
        for (var client = 0; client < ctx.MaxPlayers; client++)
        {
            if (ctx.GetMalformedUntil?.Invoke(client) > now)
                quarantined++;
        }
        return quarantined;
    }

    public static bool IRequestHcdeResyncNative(HcdeResyncContext ctx, string? reason)
    {
        if (!ctx.NetGame || ctx.DemoPlayback || IIsLocalHcdeServiceAuthorityNative(ctx.Authority))
            return false;
        if (ctx.IsHcdeConnect?.Invoke(0) != true || ctx.GetStatus?.Invoke(0) == ctx.CstatNone)
            return false;
        if (ctx.BeginReliableResyncRequest?.Invoke() != true)
            return false;
        ctx.SetNetBufferLength?.Invoke(ctx.HcdeServiceHeaderSize);
        ctx.DebugMark?.Invoke($"guest requesting HCDE resync reason={ResyncDebugReason(reason)}");
        ctx.CommitReliableResyncRequest?.Invoke();
        ctx.FlushReliableServices?.Invoke(true);
        return true;
    }

    public static bool IIsLocalHcdeLiveAuthorityNative(NetAuthorityContext ctx) =>
        IIsLocalHcdeServiceAuthorityNative(ctx);

    public static bool IIsHcdeLiveAuthoritySlotNative(NetAuthorityContext ctx, int client) =>
        IIsHcdeServiceAuthoritySlotNative(ctx, client);

    public static bool IIsHcdeClientSetupInProgressNative(NetClientSetupContext ctx, int client)
    {
        if (!IIsLocalHcdeServiceAuthorityNative(ctx.Authority))
            return false;
        if (!ClientSlotInRange(client, ctx.MaxClients) || !ClientSlotWithinPlayers(client, ctx.MaxPlayers))
            return false;
        if (ctx.GetStatus?.Invoke(client) == ctx.CstatNone || ctx.IsHcdeConnect?.Invoke(client) != true)
            return false;
        return ctx.GetStatus?.Invoke(client) != ctx.CstatReady;
    }

    public static bool IIsHcdeLiveRoutablePeerNative(HcdeLiveRouteContext ctx, int client)
    {
        if (!ClientSlotInRange(client, ctx.MaxClients) || client == ctx.ConsolePlayer
            || !IClientUsesHcdeServiceNative(ctx.Slots, client))
            return false;
        if (IIsHcdeClientSetupInProgressNative(ctx.Setup, client))
            return false;
        return true;
    }

    public static bool IShouldSendHcdeLiveControlToNative(HcdeLiveRouteContext ctx, int client) =>
        IIsHcdeLiveRoutablePeerNative(ctx, client)
        && (IIsLocalHcdeLiveAuthorityNative(ctx.Authority)
            ? !IIsHcdeLiveAuthoritySlotNative(ctx.Authority, client)
            : IIsHcdeLiveAuthoritySlotNative(ctx.Authority, client));

    public static bool IShouldSendHcdeLiveClientInputToNative(HcdeLiveRouteContext ctx, int client) =>
        IIsHcdeLiveRoutablePeerNative(ctx, client)
        && !IIsLocalHcdeLiveAuthorityNative(ctx.Authority)
        && IIsHcdeLiveAuthoritySlotNative(ctx.Authority, client);

    public static bool IShouldSendHcdeLiveServerSnapshotToNative(HcdeLiveRouteContext ctx, int client) =>
        IIsHcdeLiveRoutablePeerNative(ctx, client)
        && IIsLocalHcdeLiveAuthorityNative(ctx.Authority)
        && !IIsHcdeLiveAuthoritySlotNative(ctx.Authority, client);

    public static bool IShouldAcceptHcdeLiveClientInputFromNative(HcdeLiveRouteContext ctx, int client) =>
        IIsHcdeLiveRoutablePeerNative(ctx, client)
        && IIsLocalHcdeLiveAuthorityNative(ctx.Authority)
        && !IIsHcdeLiveAuthoritySlotNative(ctx.Authority, client);

    public static bool IShouldAcceptHcdeLiveServerSnapshotFromNative(HcdeLiveRouteContext ctx, int client) =>
        IIsHcdeLiveRoutablePeerNative(ctx, client)
        && !IIsLocalHcdeLiveAuthorityNative(ctx.Authority)
        && IIsHcdeLiveAuthoritySlotNative(ctx.Authority, client);

    private static int IGetReservedServerSlotNative(NetClientSlotContext ctx) =>
        VmINetInitNetwork.IGetReservedServerSlotNative(ctx.Reserved);
}
