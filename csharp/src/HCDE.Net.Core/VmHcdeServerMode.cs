namespace HCDE.Net.Core;

/// <summary>HCDE dedicated-server runtime boundary (<c>hcde_servermode.cpp</c>).</summary>
public static class VmHcdeServerMode
{
    public static bool IsValueArgNative(HcdeServerModeRuntimeContext ctx, int index) =>
        ctx.NumArgs?.Invoke() > index && index > 0
        && ctx.GetArg?.Invoke(index) is { Length: > 0 } text
        && text[0] != '-' && text[0] != '+';

    public static int ReadIntValueNative(HcdeServerModeRuntimeContext ctx, string flag)
    {
        var index = ctx.CheckParm?.Invoke(flag) ?? 0;
        if (!IsValueArgNative(ctx, index + 1))
            return 0;
        var text = ctx.GetArg?.Invoke(index + 1) ?? "";
        return ctx.TryParseStrictInt?.Invoke(text) ?? 0;
    }

    public static string AssignArgValueNative(HcdeServerModeRuntimeContext ctx, string flag) =>
        ctx.CheckValue?.Invoke(flag) ?? "";

    public static string ReadWarpValueNative(HcdeServerModeRuntimeContext ctx)
    {
        var index = ctx.CheckParm?.Invoke("-warp") ?? 0;
        if (!IsValueArgNative(ctx, index + 1))
            return "";
        var text = ctx.GetArg?.Invoke(index + 1) ?? "";
        if (IsValueArgNative(ctx, index + 2))
            text += " " + ctx.GetArg?.Invoke(index + 2);
        return text;
    }

    public static void HcdeServerModeInitFromArgsNative(HcdeServerModeStateContext ctx)
    {
        if (ctx.Initialized)
            return;
        ctx.SetDedicatedExecutable?.Invoke(ctx.BuildDedicatedExecutable?.Invoke() == true);
        if (ctx.NumArgs?.Invoke() is not > 0)
            return;
        ctx.SetInitialized?.Invoke(true);
        ctx.SetDedicatedServer?.Invoke(ctx.CheckParm?.Invoke("-server") != 0);
        ctx.SetHostMode?.Invoke(ctx.CheckParm?.Invoke("-host") != 0);
        ctx.SetJoinMode?.Invoke(ctx.CheckParm?.Invoke("-join") != 0);
        ctx.SetDedicatedJoin?.Invoke(
            ctx.CheckParm?.Invoke("-dedicatedjoin") != 0 || ctx.CheckParm?.Invoke("-joindedicated") != 0);
        ctx.SetNetWaitSilent?.Invoke(ctx.CheckParm?.Invoke("-netwaitsilent") != 0);
        ctx.SetRequestedServerSlots?.Invoke(ReadIntValueNative(ctx.Runtime, "-server"));
        ctx.SetGamePort?.Invoke(ReadIntValueNative(ctx.Runtime, "-port"));
        var iwad = AssignArgValueNative(ctx.Runtime, "-iwad");
        ctx.SetIwadArg?.Invoke(iwad);
        var map = AssignArgValueNative(ctx.Runtime, "-map");
        if (string.IsNullOrEmpty(map))
            map = ReadWarpValueNative(ctx.Runtime);
        ctx.SetMapArg?.Invoke(map);
        var join = AssignArgValueNative(ctx.Runtime, "-join");
        if (string.IsNullOrEmpty(join))
            join = AssignArgValueNative(ctx.Runtime, "-joindedicated");
        if (string.IsNullOrEmpty(join))
            join = AssignArgValueNative(ctx.Runtime, "-dedicatedjoin");
        ctx.SetJoinAddress?.Invoke(join);
        ctx.SetFlowState?.Invoke("startup");
        ctx.LogInit?.Invoke();
        if (ctx.DedicatedExecutable && !ctx.DedicatedServer)
            ctx.FatalError?.Invoke("hcdeserv was started without -server <slots>. Dedicated server builds must enter the explicit server runtime.");
    }

    public static void HcdeServerModeSetSelectedIwadNative(HcdeServerModeStateContext ctx, string? iwad)
    {
        HcdeServerModeInitFromArgsNative(ctx);
        ctx.SetSelectedIwad?.Invoke(iwad ?? "");
    }

    public static void HcdeServerModeSetNetworkDetailsNative(HcdeServerModeStateContext ctx, int visible, int internalSlots, int port, bool reserved, string? flow)
    {
        HcdeServerModeInitFromArgsNative(ctx);
        ctx.SetVisibleSlots?.Invoke(visible);
        ctx.SetInternalSlots?.Invoke(internalSlots);
        ctx.SetGamePort?.Invoke(port);
        ctx.SetReservedServerSlot?.Invoke(reserved);
        ctx.SetFlowState?.Invoke(flow ?? "");
        ctx.LogNetwork?.Invoke((visible, internalSlots, port, reserved));
    }

