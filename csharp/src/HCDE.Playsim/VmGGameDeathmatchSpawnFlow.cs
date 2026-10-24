namespace HCDE.Playsim;

/// <summary><c>DeathMatchSpawnPlayer</c> from <c>g_game.cpp</c> (2181–2246).</summary>
public static class VmGGameDeathmatchSpawnFlow
{
    public const int DfSpawnFarthest = 1 << 0;

    public static bool ShouldFallbackToPlayerStart(int dmStartCount) => dmStartCount < 1;

    public static bool AnyLivingPlayerSpawned(IEnumerable<bool> inGameWithLivingMob) =>
        inGameWithLivingMob.Any(x => x);

    public static bool ShouldUseFarthestSpot(int dmFlags, bool anyLiving) =>
        (dmFlags & DfSpawnFarthest) != 0 && anyLiving;

    public static bool ShouldRetryRandomWhenSpotNull(bool spotNull) => spotNull;

    public static bool ShouldUsePlayer1StartWhenCheckFails(bool checkSpot, int playerStartType) =>
        !checkSpot && playerStartType != 0;

    public static bool ShouldForceDeathmatchStartWhenPlayer1Missing(int playerStartType) =>
        playerStartType == 0;

    public static int SelectSpotStrategy(int dmFlags, bool anyLiving, bool useFarthest) =>
        useFarthest ? 1 : 0;

    public static bool ShouldPickPlayerStartFallback(bool spotNull, bool checkSpotFailed) =>
        spotNull || checkSpotFailed;

    public static bool ShouldUseDeathmatchStartZero(bool player1TypeZero) => player1TypeZero;

    public static bool PlayerInGameWithMob(bool inGame, bool hasMob, int health) =>
        inGame && hasMob && health > 0;
}

