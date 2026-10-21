namespace HCDE.Net.Core;

/// <summary>Network init and dedicated slot helpers (<c>i_net.cpp</c> 4266–4415).</summary>
public static class VmINetInitNetwork
{
    public static bool IInitNetworkNative(IInitNetworkContext ctx)
    {
        ctx.InitServerModeFromArgs?.Invoke();
        ctx.SetDedicatedServerMode?.Invoke(ctx.IsDedicatedServer?.Invoke() == true);
        ctx.SetDedicatedJoinMode?.Invoke(ctx.IsDedicatedJoin?.Invoke() == true);
        ctx.ResetGuestSetupProgress?.Invoke();
        ctx.SetSilentNetStart?.Invoke(ctx.ShouldSuppressRoomUi?.Invoke() == true);
        var dupParsed = ctx.TryParseDup?.Invoke(ctx.GetDupArg?.Invoke() ?? "");
        if (dupParsed is { Ok: true })
            ctx.SetTicDup?.Invoke(Math.Clamp(dupParsed.Value.Value, 1, ctx.MaxTicDup));
        else if (dupParsed is { Ok: false, HadValue: true })
            ctx.DebugMark?.Invoke($"invalid -dup value '{ctx.GetDupArg?.Invoke()}', keeping default");
        var portParsed = ctx.TryParsePort?.Invoke(ctx.GetPortArg?.Invoke() ?? "");
        if (portParsed is { Ok: true })
        {
            ctx.SetGamePort?.Invoke(portParsed.Value.Value);
            ctx.Printf?.Invoke($"Using alternate port {portParsed.Value.Value}");
        }
        else if (portParsed is { Ok: false, HadValue: true })
            ctx.DebugMark?.Invoke($"invalid -port value '{ctx.GetPortArg?.Invoke()}', keeping default {ctx.DefaultGamePort}");
        ctx.SetNetPassword?.Invoke(ctx.GetPasswordArg?.Invoke());
        if (ctx.DedicatedServerMode && ctx.FindServerArg?.Invoke() is { } server)
        {
            if (server.RunGame?.Invoke(server.ArgIndex) != true)
                return false;
        }
        else if (ctx.FindHostArg?.Invoke() is { } host)
        {
            if (host.RunGame?.Invoke(host.ArgIndex) != true)
                return false;
        }
        else if (ctx.FindJoinArg?.Invoke() is { } join)
        {
            if (join.RunGame?.Invoke(join.ArgIndex) != true)
                return false;
        }
        else if (ctx.FindJoinDedicatedArg?.Invoke() is { } joinDedicated)
        {
            ctx.SetDedicatedJoinMode?.Invoke(true);
            if (joinDedicated.RunGame?.Invoke(joinDedicated.ArgIndex) != true)
                return false;
        }
        else if (ctx.FindDedicatedJoinArg?.Invoke() is { } dedicatedJoin)
        {
            ctx.SetDedicatedJoinMode?.Invoke(true);
            if (dedicatedJoin.RunGame?.Invoke(dedicatedJoin.ArgIndex) != true)
                return false;
        }
        else
        {
            ctx.GenerateGameId?.Invoke();
            ctx.SetTicDup?.Invoke(1);
            ctx.AddNetworkClient?.Invoke(0);
            ctx.SetHostReady?.Invoke(0);
            ctx.SetupUserInfo?.Invoke();
        }
        ctx.SetGameStarted?.Invoke(true);
        return true;
    }

    public static bool IIsDedicatedServerModeNative(bool dedicatedServerMode) => dedicatedServerMode;

    public static void IDedicatedServerRequestStartNative(DedicatedServerRequestContext ctx)
    {
        if (!ctx.DedicatedServerMode)
        {
            ctx.Printf?.Invoke("NetServer:: Start request ignored because this is not a dedicated server.");
            return;
        }
        ctx.SetDedicatedServerStartRequested?.Invoke(true);
    }

