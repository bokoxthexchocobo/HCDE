namespace HCDE.Playsim;

/// <summary><c>G_SpawnPlayer</c> title-level tail from <c>g_game.cpp</c> (2012–2031).</summary>
public static class VmGGameSpawnPlayerTail
{
    public const int GsTitleLevel = 3;

    public static bool ShouldGiveDefaultInventory(int gameState) => gameState != GsTitleLevel;

    public static bool ShouldReinitBot(object? bot, bool hadBot) => hadBot && bot != null;

    public static (int ready, int pending) SyncReadyWeapon(int pendingWeapon) => (pendingWeapon, pendingWeapon);
}
