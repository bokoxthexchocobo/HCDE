namespace HCDE.Playsim;

/// <summary><c>player_t::HasWeaponsInSlot</c>, iteration helpers from <c>p_user.cpp</c> (1148–1243).</summary>
public static class VmPUserWeaponSlot
{
    public const int MaxPlayers = 64;

    public static bool WeaponClassPresent(object? weaponClass, bool hasInventory) =>
        weaponClass != null && hasInventory;

    public static bool ScanSlotForWeapon(int slotSize, Func<int, object?> getWeapon, Func<object?, bool> hasInventory)
    {
        for (var i = 0; i < slotSize; i++)
        {
            var weap = getWeapon(i);
            if (WeaponClassPresent(weap, hasInventory(weap))) return true;
        }
        return false;
    }

    public static bool HasWeaponsInSlot(int slotSize, Func<int, object?> getWeapon, Func<object?, bool> hasInventory) =>
        ScanSlotForWeapon(slotSize, getWeapon, hasInventory);

    public static int NextPlayerIndexStart(int pNum) => Math.Max(pNum + 1, 0);

    public static bool PlayerSlotActive(bool inGame, bool allowBot, bool isBot) =>
        inGame && (allowBot || !isBot);

    public static int GetNextPlayerNumber(int pNum, bool noBots, int maxPlayers, Func<int, (bool InGame, bool IsBot)> slot)
    {
        var start = NextPlayerIndexStart(pNum);
        for (var i = start; i < maxPlayers; i++)
        {
            var (inGame, isBot) = slot(i);
            if (PlayerSlotActive(inGame, !noBots, isBot)) return i;
        }
        return -1;
    }

    public static int? GetNextPlayerIndex(int? currentIndex, bool noBots, int maxPlayers, Func<int, (bool InGame, bool IsBot)> slot)
    {
        var pNum = currentIndex ?? -1;
        var next = GetNextPlayerNumber(pNum, noBots, maxPlayers, slot);
        return next >= 0 ? next : null;
    }
}
