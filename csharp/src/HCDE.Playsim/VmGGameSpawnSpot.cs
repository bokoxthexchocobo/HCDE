namespace HCDE.Playsim;

/// <summary><c>CheckSpot</c> and deathmatch start selection from <c>g_game.cpp</c> (2040–2149).</summary>
public static class VmGGameSpawnSpot
{
    public const int MaxRandomTries = 20;

    public static bool IsInvalidStartType(int type) => type == 0;

    public static bool AnotherPlayerOccupiesSpot(
        int playernum,
        double spotX,
        double spotY,
        IReadOnlyList<(int Index, double X, double Y, bool HasActor)> players) =>
        players.Take(playernum).Any(p => p.HasActor && p.X == spotX && p.Y == spotY);

    public static double PlayersRangeFromSpot(
        double spotX,
        double spotY,
        IReadOnlyList<(bool InGame, bool HasActor, int Health, double X, double Y)> players)
    {
        var closest = double.MaxValue;
        foreach (var p in players)
        {
            if (!p.InGame || !p.HasActor || p.Health <= 0)
                continue;
            var dx = p.X - spotX;
            var dy = p.Y - spotY;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            if (distance < closest)
                closest = distance;
        }

        return closest;
    }

    public static int SelectFarthestIndex<T>(
        IReadOnlyList<T> starts,
        Func<T, double> rangeFromSpot)
    {
        var bestDistance = 0.0;
        var bestIndex = 0;
        for (var i = 0; i < starts.Count; i++)
        {
            var distance = rangeFromSpot(starts[i]);
            if (distance > bestDistance)
            {
                bestDistance = distance;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    public static int SelectRandomOpenIndex(
        int playernum,
        int selectionCount,
        Func<int> roll,
        Func<int, bool> isOpen,
        out int lastTried)
    {
        lastTried = 0;
        for (var j = 0; j < MaxRandomTries; j++)
        {
            var i = roll() % selectionCount;
            lastTried = i;
            if (isOpen(i))
                return i;
        }

        return lastTried;
    }

    public static bool ShouldGiveTitleInventory(int gameState, int gsTitleLevel) => gameState != gsTitleLevel;
}
