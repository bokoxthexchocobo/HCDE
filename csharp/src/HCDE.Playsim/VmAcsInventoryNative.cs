namespace HCDE.Playsim;

/// <summary>Inventory helpers from <c>p_acs.cpp</c> (1723–1866).</summary>
public static class VmAcsInventoryNative
{
    public const int CfTotallyFrozen = 1 << 0;

    public static bool DoUseInvNative(
        bool foundItem,
        bool isPlayer,
        int playerCheats,
        Func<bool> useInventory,
        out int cheatsAfterUse)
    {
        if (!foundItem)
        {
            cheatsAfterUse = playerCheats;
            return false;
        }

        if (!isPlayer)
        {
            cheatsAfterUse = playerCheats;
            return useInventory();
        }

        var saved = playerCheats;
        var unfrozen = saved & ~CfTotallyFrozen;
        var res = useInventory();
        cheatsAfterUse = unfrozen | (saved & CfTotallyFrozen);
        return res;
    }

    public static int UseInventoryNative(
        string? type,
        bool activatorIsNull,
        Func<string, bool> tryFindActorClass,
        Func<bool> useOnActivator,
        Func<Func<bool>, int> useOnEachPlayer)
    {
        if (type is null)
            return 0;
        if (!tryFindActorClass(type))
            return 0;

        return activatorIsNull ? useOnEachPlayer(useOnActivator) : (useOnActivator() ? 1 : 0);
    }

    public static int AcsPlayerToInternalNative(int playernum, Func<int, int> toInternal) =>
        playernum < 0 ? playernum : toInternal(playernum);

    public static int InternalPlayerToAcsNative(int playernum, Func<int, int> toVisible) =>
        playernum < 0 ? playernum : toVisible(playernum);

    public static bool ResolveAcsPlayerNative(
        int playernum,
        Func<int, int> toInternal,
        int maxPlayers,
        Func<int, bool> isReservedSlot,
        Func<int, bool> playerInGame,
        out int internalPlayer)
    {
        internalPlayer = AcsPlayerToInternalNative(playernum, toInternal);
        return internalPlayer >= 0
               && internalPlayer < maxPlayers
               && !isReservedSlot(internalPlayer)
               && playerInGame(internalPlayer);
    }

    public static string NormalizeInventoryTypeName(string type) =>
        string.Equals(type, "Armor", StringComparison.OrdinalIgnoreCase) ? "BasicArmor" : type;

    public static int CheckInventoryNative(
        bool activatorMissing,
        string? type,
        bool max,
        Func<int> getMaxHealth,
        Func<int> getHealth,
        Func<string, bool> isUnknownClass,
        Func<string, bool> isNotInventory,
        Func<int> getItemAmount,
        Func<int> getItemMaxAmount,
        Func<int> getDefaultMaxAmount)
    {
        if (activatorMissing || type is null)
            return 0;

        if (string.Equals(type, "Health", StringComparison.OrdinalIgnoreCase))
            return max ? getMaxHealth() : getHealth();

        if (isUnknownClass(type) || isNotInventory(type))
            return 0;

        if (max)
        {
            var itemAmount = getItemAmount();
            return itemAmount > 0 ? getItemMaxAmount() : getDefaultMaxAmount();
        }

        return getItemAmount();
    }
}
