namespace HCDE.Client;

/// <summary>
/// <c>CommandDrawNumber::Parse</c> value keyword tail and validation from <c>sbarinfo_commands.cpp</c> (**1051–1200**).
/// Chunk **1080**; follows **1079** draw-number head through <c>armor</c> (**1050**). Global index bounds delegate to **1078**.
/// </summary>
public static class VmSbarinfoCommandDrawNumber1080Slice
{
    public enum DrawNumberValueKind
    {
        Ammo1,
        Ammo2,
        Ammo1Capacity,
        Ammo2Capacity,
        Score,
        Ammo,
        AmmoCapacity,
        Frags,
        Kills,
        Monsters,
        Items,
        TotalItems,
        Secrets,
        TotalSecrets,
        ArmorClass,
        SavePercent,
        AirTime,
        Accuracy,
        Stamina,
        Keys,
        GlobalVar,
        GlobalArray,
        PowerupTime,
        IntCVar,
        Inventory
    }

    public enum IntCVarAcceptedType
    {
        Bool,
        Int,
        Rejected
    }

    public static (bool Recognized, DrawNumberValueKind Kind) TryClassifyDrawNumberValueKeyword(string identifier)
    {
        if (identifier.Equals("ammo1", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.Ammo1);
        if (identifier.Equals("ammo2", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.Ammo2);
        if (identifier.Equals("ammo1capacity", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.Ammo1Capacity);
        if (identifier.Equals("ammo2capacity", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.Ammo2Capacity);
        if (identifier.Equals("score", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.Score);
        if (identifier.Equals("ammo", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.Ammo);
        if (identifier.Equals("ammocapacity", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.AmmoCapacity);
        if (identifier.Equals("frags", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.Frags);
        if (identifier.Equals("kills", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.Kills);
        if (identifier.Equals("monsters", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.Monsters);
        if (identifier.Equals("items", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.Items);
        if (identifier.Equals("totalitems", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.TotalItems);
        if (identifier.Equals("secrets", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.Secrets);
        if (identifier.Equals("totalsecrets", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.TotalSecrets);
        if (identifier.Equals("armorclass", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.ArmorClass);
        if (identifier.Equals("savepercent", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.SavePercent);
        if (identifier.Equals("airtime", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.AirTime);
        if (identifier.Equals("accuracy", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.Accuracy);
        if (identifier.Equals("stamina", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.Stamina);
        if (identifier.Equals("keys", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.Keys);
        if (identifier.Equals("globalvar", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.GlobalVar);
        if (identifier.Equals("globalarray", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.GlobalArray);
        if (identifier.Equals("poweruptime", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.PowerupTime);
        if (identifier.Equals("intcvar", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.IntCVar);
        return (false, DrawNumberValueKind.Inventory);
    }

    public static bool ShouldRequireClassNameToken(bool parenthesized, bool sawStringConst) =>
        !parenthesized || !sawStringConst;

    public static bool ShouldUseDefaultAmmoClassFallback(bool actorFound, bool isAmmoKind) =>
        !actorFound || !isAmmoKind;

    public static bool ShouldUseDefaultPowerupGiverFallback(bool actorFound, bool isPowerupGiverKind) =>
        !actorFound || !isPowerupGiverKind;

    public static bool IsDrawNumberGlobalIndexInRange(int index) =>
        VmSbarinfoCommandDrawString1078Slice.IsGlobalVarIndexInRange(index);

    public static IntCVarAcceptedType ResolveDrawNumberIntCVarAcceptance(bool cvarExists, bool isBool, bool isInt)
    {
        if (!cvarExists)
            return IntCVarAcceptedType.Rejected;
        if (isBool)
            return IntCVarAcceptedType.Bool;
        if (isInt)
            return IntCVarAcceptedType.Int;
        return IntCVarAcceptedType.Rejected;
    }

    public static bool ShouldValidateInventoryItemClass(DrawNumberValueKind resolvedKind) =>
        resolvedKind == DrawNumberValueKind.Inventory;

    public static bool ShouldUseDefaultInventoryClassFallback(bool actorFound, bool isInventoryKind) =>
        !actorFound || !isInventoryKind;
}
