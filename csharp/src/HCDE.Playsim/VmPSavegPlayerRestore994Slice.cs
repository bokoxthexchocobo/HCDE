namespace HCDE.Playsim;

/// <summary>
/// `ReadMultiplePlayers` tail, `CopyPlayer`, and `SpawnExtraPlayers` from <c>p_saveg.cpp</c> (**751–900**).
/// </summary>
public static class VmPSavegPlayerRestore994Slice
{
    public const int BotInUseYes = 1;

    public static bool TryAssignNamedTempPlayer(
        bool fromHub,
        bool tempAlreadyUsed,
        bool namesMatch,
        Action copyFullPlayer,
        Action assignMobjOnly)
    {
        if (tempAlreadyUsed || !namesMatch)
            return false;

        if (fromHub)
            assignMobjOnly();
        else
            copyFullPlayer();

        return true;
    }

    public static bool TryAssignFirstUnusedTempPlayer(
        bool fromHub,
        bool tempAlreadyUsed,
        Action copyFullPlayer,
        Action assignMobjOnly)
    {
        if (tempAlreadyUsed)
            return false;

        if (fromHub)
            assignMobjOnly();
        else
            copyFullPlayer();

        return true;
    }

    public static bool ShouldDestroyUnusedTempActor(bool tempUsed) => !tempUsed;

    public static int MergeChaseCamCheat(int cheats, int chaseCamFlag) =>
        cheats | chaseCamFlag;

    public enum UserinfoRestoreKind
    {
        BotFromSave,
        HumanPreserveSaveClass,
    }

    public static UserinfoRestoreKind ResolveUserinfoRestoreAfterCopy(
        bool isBot,
        bool botMatched,
        ref int botCount)
    {
        if (isBot)
        {
            ApplyBotRestoreCounters(ref botCount, botMatched);
            return UserinfoRestoreKind.BotFromSave;
        }

        return UserinfoRestoreKind.HumanPreserveSaveClass;
    }

    public static bool TryMarkMatchingBotInUse(
        IReadOnlyList<string> botNames,
        string savedName,
        int startIndex,
        out int matchedIndex)
    {
        for (var i = startIndex; i < botNames.Count; i++)
        {
            if (string.Equals(botNames[i], savedName, StringComparison.OrdinalIgnoreCase))
            {
                matchedIndex = i;
                return true;
            }
        }

        matchedIndex = -1;
        return false;
    }

    public static void ApplyBotRestoreCounters(ref int botCount, bool botMatched)
    {
        if (botMatched)
            botCount++;
    }

    public static void RelinkPlayerOwnedPsprites<T>(
        T? firstPsprite,
        object player,
        Action<T, object> setOwner,
        Func<T, T?> getNext)
        where T : class
    {
        var pspr = firstPsprite;
        while (pspr is not null)
        {
            setOwner(pspr, player);
            pspr = getNext(pspr);
        }
    }

    public static void RestoreStickyInputFlags(
        ref bool attackDown,
        ref bool useDown,
        bool savedAttackDown,
        bool savedUseDown)
    {
        attackDown = savedAttackDown;
        useDown = savedUseDown;
    }

    public static bool ShouldSpawnExtraPlayers(bool deathmatch, bool isPrimaryLevel) =>
        !deathmatch && isPrimaryLevel;

    public static bool ShouldSpawnMissingPlayerPawn(bool playerInGame, object? mobj) =>
        playerInGame && mobj is null;
}
