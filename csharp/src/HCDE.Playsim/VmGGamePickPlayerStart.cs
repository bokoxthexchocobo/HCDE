namespace HCDE.Playsim;

/// <summary><c>PickPlayerStart</c> from <c>g_game.cpp</c> (2252–2284).</summary>
public static class VmGGamePickPlayerStart
{
    public const int PpsForceRandom = 1;
    public const int PpsNoBlockingCheck = 2;

    public static bool HasAnyStarts(int count) => count > 0;

    public static bool ShouldRandomize(int levelRandomStarts, int flags, int assignedStartType) =>
        levelRandomStarts != 0 || (flags & PpsForceRandom) != 0 || assignedStartType == 0;

    public static bool ShouldCollectOpenStarts(int flags) => (flags & PpsNoBlockingCheck) == 0;

    public static int PickOpenStartIndex(int openCount, Func<int, int> roll) =>
        openCount > 0 ? roll(openCount) : -1;

    public static int PickAnyStartIndex(int startCount, Func<int, int> roll) =>
        startCount > 0 ? roll(startCount) % startCount : 0;

    public static bool ShouldUseAssignedStart(bool randomize) => !randomize;

    public static void CollectOpenStarts<T>(
        IReadOnlyList<T> starts,
        int playernum,
        Func<T, bool> isOpen,
        IList<T> openStarts)
    {
        openStarts.Clear();
        for (var i = 0; i < starts.Count; i++)
        {
            if (isOpen(starts[i]))
                openStarts.Add(starts[i]);
        }
    }
}

