namespace HCDE.Net.Core;

/// <summary>Intermission lobby / screen-job ready helpers (<c>d_net.cpp</c>).</summary>
public static class VmDNetScreenJobReady
{
    public static bool IsPlayerReadyNative(int player, bool ready) => ready;

    public static bool IsPlayerReadyParticipantNative(bool cutsceneReady, bool hasBot) =>
        cutsceneReady && !hasBot;

    public static bool ShouldApplyLocalReadyNative(bool netgame, bool demoplayback) =>
        netgame && !demoplayback;

    public static bool ConsolePlayerInRangeNative(int consolePlayer, int maxPlayers) =>
        consolePlayer >= 0 && consolePlayer < maxPlayers;

    public static bool ShouldWriteReadiedCommandNative(bool netgame, bool demoplayback) =>
        netgame && !demoplayback;

    public static bool InvasionControlRejectedNative(int controlResult) => controlResult == 0;
}
