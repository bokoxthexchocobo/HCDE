namespace HCDE.Client;

/// <summary>
/// <c>CommandDrawNumber::Tick</c> value dispatch tail and draw-value interpolation from <c>sbarinfo_commands.cpp</c> (**1351–1500**).
/// Chunk **1082**; follows **1081** <c>AMMO1CAPACITY</c> null-check seam (**1350**).
/// </summary>
public static class VmSbarinfoCommandDrawNumber1082Slice
{
    public const int TicRate = 35;

    public static int ResolveParameterizedAmmoCapacityMax(bool itemFound, int itemMaxAmount, int defaultTypeMaxAmount) =>
        itemFound ? itemMaxAmount : defaultTypeMaxAmount;

    public static int ResolveDrawNumberPrimaryLevelStat(
        VmSbarinfoCommandDrawNumber1080Slice.DrawNumberValueKind kind,
        int fragCount,
        int killedMonsters,
        int totalMonsters,
        int foundItems,
        int totalItems,
        int foundSecrets,
        int totalSecrets,
        int score) =>
        kind switch
        {
            VmSbarinfoCommandDrawNumber1080Slice.DrawNumberValueKind.Frags => fragCount,
            VmSbarinfoCommandDrawNumber1080Slice.DrawNumberValueKind.Kills => killedMonsters,
            VmSbarinfoCommandDrawNumber1080Slice.DrawNumberValueKind.Monsters => totalMonsters,
            VmSbarinfoCommandDrawNumber1080Slice.DrawNumberValueKind.Items => foundItems,
            VmSbarinfoCommandDrawNumber1080Slice.DrawNumberValueKind.TotalItems => totalItems,
            VmSbarinfoCommandDrawNumber1080Slice.DrawNumberValueKind.Secrets => foundSecrets,
            VmSbarinfoCommandDrawNumber1080Slice.DrawNumberValueKind.TotalSecrets => totalSecrets,
            VmSbarinfoCommandDrawNumber1080Slice.DrawNumberValueKind.Score => score,
            _ => 0
        };

    public static int ComputeDrawNumberArmorClassOrSavePercent(
        double hexenSlotSum,
        bool basicArmorActive,
        double basicSavePercent,
        VmSbarinfoCommandDrawNumber1080Slice.DrawNumberValueKind kind)
    {
        var add = hexenSlotSum;
        if (basicArmorActive)
            add += basicSavePercent * 100;
        if (kind == VmSbarinfoCommandDrawNumber1080Slice.DrawNumberValueKind.ArmorClass)
            add /= 5;
        return (int)add;
    }

    public static int ConvertPowerupEffectTicsToDisplaySeconds(int effectTics, int ticRate) =>
        effectTics < 0 ? 0 : effectTics / ticRate + 1;

    public static int ResolveDrawNumberAirTimeTickNum(
        int waterLevel,
        int airSupply,
        int mapTime,
        int airFinished,
        int ticRate)
    {
        if (waterLevel < 3)
            return airSupply / ticRate;
        var ticksUntilOut = airFinished - mapTime + (ticRate - 1);
        return Math.Clamp(ticksUntilOut / ticRate, 0, int.MaxValue);
    }

    public static (bool Updated, int Num) ResolveSelectedInventoryTickNum(
        bool hasSelectedInventory,
        int selectedAmount,
        int priorNum) =>
        hasSelectedInventory ? (true, selectedAmount) : (false, priorNum);

    public static int FoldDrawNumberKeyCount(int priorCount, bool inventoryItemIsKey) =>
        inventoryItemIsKey ? priorCount + 1 : priorCount;

    public static int ResolveDrawNumberIntCVarTickNum(
        bool cvarExists,
        VmSbarinfoCommandDrawNumber1080Slice.IntCVarAcceptedType acceptedType,
        int intRepresentation)
    {
        if (!cvarExists || acceptedType == VmSbarinfoCommandDrawNumber1080Slice.IntCVarAcceptedType.Rejected)
            return 0;
        return intRepresentation;
    }

    public static bool ShouldApplyDrawNumberInterpolation(
        int interpolationSpeed,
        bool hudChanged,
        int levelTime) =>
        interpolationSpeed != 0 && (!hudChanged || levelTime == 1);

    public static int StepDrawNumberInterpolationTowardTarget(
        int drawValue,
        int targetNum,
        int interpolationSpeed)
    {
        if (drawValue > targetNum)
            return drawValue - Math.Clamp((drawValue - targetNum) >> 2, 1, interpolationSpeed);
        if (drawValue < targetNum)
            return drawValue + Math.Clamp((targetNum - drawValue) >> 2, 1, interpolationSpeed);
        return drawValue;
    }

    public static int ResolveDrawNumberDisplayValueAfterTick(
        int drawValue,
        int targetNum,
        int interpolationSpeed,
        bool hudChanged,
        int levelTime)
    {
        if (!ShouldApplyDrawNumberInterpolation(interpolationSpeed, hudChanged, levelTime))
            return targetNum;
        return StepDrawNumberInterpolationTowardTarget(drawValue, targetNum, interpolationSpeed);
    }
}
