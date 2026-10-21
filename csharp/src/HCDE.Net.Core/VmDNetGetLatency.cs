namespace HCDE.Net.Core;

/// <summary><c>Net_GetLatency</c> severity buckets (<c>d_net.cpp</c> 9679–9693).</summary>
public static class VmDNetGetLatency
{
    public static int GameDelayMsNative(int clientTic, int gameTic, int ticRate) =>
        (clientTic - gameTic) * 1000 / Math.Max(ticRate, 1);

    public static int LatencySeverityNative(int gameDelayMs)
    {
        if (gameDelayMs >= 160)
            return 3;
        if (gameDelayMs >= 120)
            return 2;
        if (gameDelayMs >= 80)
            return 1;
        return 0;
    }

    public static bool ConsolePlayerInRangeNative(int consolePlayer, int maxPlayers) =>
        consolePlayer >= 0 && consolePlayer < maxPlayers;

    public static int ArbitratorDelayNative(int consolePlayer, int maxPlayers, int averageLatency) =>
        ConsolePlayerInRangeNative(consolePlayer, maxPlayers) ? averageLatency : 0;

    public static int ClassicInvasionStateNative(int invasionState) =>
        invasionState switch
        {
            1 => 5,
            2 or 3 => 6,
            4 => 7,
            5 => 8,
            6 => 9,
            _ => 0,
        };
}
