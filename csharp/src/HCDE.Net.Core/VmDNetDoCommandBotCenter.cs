namespace HCDE.Net.Core;

/// <summary><c>Net_DoCommand</c> bot and centerview cases (<c>d_net.cpp</c> 8743–8754).</summary>
public static class VmDNetDoCommandBotCenter
{
    public static void ApplyAddBotNative(object? level, object? stream, int player, Action<object?, object?, int>? tryAddBot)
    {
        if (level is null)
            return;
        tryAddBot?.Invoke(level, stream, player);
    }

    public static void ApplyKillBotsNative(object? level, Action<object?, bool>? removeAllBots, Action<string>? printRemoved)
    {
        if (level is null)
            return;
        removeAllBots?.Invoke(level, true);
        printRemoved?.Invoke("Removed all bots\n");
    }

    public static void ApplyCenterViewNative(int player, Action<int, bool>? setCentering) =>
        setCentering?.Invoke(player, true);
}
