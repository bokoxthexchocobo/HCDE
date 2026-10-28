namespace HCDE.Client;

/// <summary>
/// <c>CommandDrawBar::Parse</c> tail, <c>Reset</c>, and <c>Tick</c> value head from <c>sbarinfo_commands.cpp</c> (**2551–2700**).
/// Chunk **1090**; **1089** owns ammo validation (**2553–2556**) and draw clip helpers.
/// </summary>
public static class VmSbarinfoCommandDrawBar1090Slice
{
    public enum DrawBarParseFlag
    {
        None,
        Reverse,
        Interpolate,
        Unknown
    }

    public static (bool Recognized, DrawBarParseFlag Flag) TryClassifyDrawBarParseFlagKeyword(string identifier)
    {
        if (identifier.Equals("reverse", StringComparison.OrdinalIgnoreCase))
            return (true, DrawBarParseFlag.Reverse);
        if (identifier.Equals("interpolate", StringComparison.OrdinalIgnoreCase))
            return (true, DrawBarParseFlag.Interpolate);
        return (false, DrawBarParseFlag.Unknown);
    }

    public static (bool ContinueFlagLoop, bool RequireCommaBeforeCoordinates) ResolveDrawBarParseFlagListContinuation(
        bool nextTokenIsPipe) =>
        nextTokenIsPipe
            ? (ContinueFlagLoop: true, RequireCommaBeforeCoordinates: false)
            : (ContinueFlagLoop: false, RequireCommaBeforeCoordinates: true);

    public static (bool Recognized, bool Horizontal, bool UnknownDirection) ClassifyDrawBarOrientationKeyword(
        string identifier)
    {
        if (identifier.Equals("horizontal", StringComparison.OrdinalIgnoreCase))
            return (true, true, false);
        if (identifier.Equals("vertical", StringComparison.OrdinalIgnoreCase))
            return (true, false, false);
        return (false, false, true);
    }

    public static bool ShouldReplaceDrawBarPowerupGiverClass(bool classFound, bool isPowerupGiverKind) =>
        !classFound || !isPowerupGiverKind;

    public static bool ShouldReplaceDrawBarGenericInventoryClass(bool classFound, bool isInventoryKind) =>
        !classFound || !isInventoryKind;

    public static bool ApplyDrawBarReverseWhenBorderNonZero(int border, bool reverse) =>
        border != 0 ? !reverse : reverse;

    public static int ResolveDrawBarInterpolationSpeedForType(
        VmSbarinfoCommandDrawBar1089Slice.DrawBarValueType type,
        bool scriptInterpolateHealth,
        int scriptHealthInterpolationSpeed,
        bool scriptInterpolateArmor,
        int scriptArmorInterpolationSpeed,
        int parsedInterpolationSpeed) =>
        type switch
        {
            VmSbarinfoCommandDrawBar1089Slice.DrawBarValueType.Health =>
                scriptInterpolateHealth ? scriptHealthInterpolationSpeed : parsedInterpolationSpeed,
            VmSbarinfoCommandDrawBar1089Slice.DrawBarValueType.Armor =>
                scriptInterpolateArmor ? scriptArmorInterpolationSpeed : parsedInterpolationSpeed,
            _ => parsedInterpolationSpeed
        };

    public static (double DrawValue, int Pixel) ApplyCommandDrawBarResetState(
        double currentDrawValue,
        int currentPixel) => (0.0, currentPixel);

    public static double ClampDrawBarDisplayedHealth(double health) => health < 0 ? 0 : health;

    public static int ResolveDrawBarHealthTickMax(
        bool useMaximumConstant,
        int maximumConstant,
        bool comparerItemFound,
        int comparerItemAmount,
        int classMaxHealth)
    {
        if (useMaximumConstant)
            return maximumConstant;
        if (comparerItemFound)
            return comparerItemAmount;
        return classMaxHealth;
    }

    public static int ResolveDrawBarArmorTickValue(bool armorPresent, int armorAmount) =>
        armorPresent ? armorAmount : 0;

    public static int ResolveDrawBarArmorTickMax(
        bool useMaximumConstant,
        int maximumConstant,
        bool comparerItemFound,
        int comparerItemAmount,
        int defaultMaxWhenNoComparer) =>
        useMaximumConstant
            ? maximumConstant
            : comparerItemFound
                ? comparerItemAmount
                : defaultMaxWhenNoComparer;

    public static (double Value, int Max) ResolveDrawBarAmmoSlotTick(
        bool ammoTypeMissing,
        double ammoCount,
        int maxAmountWhenPresent)
    {
        if (ammoTypeMissing)
            return (0, 1);
        return (ammoCount, maxAmountWhenPresent);
    }
}