    public static void IDedicatedServerRequestAbortNative(DedicatedServerRequestContext ctx)
    {
        if (!ctx.DedicatedServerMode)
        {
            ctx.SetDedicatedServerAbortRequested?.Invoke(true);
            ctx.Printf?.Invoke("NetServer:: Stop request queued while dedicated server mode initializes.");
            return;
        }
        ctx.SetDedicatedServerAbortRequested?.Invoke(true);
    }

    public static bool IUsesDedicatedServerSlotNative(bool dedicatedServerMode, bool dedicatedJoinMode) =>
        dedicatedServerMode || dedicatedJoinMode;

    public static int IGetReservedServerSlotNative(ReservedServerSlotContext ctx)
    {
        var authoritySlot = ctx.GetHcdeServiceAuthoritySlot?.Invoke() ?? -1;
        if (authoritySlot < 0)
            return -1;
        if (ctx.UsesDedicatedServerSlot?.Invoke() == true)
            return authoritySlot;
        if (ctx.IsRemoteHcdeServiceAuthority?.Invoke(authoritySlot) == true)
            return authoritySlot;
        return -1;
    }
}

public sealed class IInitNetworkContext
{
    public bool DedicatedServerMode { get; set; }
    public int MaxTicDup { get; init; }
    public uint DefaultGamePort { get; init; }
    public Action? InitServerModeFromArgs { get; init; }
    public Func<bool>? IsDedicatedServer { get; init; }
    public Func<bool>? IsDedicatedJoin { get; init; }
    public Action<bool>? SetDedicatedServerMode { get; init; }
    public Action<bool>? SetDedicatedJoinMode { get; init; }
    public Action? ResetGuestSetupProgress { get; init; }
    public Func<bool>? ShouldSuppressRoomUi { get; init; }
    public Action<bool>? SetSilentNetStart { get; init; }
    public Func<string?>? GetDupArg { get; init; }
    public Func<string, (bool Ok, bool HadValue, int Value)>? TryParseDup { get; init; }
    public Action<int>? SetTicDup { get; init; }
    public Func<string?>? GetPortArg { get; init; }
    public Func<string, (bool Ok, bool HadValue, int Value)>? TryParsePort { get; init; }
    public Action<int>? SetGamePort { get; init; }
    public Func<string?>? GetPasswordArg { get; init; }
    public Action<string?>? SetNetPassword { get; init; }
    public Func<(int ArgIndex, Func<int, bool>? RunGame)?>? FindServerArg { get; init; }
    public Func<(int ArgIndex, Func<int, bool>? RunGame)?>? FindHostArg { get; init; }
    public Func<(int ArgIndex, Func<int, bool>? RunGame)?>? FindJoinArg { get; init; }
    public Func<(int ArgIndex, Func<int, bool>? RunGame)?>? FindJoinDedicatedArg { get; init; }
    public Func<(int ArgIndex, Func<int, bool>? RunGame)?>? FindDedicatedJoinArg { get; init; }
    public Action? GenerateGameId { get; init; }
    public Action<int>? AddNetworkClient { get; init; }
    public Action<int>? SetHostReady { get; init; }
    public Action? SetupUserInfo { get; init; }
    public Action<bool>? SetGameStarted { get; init; }
    public Action<string>? DebugMark { get; init; }
    public Action<string>? Printf { get; init; }
}

public sealed class DedicatedServerRequestContext
{
    public bool DedicatedServerMode { get; init; }
    public Action<string>? Printf { get; init; }
    public Action<bool>? SetDedicatedServerStartRequested { get; init; }
    public Action<bool>? SetDedicatedServerAbortRequested { get; init; }
}

public sealed class ReservedServerSlotContext
{
    public Func<int>? GetHcdeServiceAuthoritySlot { get; init; }
    public Func<bool>? UsesDedicatedServerSlot { get; init; }
    public Func<int, bool>? IsRemoteHcdeServiceAuthority { get; init; }
}