    public static void HcdeServerModeSetMasterStateNative(HcdeServerModeStateContext ctx, bool advertising, int configuredMasters)
    {
        HcdeServerModeInitFromArgsNative(ctx);
        ctx.SetMasterStateKnown?.Invoke(true);
        ctx.SetMasterAdvertising?.Invoke(advertising);
        ctx.SetConfiguredMasters?.Invoke(configuredMasters);
        ctx.LogMaster?.Invoke(advertising, configuredMasters);
    }

    public static void HcdeServerModeSetAuthorityStateNative(HcdeServerModeStateContext ctx, int slot, bool playerBacked, bool settingsController, bool waiting, string? name)
    {
        HcdeServerModeInitFromArgsNative(ctx);
        ctx.SetAuthorityStateKnown?.Invoke(true);
        ctx.SetAuthoritySlot?.Invoke(slot);
        ctx.SetAuthorityPlayerBacked?.Invoke(playerBacked);
        ctx.SetAuthoritySettingsController?.Invoke(settingsController);
        ctx.SetAuthorityWaiting?.Invoke(waiting);
        ctx.SetAuthorityName?.Invoke(name ?? "");
        ctx.LogAuthority?.Invoke(slot, playerBacked, settingsController, waiting);
    }

}

public sealed class HcdeServerModeRuntimeContext
{
    public Func<int>? NumArgs { get; init; }
    public Func<int, string?>? GetArg { get; init; }
    public Func<string, int>? CheckParm { get; init; }
    public Func<string, string?>? CheckValue { get; init; }
    public Func<string, int>? TryParseStrictInt { get; init; }
}

public sealed class HcdeServerModeStateContext
{
    public HcdeServerModeRuntimeContext Runtime { get; init; } = new();
    public bool Initialized { get; set; }
    public bool DedicatedExecutable { get; set; }
    public bool DedicatedServer { get; set; }
    public bool HostMode { get; set; }
    public bool JoinMode { get; set; }
    public bool DedicatedJoin { get; set; }
    public bool NetWaitSilent { get; set; }
    public bool AuthorityStateKnown { get; set; }
    public bool AuthorityPlayerBacked { get; set; } = true;
    public bool AuthoritySettingsController { get; set; }
    public bool AuthorityWaiting { get; set; }
    public int AuthoritySlot { get; set; }
    public string AuthorityName { get; set; } = "";
    public Action<bool>? SetDedicatedExecutable { get; init; }
    public Func<bool>? BuildDedicatedExecutable { get; init; }
    public Action<bool>? SetInitialized { get; init; }
    public Action<bool>? SetDedicatedServer { get; init; }
    public Func<string, int>? CheckParm { get; init; }
    public Action<bool>? SetHostMode { get; init; }
    public Action<bool>? SetJoinMode { get; init; }
    public Action<bool>? SetDedicatedJoin { get; init; }
    public Action<bool>? SetNetWaitSilent { get; init; }
    public Action<int>? SetRequestedServerSlots { get; init; }
    public Action<int>? SetGamePort { get; init; }
    public Action<string>? SetIwadArg { get; init; }
    public Action<string>? SetMapArg { get; init; }
    public Action<string>? SetJoinAddress { get; init; }
    public Action<string>? SetFlowState { get; init; }
    public Action<string>? SetSelectedIwad { get; init; }
    public Action? LogInit { get; init; }
    public Action<string>? FatalError { get; init; }
    public Func<int>? NumArgs => Runtime.NumArgs;
    public Action<int>? SetVisibleSlots { get; init; }
    public Action<int>? SetInternalSlots { get; init; }
    public Action<bool>? SetReservedServerSlot { get; init; }
    public Action<(int Visible, int Internal, int Port, bool Reserved)>? LogNetwork { get; init; }
    public Action<bool>? SetMasterStateKnown { get; init; }
    public Action<bool>? SetMasterAdvertising { get; init; }
    public Action<int>? SetConfiguredMasters { get; init; }
    public Action<bool, int>? LogMaster { get; init; }
    public Action<bool>? SetAuthorityStateKnown { get; init; }
    public Action<int>? SetAuthoritySlot { get; init; }
    public Action<bool>? SetAuthorityPlayerBacked { get; init; }
    public Action<bool>? SetAuthoritySettingsController { get; init; }
    public Action<bool>? SetAuthorityWaiting { get; init; }
    public Action<string>? SetAuthorityName { get; init; }
    public Action<int, bool, bool, bool>? LogAuthority { get; init; }
    public Action<string>? DebugMark { get; init; }
    public Action<string>? PrintDiagnosticsBlock { get; init; }
}
