namespace HCDE.Net.Core;

/// <summary>HCDE server mode queries and UI suppression (<c>hcde_servermode.cpp</c> 219–395).</summary>
public static class VmHcdeServerModeAccessors
{
    public static void HcdeServerModeSetAuthoritySettingsControllerNative(HcdeServerModeStateContext ctx, bool enabled)
    {
        VmHcdeServerMode.HcdeServerModeInitFromArgsNative(ctx);
        ctx.SetAuthorityStateKnown?.Invoke(true);
        ctx.SetAuthoritySettingsController?.Invoke(enabled);
        ctx.DebugMark?.Invoke($"authority settings-controller={(enabled ? 1 : 0)}");
    }

    public static void HcdeServerModeSetAuthorityWaitingNative(HcdeServerModeStateContext ctx, bool waiting)
    {
        VmHcdeServerMode.HcdeServerModeInitFromArgsNative(ctx);
        ctx.SetAuthorityStateKnown?.Invoke(true);
        ctx.SetAuthorityWaiting?.Invoke(waiting);
        ctx.DebugMark?.Invoke($"authority waiting={(waiting ? 1 : 0)}");
    }

    public static void HcdeServerModePrintDiagnosticsNative(HcdeServerModeStateContext ctx, string? phase)
    {
        VmHcdeServerMode.HcdeServerModeInitFromArgsNative(ctx);
        if (!ctx.DedicatedServer && !ctx.HostMode && !ctx.JoinMode && !ctx.DedicatedJoin)
            return;
        ctx.PrintDiagnosticsBlock?.Invoke(phase ?? "unknown");
    }

    public static void HcdeServerModeGuardClientSubsystemNative(HcdeServerModeStateContext ctx, string? subsystem)
    {
        VmHcdeServerMode.HcdeServerModeInitFromArgsNative(ctx);
        if (!ctx.DedicatedServer)
            return;
        ctx.FatalError?.Invoke($"Dedicated server attempted to initialize client subsystem: {subsystem ?? "unknown"}. This is a server-boundary bug.");
    }

    public static bool HcdeServerModeIsDedicatedServerNative(HcdeServerModeStateContext ctx)
    {
        VmHcdeServerMode.HcdeServerModeInitFromArgsNative(ctx);
        return ctx.DedicatedServer;
    }

    public static bool HcdeServerModeIsDedicatedExecutableNative(HcdeServerModeStateContext ctx)
    {
        VmHcdeServerMode.HcdeServerModeInitFromArgsNative(ctx);
        return ctx.DedicatedExecutable;
    }

    public static bool HcdeServerModeIsDedicatedJoinNative(HcdeServerModeStateContext ctx)
    {
        VmHcdeServerMode.HcdeServerModeInitFromArgsNative(ctx);
        return ctx.DedicatedJoin;
    }

    public static bool HcdeServerModeHasAuthorityStateNative(HcdeServerModeStateContext ctx)
    {
        VmHcdeServerMode.HcdeServerModeInitFromArgsNative(ctx);
        return ctx.AuthorityStateKnown;
    }

    public static bool HcdeServerModeIsAuthorityPlayerBackedNative(HcdeServerModeStateContext ctx)
    {
        VmHcdeServerMode.HcdeServerModeInitFromArgsNative(ctx);
        return ctx.AuthorityPlayerBacked;
    }

    public static bool HcdeServerModeAuthorityCanControlSettingsNative(HcdeServerModeStateContext ctx)
    {
        VmHcdeServerMode.HcdeServerModeInitFromArgsNative(ctx);
        return ctx.AuthoritySettingsController;
    }

    public static bool HcdeServerModeIsAuthorityWaitingNative(HcdeServerModeStateContext ctx)
    {
        VmHcdeServerMode.HcdeServerModeInitFromArgsNative(ctx);
        return ctx.AuthorityWaiting;
    }

    public static int HcdeServerModeGetAuthoritySlotNative(HcdeServerModeStateContext ctx)
    {
        VmHcdeServerMode.HcdeServerModeInitFromArgsNative(ctx);
        return ctx.AuthoritySlot;
    }

    public static string HcdeServerModeGetAuthorityNameNative(HcdeServerModeStateContext ctx)
    {
        VmHcdeServerMode.HcdeServerModeInitFromArgsNative(ctx);
        return string.IsNullOrEmpty(ctx.AuthorityName) ? "HCDE server" : ctx.AuthorityName;
    }

    public static bool HcdeServerModeShouldSuppressRoomUiNative(HcdeServerModeStateContext ctx)
    {
        VmHcdeServerMode.HcdeServerModeInitFromArgsNative(ctx);
        return ctx.DedicatedServer || ctx.NetWaitSilent;
    }

    public static bool HcdeServerModeShouldSuppressStartupUiNative(HcdeServerModeStateContext ctx)
    {
        VmHcdeServerMode.HcdeServerModeInitFromArgsNative(ctx);
        return ctx.DedicatedServer;
    }
}
