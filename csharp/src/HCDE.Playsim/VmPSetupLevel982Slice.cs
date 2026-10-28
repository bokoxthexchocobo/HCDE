namespace HCDE.Playsim;

/// <summary>
/// `P_SetupLevel` continuation from `p_setup.cpp` (**451–600**).
/// </summary>
public static class VmPSetupLevel982Slice
{
    public const int Level2RandomPlayerStarts = 1 << 1;
    public const int Level2Preraiseweapon = 1 << 2;
    public const int MfFriendly = 1 << 29;
    public const int SecfEndgodmode = 1 << 0;
    public const int SecfEndlevel = 1 << 1;
    public const int Df2Nocountendmonst = 1 << 0;
    public const int SpfWeaponfullyup = 1;

    public static bool ConsolePlayerResetsViewHeight(bool hasConsolePlayer, bool beforeSoundReset) =>
        hasConsolePlayer && beforeSoundReset;

    public static bool ShouldStartHubMusic(bool primaryLevelReentering) => !primaryLevelReentering;

    public static bool MapOpenSucceeded(object? mapData, bool openAttempted) =>
        openAttempted && mapData is not null;

    public static bool ShouldFireNewGameEvent(bool newGame, bool mapLoaded) =>
        newGame && mapLoaded;

    public static bool DeathmatchSpawnPassActive(bool deathmatch, bool playerInGame) =>
        deathmatch && playerInGame;

    public static bool RandomPlayerStartPassActive(int levelFlags2, bool playerInGame) =>
        playerInGame && (levelFlags2 & Level2RandomPlayerStarts) != 0;

    public static bool NewGameSpawnFillNeeded(
        bool newGame,
        bool playerInGame,
        int playerStateEnter,
        bool mobMissing) =>
        newGame && playerInGame && playerStateEnter != 0 && mobMissing;

    public static int SelectSpawnWeaponFlags(int levelFlags2) =>
        (levelFlags2 & Level2Preraiseweapon) != 0 ? SpfWeaponfullyup : 0;

    public static bool UnfriendlyDeathmatchRelocationEnabled(int deathmatchStartsCount, bool playerInGame, bool mobPresent) =>
        deathmatchStartsCount > 0 && playerInGame && mobPresent;

    public static bool ShouldDestroyPreviousSpawn(bool replacementSucceeded, object? newMob, object? oldMob) =>
        replacementSucceeded && newMob is not null && oldMob is not null && !ReferenceEquals(newMob, oldMob);

    public static bool SectorIsEndLevelDamageZone(int sectorFlags, int damageAmount) =>
        damageAmount > 0
        && (sectorFlags & (SecfEndgodmode | SecfEndlevel)) == (SecfEndgodmode | SecfEndlevel);

    public static bool ShouldClearEndLevelMonsterCounters(
        int dmflags2,
        bool actorCountsAsKill,
        int sectorFlags,
        int sectorDamage) =>
        (dmflags2 & Df2Nocountendmonst) != 0
        && actorCountsAsKill
        && SectorIsEndLevelDamageZone(sectorFlags, sectorDamage);

    public static bool ShouldRunPrecachePass(bool precacheEnabled, bool levelLoadFinished) =>
        precacheEnabled && levelLoadFinished;

    public static bool TryClearMobBeforeSpawn(bool clearMob, Action clearAction, out bool cleared)
    {
        cleared = false;
        if (!clearMob)
            return false;
        clearAction();
        cleared = true;
        return true;
    }

    public static bool DeathmatchAnnounceFollowsPrecache(bool deathmatch, bool precacheFinished) =>
        deathmatch && precacheFinished;
}
