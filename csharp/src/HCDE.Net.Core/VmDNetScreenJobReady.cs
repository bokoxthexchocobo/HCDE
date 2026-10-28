namespace HCDE.Net.Core;

/// <summary>Intermission lobby / screen-job ready helpers (<c>d_net.cpp</c> 10103–10167).</summary>
public static class VmDNetScreenJobReady
{
    public static bool IsPlayerReadyParticipantNative(bool cutsceneReadyParticipant, bool hasBot) =>
        cutsceneReadyParticipant && !hasBot;

    public static bool ShouldApplyLocalReadyNative(bool netgame, bool demoplayback) =>
        netgame && !demoplayback;

    public static bool ConsolePlayerInRangeNative(int consolePlayer, int maxPlayers) =>
        consolePlayer >= 0 && consolePlayer < maxPlayers;

    public static bool ShouldWriteReadiedCommandNative(bool netgame, bool demoplayback) =>
        netgame && !demoplayback;

    public static bool ShouldApplyLocalReadiedUpNative(
        bool netgame,
        bool demoplayback,
        int consolePlayer,
        int maxPlayers) =>
        ShouldApplyLocalReadyNative(netgame, demoplayback)
        && ConsolePlayerInRangeNative(consolePlayer, maxPlayers);

    public static bool ShouldInvokeReadiedUpNative(
        bool netgame,
        bool demoplayback,
        int consolePlayer,
        int maxPlayers) =>
        ShouldApplyLocalReadiedUpNative(netgame, demoplayback, consolePlayer, maxPlayers);
}
