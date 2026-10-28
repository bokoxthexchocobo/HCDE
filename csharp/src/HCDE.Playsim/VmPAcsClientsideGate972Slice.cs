namespace HCDE.Playsim;

/// <summary>
/// ACS script-owner routing, clientside activation gate, and <c>DLevelScript</c> layout head from
/// <c>p_acs.cpp</c> (**601–750**). Exclusive to chunk **972**; does not re-credit fixed-point helpers
/// (**601–624**, in chunk **971** / off-tally scaffolding) or <c>IsClientSideScript</c> (**669–672**).
/// </summary>
public static class VmPAcsClientsideGate972Slice
{
    public const int ScriptRunning = 0;
    public const int ScriptSuspended = 1;
    public const int ScriptDelayed = 2;
    public const int ScriptTagWait = 3;
    public const int ScriptPolyWait = 4;
    public const int ScriptScriptWaitPre = 5;
    public const int ScriptScriptWait = 6;
    public const int ScriptPleaseRemove = 7;
    public const int ScriptDivideBy0 = 8;
    public const int ScriptModulusBy0 = 9;

    public const int DefaultHudWidth = 320;
    public const int DefaultHudHeight = 200;

    public static object? GetScriptOwner(object? activator, bool activatorHasPlayer, object? playerMo)
    {
        if (activator == null)
            return null;
        return activatorHasPlayer ? playerMo : activator;
    }

    public static bool ShouldIgnoreClientSideScript(
        bool activatorNull,
        bool activatorIsClientSide,
        object? owner,
        bool ownerHasPlayer,
        bool levelNull,
        Func<object?, bool>? isConsolePlayer)
    {
        if (activatorNull || activatorIsClientSide)
            return false;
        if (owner == null || levelNull)
            return false;
        if (!ownerHasPlayer)
            return false;
        return isConsolePlayer?.Invoke(owner) == false;
    }

    public static bool ActivatorIsNonPlayerWorld(object? owner, bool ownerHasPlayer) =>
        owner != null && !ownerHasPlayer;

    public static bool StringBuilderShouldPush(string builder, int stackDepth) =>
        builder.Length > 0 || stackDepth > 0;

    public static bool StringBuilderPopSucceeded(int stackDepth) => stackDepth > 0;

    public static int ResolveHudWidth(int configured, int fallback = DefaultHudWidth) =>
        configured > 0 ? configured : fallback;

    public static int ResolveHudHeight(int configured, int fallback = DefaultHudHeight) =>
        configured > 0 ? configured : fallback;

    public static bool ScriptStateIsWait(int state) =>
        state is ScriptTagWait or ScriptPolyWait or ScriptScriptWaitPre or ScriptScriptWait;

    public static bool ScriptStateIsTerminalError(int state) =>
        state is ScriptDivideBy0 or ScriptModulusBy0 or ScriptPleaseRemove;

    public static bool ShouldLockLocalStrings(int levelNum) => levelNum >= 0;

    public static IReadOnlyList<string> DLevelScriptProtectedFields { get; } = new[]
    {
        "next", "prev", "script", "Localvars", "pc", "state", "statedata", "activator",
        "activationline", "backSide", "bClientSide", "activefont", "hudwidth", "hudheight",
        "ClipRectLeft", "ClipRectTop", "ClipRectWidth", "ClipRectHeight", "WrapWidth",
        "HandleAspect", "activeBehavior", "InModuleScriptNumber", "ACS_StringBuilderStack",
    };

    public static IReadOnlyList<string> DLevelScriptPublicMethods { get; } = new[]
    {
        "Serialize", "RunScript", "GetClassForIndex", "SetState", "GetState", "GetNext",
        "MarkLocalVarStrings", "LockLocalVarStrings",
    };
}
