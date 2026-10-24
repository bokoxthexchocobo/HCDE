namespace HCDE.Client;

/// <summary><c>CUSTOM_CVAR(fraglimit)</c> from <c>d_main.cpp</c> (466–482).</summary>
public static class VmDMainFragLimitPolicy
{
    public const int MaxPlayers = 32;

    public static bool ShouldCheckFragLimit(bool deathmatch, int fragLimit) =>
        deathmatch && fragLimit > 0;

    public static bool ShouldExitLevel(int fragLimit, int playerFragCount) =>
        fragLimit > 0 && playerFragCount >= fragLimit;

    public static bool ShouldEvaluatePlayer(bool inGame, int fragLimit, int fragCount) =>
        inGame && ShouldExitLevel(fragLimit, fragCount);

    public static int? FindFirstViolatingPlayer(
        IReadOnlyList<bool> inGame,
        IReadOnlyList<int> fragCounts,
        int fragLimit)
    {
        if (!ShouldCheckFragLimit(true, fragLimit))
            return null;

        var count = Math.Min(inGame.Count, fragCounts.Count);
        for (var i = 0; i < count; i++)
        {
            if (ShouldEvaluatePlayer(inGame[i], fragLimit, fragCounts[i]))
                return i;
        }

        return null;
    }

    public static string FragLimitMessageKey => "TXT_FRAGLIMIT";
}
